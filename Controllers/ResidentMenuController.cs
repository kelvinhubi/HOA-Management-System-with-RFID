using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.InfoSec;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class ResidentMenuController : Controller
    {
        private readonly AppDbContext _db;
        private readonly EnvironmentModel _env;
        public ResidentMenuController( AppDbContext db, IOptions<EnvironmentModel> env)
        {
            _db = db;
            _env = env.Value;
        }
        public IActionResult Dashboard()
        {
            if (CheckRole()) { 
                ViewData["JoinedHomes"] = _db.homeRequests.Where(_ => _.ResidentID == Convert.ToInt32(HttpContext.Session.GetString("SessionID"))).Count();
                ViewData["Vehicles"] = _db.Vehicle_Information.Where(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID"))).Count();
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        //Announcements
        public IActionResult Announcements() { 
            if (CheckRole()) {
                return View(_db.Announcements.ToList());
            }
            return RedirectToAction("AccessDenied", "ResidentMenu");
        }
        //HomesList
        public IActionResult HomeList() {
            if (CheckRole())
            {
                NewModel model = new NewModel();
                model.Homes = _db.homesLists.ToList();
                model.HomeRequests = _db.homeRequests.Where(_ => _.ResidentID == Convert.ToInt32(HttpContext.Session.GetString("SessionID"))).ToList();
                return View(model);

            }
            return RedirectToAction("AccessDenied", "ResidentMenu");//Must change to its main controller
        }
        //Vehicles List
        public IActionResult VehiclesList()
        {
            if (CheckRole())
            {
                var result = _db.Vehicle_Information.Where(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
                if (result != null)
                {
                    return View(result);
                }
                return View();
            }
            return RedirectToAction("AccessDenied", "ResidentMenu");
        }


        //Profile
       public IActionResult _ChangeUserPass()
        {
            if (CheckRole()) {
                var result = _db.User_Accounts.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
                return View(result);
            }
            return RedirectToAction("AccessDenied", "ResidentMenu");
        }
        public IActionResult _ChangeDetails()
        {
            if (CheckRole())
            {
                var result = _db.Homeowner_Details.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
                return View(result);
            }
            return RedirectToAction("AccessDenied", "ResidentMenu");
        }
        [HttpPost]
        public JsonResult UpdateDetails(Homeowner_details info)
        {
            if (ModelState.IsValid)
            {
                _db.Homeowner_Details.Update(info);
                _db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public JsonResult UpdateProfile(int AccountID, string Password, string Username)
        {
            if (ModelState.IsValid)
            {
                var result = _db.User_Accounts.Where(_ => _.AccountID == AccountID).ToList().FirstOrDefault();
                var result1 = _db.Homeowner_Details.Where(_ => _.AccountID == AccountID).ToList().FirstOrDefault();
                if (result1 != null && result != null)
                {
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
        public IActionResult JoinHome(int? id) {
            if (id == null) {
                return NotFound();
            }
            var info = _db.homesLists.FirstOrDefault(_ => _.HomeID == id);
            if (info == null) { return NotFound(); }
            var result = new HomeRequest
            {
                HomeID = info.HomeID,
                AccountID = info.AccountID,
                ResidentID = Convert.ToInt32(HttpContext.Session.GetString("SessionID")),
                HomeName = info.HomeName,
                FullName = info.FullName,
                BlkNO = info.BlkNO,
                LotNo = info.LotNo,
                Address = info.Address,
                Status = "Requested"
            };
            _db.homeRequests.Update(result);
            _db.SaveChanges();
            return RedirectToAction("HomeList","ResidentMenu");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        public bool CheckRole()
        {
            var usertype = HttpContext.Session.GetString("UserType");
            Console.WriteLine(usertype);
            if (usertype != null)
            {
                if (usertype == "Resident")
                {
                    return true;

                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public IActionResult AccessDenied() {
            return View();
        }
    }
}
