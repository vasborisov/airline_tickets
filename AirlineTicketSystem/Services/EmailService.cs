using Airline_Ticket_System.Configurations;
using Airline_Ticket_System.Entities;
using Airline_Ticket_System.Models.Email;
using Airline_Ticket_System.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Airline_Ticket_System.Services;

/// <summary>Professional email service with HTML template support and SMTP sending.</summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _environment;
    
    // Rate limiting
    private readonly Dictionary<string, List<DateTime>> _emailHistory = new();
    private readonly object _lockObject = new object();

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger,
        IServiceProvider serviceProvider,
        IWebHostEnvironment environment)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task SendWelcomeAsync(string toEmail, string displayName, CancellationToken cancellationToken = default)
    {
        var model = new
        {
            DisplayName = displayName,
            Email = toEmail,
            LoginUrl = GetBaseUrl() + "/Account/Login"
        };

        await SendEmailAsync(
            toEmail,
            "🎉 Добре дошли в Airline Ticket System!",
            "WelcomeRegistration",
            "Account",
            model,
            cancellationToken);
    }

    public async Task SendOperatorCreatedAsync(string toEmail, string displayName, CancellationToken cancellationToken = default)
    {
        var model = new
        {
            DisplayName = displayName,
            Email = toEmail,
            LoginUrl = GetBaseUrl() + "/Account/Login"
        };

        await SendEmailAsync(
            toEmail,
            "👨‍💼 Създаден е акаунт на оператор",
            "OperatorAccountCreated",
            "Account",
            model,
            cancellationToken);
    }

    public async Task SendAccountActiveChangedAsync(string toEmail, string displayName, bool isActive, CancellationToken cancellationToken = default)
    {
        var model = new
        {
            DisplayName = displayName,
            Email = toEmail,
            IsActive = isActive,
            LoginUrl = GetBaseUrl() + "/Account/Login"
        };

        var subject = isActive ? "✅ Акаунтът ви е активиран" : "⚠️ Акаунтът ви е деактивиран";

        await SendEmailAsync(
            toEmail,
            subject,
            "AccountStatusChanged",
            "Account",
            model,
            cancellationToken);
    }

    public async Task SendBookingConfirmationAsync(string toEmail, string pnr, Flight flight, Passenger passenger, CancellationToken cancellationToken = default)
    {
        if (flight == null)
            throw new ArgumentNullException(nameof(flight), "Flight cannot be null for booking confirmation email");
        
        if (passenger == null)
            throw new ArgumentNullException(nameof(passenger), "Passenger cannot be null for booking confirmation email");

        _logger.LogInformation("Sending booking confirmation email to {Email} for PNR {PNR}. Flight: {FlightNumber}, Passenger: {PassengerName}", 
            toEmail, pnr, flight.FlightNumber, $"{passenger.FirstName} {passenger.FamilyName}");
            
        var model = new BookingConfirmationEmailModel
        {
            PNR = pnr ?? string.Empty,
            Flight = flight,
            Passenger = passenger,
            PaymentAmount = null,
            PaymentStatus = "Confirmed",
            BookingDetailsUrl = GetBaseUrl() + $"/Booking/ByPnr?pnr={Uri.EscapeDataString(pnr ?? string.Empty)}"
        };

        await SendEmailAsync(
            toEmail,
            $"🎫 Потвърждение на резервация - PNR: {pnr}",
            "BookingConfirmation",
            "Booking",
            model,
            cancellationToken);
    }

    public async Task SendBookingCancelledAsync(string toEmail, string pnr, decimal? refundAmount, Flight? flight = null, CancellationToken cancellationToken = default)
    {
        var model = new
        {
            PNR = pnr,
            RefundAmount = refundAmount,
            Flight = flight,
            NewBookingUrl = GetBaseUrl() + "/Flight"
        };

        await SendEmailAsync(
            toEmail,
            $"❌ Отмяна на резервация - PNR: {pnr}",
            "BookingCancelled",
            "Booking",
            model,
            cancellationToken);
    }

    public async Task SendFlightScheduleChangedAsync(string toEmail, Flight flight, DateTime? originalDepartureTime = null, CancellationToken cancellationToken = default)
    {
        var model = new
        {
            Flight = flight,
            OriginalDepartureTime = originalDepartureTime,
            FlightDetailsUrl = GetBaseUrl() + $"/Flight/Details/{flight.Id}"
        };

        var subject = flight.Status == "Cancelled" 
            ? $"❌ Отмяна на полет {flight.FlightNumber}"
            : $"⏰ Промяна в полет {flight.FlightNumber}";

        await SendEmailAsync(
            toEmail,
            subject,
            "FlightScheduleChanged",
            "Flight",
            model,
            cancellationToken);
    }

    public async Task SendFlightReminderAsync(string toEmail, string pnr, Flight flight, Passenger passenger, CancellationToken cancellationToken = default)
    {
        var model = new
        {
            PNR = pnr,
            Flight = flight,
            Passenger = passenger,
            CheckinUrl = GetBaseUrl() + $"/Booking/Checkin/{pnr}" // Future feature
        };

        await SendEmailAsync(
            toEmail,
            $"✈️ Напомняне за полет {flight.FlightNumber} утре",
            "FlightReminder",
            "Booking",
            model,
            cancellationToken);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string templateName, string folder, object model, CancellationToken cancellationToken = default)
    {
        try
        {
            // Rate limiting check
            if (!IsWithinRateLimit(toEmail))
            {
                _logger.LogWarning("Email rate limit exceeded for {Email}. Skipping email: {Subject}", toEmail, subject);
                return;
            }

            // In development, just log the email instead of sending
            if (_environment.IsDevelopment() || string.IsNullOrEmpty(_emailSettings.SmtpServer))
            {
                _logger.LogInformation("DEVELOPMENT EMAIL to {Email}: {Subject} (Template: {Template})", 
                    toEmail, subject, $"{folder}/{templateName}");
                
                // Still render the template for testing
                var htmlContent = await RenderEmailTemplateAsync($"{folder}/{templateName}", model);
                _logger.LogDebug("Email HTML Content Length: {Length} characters", htmlContent?.Length ?? 0);
            }

            // Render email template
            var emailBody = await RenderEmailTemplateAsync($"{folder}/{templateName}", model);
            if (string.IsNullOrEmpty(emailBody))
            {
                _logger.LogError("Failed to render email template {Template} for {Email}", $"{folder}/{templateName}", toEmail);
                return;
            }

            // Send email via SMTP
            await SendSmtpEmailAsync(toEmail, subject, emailBody, cancellationToken);
            
            // Track sent email for rate limiting
            TrackSentEmail(toEmail);
            
            _logger.LogInformation("Email sent successfully to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            throw;
        }
    }

    private async Task<string?> RenderEmailTemplateAsync(string templateName, object model)
    {
        try
        {
            _logger.LogDebug("Rendering email template: {Template} with model type: {ModelType}", templateName, model?.GetType().Name);
            
            using var scope = _serviceProvider.CreateScope();
            var httpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
            
            var viewEngine = scope.ServiceProvider.GetRequiredService<ICompositeViewEngine>();
            var tempDataProvider = scope.ServiceProvider.GetRequiredService<ITempDataProvider>();
            
            // Fix Windows path separators - replace backslashes with forward slashes
            var templatePath = $"~/Views/EmailTemplates/{templateName.Replace('\\', '/')}.cshtml";
            
            _logger.LogDebug("Looking for template at path: {TemplatePath}", templatePath);
            
            var viewResult = viewEngine.GetView(null, templatePath, false);

            if (!viewResult.Success)
            {
                _logger.LogError("Email template not found: {Template} at path {TemplatePath}. Searched locations: {SearchedLocations}", 
                    templateName, templatePath, string.Join(", ", viewResult.SearchedLocations ?? Array.Empty<string>()));
                return null;
            }

            using var stringWriter = new StringWriter();
            
            // Create ViewDataDictionary with object type for better compatibility
            var viewDictionary = new ViewDataDictionary<object>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };
            
            var tempData = new TempDataDictionary(httpContext, tempDataProvider);
            var viewContext = new ViewContext(actionContext, viewResult.View, viewDictionary, tempData, stringWriter, new HtmlHelperOptions());

            _logger.LogDebug("Rendering view for template: {Template}", templateName);
            await viewResult.View.RenderAsync(viewContext);
            
            var result = stringWriter.ToString();
            _logger.LogDebug("Successfully rendered template: {Template}. Content length: {Length}", templateName, result.Length);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering email template {Template}. Model: {@Model}", templateName, model);
            return null;
        }
    }

    private async Task SendSmtpEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
        {
            EnableSsl = _emailSettings.EnableSsl,
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.AppPassword),
            Timeout = 30000 // 30 seconds timeout
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        mailMessage.To.Add(toEmail);
        
        // Add a text alternative for email clients that don't support HTML
        var textBody = System.Text.RegularExpressions.Regex.Replace(htmlBody, "<[^>]*>", "");
        var textView = AlternateView.CreateAlternateViewFromString(textBody, Encoding.UTF8, "text/plain");
        var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
        
        mailMessage.AlternateViews.Add(textView);
        mailMessage.AlternateViews.Add(htmlView);

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }

    private bool IsWithinRateLimit(string email)
    {
        if (_emailSettings.MaxEmailsPerHour <= 0) return true;

        lock (_lockObject)
        {
            var now = DateTime.UtcNow;
            var oneHourAgo = now.AddHours(-1);

            // Initialize or clean up email history for this address
            if (!_emailHistory.ContainsKey(email))
            {
                _emailHistory[email] = new List<DateTime>();
            }
            else
            {
                // Remove emails older than 1 hour
                _emailHistory[email].RemoveAll(dt => dt < oneHourAgo);
            }

            // Check if under rate limit
            if (_emailHistory[email].Count >= _emailSettings.MaxEmailsPerHour)
            {
                return false;
            }

            return true;
        }
    }

    private void TrackSentEmail(string email)
    {
        lock (_lockObject)
        {
            if (!_emailHistory.ContainsKey(email))
            {
                _emailHistory[email] = new List<DateTime>();
            }
            
            _emailHistory[email].Add(DateTime.UtcNow);
        }
    }

    private string GetBaseUrl()
    {
        // In a real application, this should come from configuration
        return _environment.IsDevelopment() 
            ? "http://localhost:5090" 
            : "https://your-production-domain.com";
    }
}