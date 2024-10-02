using Microsoft.AspNetCore.Mvc;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class UserMenuController : Controller
    {
        private readonly ILogger<UserMenuController> _logger;
        public UserMenuController(ILogger<UserMenuController> logger) {
            _logger = logger;
        }
        public IActionResult Dashboard()
        {
            var name = HttpContext.Session.GetString("SessionUsername");
            ViewData["name"] = name;
            //_logger.LogInformation("Session Name: {Username}", name);
            Console.WriteLine(name);
            return View();
        }
        public IActionResult AssociationDues()
        {
            return View();
        }


        public IActionResult Logs()
        {
            return View();
        }


        public IActionResult VisitorsList()
        {
            return View();
        }


        public IActionResult VehiclesList()
        {
            return View();
        }


        public IActionResult HomeList()
        {
            return View();
        }
       
    }
}
