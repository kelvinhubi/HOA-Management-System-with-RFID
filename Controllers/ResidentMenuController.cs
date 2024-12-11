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
        //Events
        public IActionResult Events()
        {
            ViewData["SessionID"] = HttpContext.Session.GetString("SessionID");
            NewModel model = new NewModel();
            model.Events = _db.Events.ToList();
            model.EventsReserved = _db.EventsReserved.ToList();
            return View(model);
        }
        public IActionResult _EventDetails(int? id)
        {
            if (id == null) { return NotFound(); }
            var result = _db.Events.FirstOrDefault(_ => _.EventID == id);
            if (result == null)
            {
                return NotFound();
            }
            return View(result);
        }
        public IActionResult JoinFree(int? EvID)
        {
            var result = _db.Homeowner_Details.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
            var EvResult = _db.Events.FirstOrDefault(_ => _.EventID == Convert.ToInt32(EvID));
            if (result != null && EvResult != null)
            {
                var info = new EventsReserved { AccountID = result.AccountID, EventID = EvResult.EventID, Fee = null, Status = "Joined" };
                _db.EventsReserved.Add(info);
                _db.SaveChanges();
                return RedirectToAction("Events");
            }
            return RedirectToAction("Events");
        }
        //Facility

        public IActionResult Facilities()
        {
            NewModel model = new NewModel();
            model.facilities = _db.Facilities.ToList();
            model.facilitiesreserved = _db.FacilitiesReserved.ToList();
            model.Homeacc = _db.Homeowner_Details.ToList();
            return View(model);
        }
        public IActionResult _ReserveFacility(int? id)
        {
            var result = _db.Facilities.FirstOrDefault(_ => _.FacilityID == id);
            if (result == null) { return NotFound(); }
            FacilityReservation info = new FacilityReservation { AccountID = Convert.ToInt32(HttpContext.Session.GetString("SessionID")), FacilityID = result.FacilityID };
            return View(info);
        }
        [HttpPost]
        public IActionResult _ReserveFacility(FacilityReservation info)
        {
            info.PaymentStatus = "Paid";
            var result = _db.FacilitiesReserved.Where(_ =>_.FacilityID ==info.FacilityID && (_.StartTime <= info.EndTime && _.EndTime >= info.StartTime) || (_.StartTime >= info.StartTime && _.StartTime <= info.EndTime) || (_.EndTime >= info.StartTime && _.EndTime <= info.EndTime));
            if (result.Count() != 0)
            {
                ModelState.AddModelError("Reserved", "Sorry Date and time already reserved");
                return View(info);
            }
            _db.FacilitiesReserved.Add(info);
            _db.SaveChanges();
            return RedirectToAction("Facilities");
        }

        //Pets
        public IActionResult Pets()
        {
            OwnerPetInfo model = new OwnerPetInfo();
            model.Pets = _db.PetInformation.ToList();
            return View(model);
        }
        public IActionResult _AddPets(OwnerPetInfo info)
        {
            foreach (var items in info.Pets)
            {
                items.AccountID = Convert.ToInt32(HttpContext.Session.GetString("SessionID"));
                _db.PetInformation.Add(items);
            }
            _db.SaveChanges();
            return RedirectToAction("Pets");
        }
        public IActionResult _EditPets(int? id)
        {
            if (id == null) { return NotFound(); }
            var result = _db.PetInformation.FirstOrDefault(_ => _.PetId == id);
            if (result == null) { return NotFound(); }
            return View(result);
        }
        [HttpPost]
        public IActionResult _EditPets(PetInformation info)
        {
            _db.PetInformation.Update(info);
            _db.SaveChanges();
            return RedirectToAction("Pets", "ResidentMenu");
        }
        public IActionResult _DeletePets(int? id)
        {
            if (id == null) { return NotFound(); }
            var result = _db.PetInformation.FirstOrDefault(_ => _.PetId == id);
            if (result == null) { return NotFound(); }
            _db.PetInformation.Remove(result);
            _db.SaveChanges();
            return RedirectToAction("Pets");
        }


        //ViolationSanction
        public IActionResult Violation()
        {
            return View(_db.Violation.ToList());
        }
        public IActionResult _ReportViolation() { return View(); }
        [HttpPost]
        public IActionResult _ReportViolation(ViolationSanction info)
        {
            var result = _db.Homeowner_Details.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
            info.AccountID = Convert.ToInt32(HttpContext.Session.GetString("SessionID"));
            info.Complainant = result.FullName;
            _db.Violation.Add(info);
            _db.SaveChanges();
            return RedirectToAction("Violation");
        }
        public IActionResult _EditViolation(int? id)
        {
            var result = _db.Violation.FirstOrDefault(_ => _.ViolationID == id);
            if (result == null) { return NotFound(); }
            return View(result);
        }
        [HttpPost]
        public IActionResult _EditViolation(ViolationSanction info)
        {
            _db.Violation.Update(info);
            _db.SaveChanges();
            return RedirectToAction("Violation");
        }
        public IActionResult _DeleteViolation(int? id)
        {
            var result = _db.Violation.FirstOrDefault(_ => _.ViolationID == id);
            if (result == null) { return NotFound(); }
            _db.Violation.Remove(result);
            _db.SaveChanges();
            return RedirectToAction("Violation");
        }
        //Download ByLaws
        public IActionResult RulesRegulation() { return View(); }
        public IActionResult DownloadByLaws()
        {
            string filepath = "wwwroot\\Downloadable\\file.pdf";
            return File(System.IO.File.ReadAllBytes(filepath), "application/pdf", "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "ByLaws.pdf");
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
        //Events
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
