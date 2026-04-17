using System.Diagnostics;
using Airline_Ticket_System.Models;
using Microsoft.AspNetCore.Mvc;

namespace Airline_Ticket_System.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // If user is logged in, redirect them to flights page
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Flight");
            }
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
