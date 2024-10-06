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
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Shared");
        }
        public IActionResult AssociationDues()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Shared");
        }


        public IActionResult Logs()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Shared");
        }


        public IActionResult VisitorsList()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Shared");
        }


        public IActionResult VehiclesList()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Shared");
        }


        public IActionResult HomeList()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Shared");
        }

        public bool CheckRole()
        {
            var usertype = HttpContext.Session.GetString("UserType");
            Console.WriteLine(usertype);
            if (usertype != null)
            {
                if (usertype == "User")
                {
                    return true;

                }
                else {
                    return false;
                }
            }
            return false;
        }

    }
}
