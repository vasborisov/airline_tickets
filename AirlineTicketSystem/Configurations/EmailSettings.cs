namespace Airline_Ticket_System.Configurations;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";

    public int Port { get; set; } = 587;

    public string SenderEmail { get; set; } = "";

    public string SenderName { get; set; } = "Airline Ticket System";

    public string Username { get; set; } = "";

    public string AppPassword { get; set; } = "";

    public bool EnableSsl { get; set; } = true;

    public int MaxEmailsPerHour { get; set; } = 100;
}
