
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.InfoSec;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class UserMenuController : Controller
    {
        private readonly ILogger<UserMenuController> _logger;
        private readonly AppDbContext _db;
        private readonly EnvironmentModel _env;
        public UserMenuController(ILogger<UserMenuController> logger, AppDbContext db,IOptions<EnvironmentModel> env) {
            _logger = logger;
            _db = db;
            _env = env.Value;
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
		public IActionResult _ChangeUserPass()
		{
			var result = _db.User_Accounts.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
			return View(result);
		}
		[HttpPost]
		public JsonResult UpdateProfile(int AccountID, string Password, string Username)
		{
			if (ModelState.IsValid)
			{
                var result = _db.User_Accounts.Where(_ => _.AccountID == AccountID).ToList().FirstOrDefault();
                var result1 = _db.Homeowner_Details.Where(_ => _.AccountID == AccountID).ToList().FirstOrDefault();
                if (result1 != null && result != null) {
                    try
                    {
                        if (Encryption.Decrypt(Password, _env.EncryptionKey, _env.IVKey).ToString().Equals(Encryption.Decrypt(result.Password, _env.EncryptionKey, _env.IVKey).ToString()))

					    {
							result.Password = Encryption.Encrpyt(Encryption.Decrypt(Password, _env.EncryptionKey, _env.IVKey), _env.EncryptionKey, _env.IVKey);
						}
					}
                    catch (Exception)
                    {
                        result.Password = Encryption.Encrpyt(Password, _env.EncryptionKey, _env.IVKey);

					}
                    result.Username = Username;
                    result1.Username = Username;
                    _db.User_Accounts.Update(result);
                    _db.Homeowner_Details.Update(result1);
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
				
			}
			return Json(new { success = false });
		}

	}
}
