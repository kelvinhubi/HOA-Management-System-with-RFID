using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class UserMenuController : Controller
    {
        private readonly ILogger<UserMenuController> _logger;
        private readonly AppDbContext _db;
        public UserMenuController(ILogger<UserMenuController> logger, AppDbContext db) {
            _logger = logger;
            _db = db;
        }
        public IActionResult Dashboard()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Home");
        }
        public IActionResult AssociationDues()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Home");
        }

        public IActionResult Announcements() { 
            return View(_db.Announcements.ToList());
        }
        public IActionResult Logs()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Home");
        }


        public IActionResult VisitorsList()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Home");
        }


        public IActionResult VehiclesList()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Home");
        }


        public IActionResult HomeList()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Home");
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
