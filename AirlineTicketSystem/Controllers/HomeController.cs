using Microsoft.AspNetCore.Mvc;

namespace Airline_Ticket_System.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
