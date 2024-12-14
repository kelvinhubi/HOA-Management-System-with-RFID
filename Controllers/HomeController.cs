using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.InfoSec;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;
        private readonly EnvironmentModel _env;
        public HomeController(ILogger<HomeController> logger, AppDbContext db, IOptions<EnvironmentModel> Accessor)
        {
            _logger = logger;
            _db = db;
            _env = Accessor.Value;
            var CheckAdmin = _db.Admin_Accounts.Count();
            var CheckGuard = _db.Guard_Information.Count();
            var CheckSuperAdmin = _db.SuperAdmin_Accounts.Count();
            if (CheckAdmin == 0) {
                _db.Admin_Accounts.Add(new Admin_Account { Username="ADMIN", Password = Encryption.Encrpyt("ADMIN1234", _env.EncryptionKey, _env.IVKey), Email = "krfortin15@gmail.com" });
                _db.SaveChanges();
            }
            if (CheckSuperAdmin == 0)
            {
                _db.SuperAdmin_Accounts.Add(new SuperAdmin { Username = "SuperAdmin", Password = Encryption.Encrpyt("SADMIN1234", _env.EncryptionKey, _env.IVKey)});
                _db.SaveChanges();
            }
        }
        public IActionResult Announcements() {
            var result = _db.Announcements.OrderByDescending(_=>_.DatePosted);
            return View(result); 
        }
        public IActionResult Index()
        {
            return View();
        }
        [Area("Admin")]
        public IActionResult Admin() {
            return RedirectToAction("Admin_LoginForm","Login");
        }
        [Area("Guard")]
        public IActionResult Guard()
        {
            return RedirectToAction("GuardLoginForm", "Login");
        }
        [Area("SAdmin")]
        public IActionResult SuperAdmin()
        {
            return RedirectToAction("SuperAdmin_LoginForm", "Login");
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult AccessDenied() { return View(); }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
