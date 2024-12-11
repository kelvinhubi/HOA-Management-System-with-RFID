
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.InfoSec;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using NuGet.Protocol;
using RestSharp;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class UserMenuController : Controller
    {
        private readonly ILogger<UserMenuController> _logger;
        private readonly AppDbContext _db;
        private readonly EnvironmentModel _env;
        public UserMenuController(ILogger<UserMenuController> logger, AppDbContext db, IOptions<EnvironmentModel> env) {
            _logger = logger;
            _db = db;
            _env = env.Value;
        }
        public IActionResult Dashboard()
        {
            if (CheckRole()) {
                NewModel model = new NewModel();
                model.Vehicles = _db.Vehicle_Information.Where(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID"))).ToList();
                model.ListDues = _db.Due_Details.Where(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID"))).ToList();
                model.Homes = _db.homesLists.Where(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID"))).ToList();
                return View(model); 
            }
            return RedirectToAction("AccessDenied", "UserMenu");
        }
        public IActionResult AssociationDues()
        {
            if (CheckRole()) {
                NewModel model = new NewModel();
                model.ListDues = _db.Due_Details.Where(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID"))).ToList();
                return View(model);
            }

            return RedirectToAction("AccessDenied", "UserMenu");
        }
        [HttpPost]
        public async Task<JsonResult> Payout(string Amount, string Invoice) {
            var options = new RestClientOptions("https://api.paymongo.com/v1/links");
            var client = new RestClient(options);
            Amount = Amount + "00";
            string jsonstring = "{\"data\":{\"attributes\":{\"amount\":" + Amount + ",\"description\":\"" + Invoice + "\"}}}";
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Basic c2tfdGVzdF9DRlRlTTRDUkdXSGVLdE1TSzFrVkw5VnE6");
            request.AddJsonBody(jsonstring, false);
            var response = await client.PostAsync(request);
            Console.WriteLine("{0}", response.Content);
            string str = response.Content;
            PayoutDetails paydetails = JsonConvert.DeserializeObject<PayoutDetails>(str);
            var pay = paydetails.Data.Attributes.Checkout_url;
            return Json(paydetails);

        }
        [HttpPost]
        public async Task<JsonResult> CheckPayment(string id) {
            await Task.Delay(5000);
            var options = new RestClientOptions("https://api.paymongo.com/v1/links/" + id);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Basic c2tfdGVzdF9DRlRlTTRDUkdXSGVLdE1TSzFrVkw5VnE6");
            var response = await client.GetAsync(request);
            PayoutDetails paydetails = JsonConvert.DeserializeObject<PayoutDetails>(response.Content);
            if (paydetails.Data.Attributes.Status != "unpaid") {
                var result = _db.Due_Details.Where(_ => _.Invoice.Equals(paydetails.Data.Attributes.Description)).ToList().FirstOrDefault();
                if (result != null) {
                    result.Status = "Paid";
                    await _db.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });

        }
        [HttpPost]
        public async Task<JsonResult> CheckPaymentForEvent(string id,string EvID)
        {
            await Task.Delay(5000);
            var options = new RestClientOptions("https://api.paymongo.com/v1/links/" + id);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Basic c2tfdGVzdF9DRlRlTTRDUkdXSGVLdE1TSzFrVkw5VnE6");
            var response = await client.GetAsync(request);
            PayoutDetails paydetails = JsonConvert.DeserializeObject<PayoutDetails>(response.Content);
            if (paydetails.Data.Attributes.Status != "unpaid")
            {
                var result = _db.Homeowner_Details.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
                var EvResult = _db.Events.FirstOrDefault(_ => _.EventID == Convert.ToInt32(EvID));
                if (result != null && EvResult != null)
                {
                    var info = new EventsReserved { AccountID = result.AccountID, EventID = EvResult.EventID, Fee = EvResult.EventFee, Status = "Joined"};
                    _db.EventsReserved.Add(info);
                    await _db.SaveChangesAsync();
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });

        }
        [HttpPost]
        public async Task<JsonResult> CheckPaymentForFacilities(string id)
        {
            await Task.Delay(5000);
            var options = new RestClientOptions("https://api.paymongo.com/v1/links/" + id);
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Basic c2tfdGVzdF9DRlRlTTRDUkdXSGVLdE1TSzFrVkw5VnE6");
            var response = await client.GetAsync(request);
            PayoutDetails paydetails = JsonConvert.DeserializeObject<PayoutDetails>(response.Content);
            if (paydetails.Data.Attributes.Status != "unpaid")
            {
                    return Json(new { success = true });
            }
            return Json(new { success = false });

        }
        public IActionResult Announcements() {
            if (CheckRole()) {

                var result = _db.Announcements.ToList();
                if (result != null)
                {
                    return View(result);
                }
                return View();
            }
            return RedirectToAction("AccessDenied", "UserMenu");
        }

        //VehicleList
        public IActionResult VehiclesList()
        {
            if (CheckRole()) {
                var result = _db.Vehicle_Information.Where(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
                if (result != null)
                {
                    return View(result);
                }
                return View();
            }
            return RedirectToAction("AccessDenied", "UserMenu");
        }

        //HomeList
        public IActionResult HomeList()
        {
            if (CheckRole()) {
                NewModel model = new NewModel();
                model.Homes = _db.homesLists.ToList();
                model.Homeacc = _db.Homeowner_Details.ToList();
                model.HomeRequests = _db.homeRequests.Where(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
                return View(model);
            }
            return RedirectToAction("AccessDenied", "UserMenu");
        }

        public IActionResult _AcceptRequest(int? id) {
            if (id == null) {
                return NotFound();
            }
            var info = _db.homeRequests.FirstOrDefault(_ => _.HomeReqID == id);
            if (info == null) { return NotFound(); }
            info.Status = "Approved";
            _db.homeRequests.Update(info);
            _db.SaveChanges();
            return RedirectToAction("AccessDenied", "UserMenu");
        }

        public IActionResult _RejectRequest(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var info = _db.homeRequests.FirstOrDefault(_ => _.HomeReqID == id);
            if (info == null) { return NotFound();}
            info.Status = "Rejected";
            _db.homeRequests.Update(info);
            _db.SaveChanges();
            return RedirectToAction("HomeList", "UserMenu");
        }

        //MaintenanceRequest
        public IActionResult MaintenanceRequest() { 
            NewModel model = new NewModel();
            model.maintenances = _db.maintenanceRequests.Where(_=>_.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID"))).ToList();
            return View(model);
        }
        public IActionResult _AddRequest(MaintenanceRequests info) {
            info.AccountID = Convert.ToInt32(HttpContext.Session.GetString("SessionID"));
            _db.maintenanceRequests.Add(info);
            _db.SaveChanges();
            return RedirectToAction("MaintenanceRequest");
        }
        public IActionResult _MaintenanceCancel(int? id)
        {
            if (id == null) { 
                return NotFound();
            }
            var result = _db.maintenanceRequests.FirstOrDefault(_ => _.RequestID == id);
            if (result == null) {
                return NotFound();
            }
            _db.maintenanceRequests.Remove(result);
            _db.SaveChanges();
            return RedirectToAction("MaintenanceRequest");
        }
        public IActionResult _MaintenanceDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var result = _db.maintenanceRequests.FirstOrDefault(_ => _.RequestID == id);
            if (result == null)
            {
                return NotFound();
            }

            return PartialView(result);
        }

        //Pets
        public IActionResult Pets() { 
            OwnerPetInfo model = new OwnerPetInfo();
            model.Pets = _db.PetInformation.ToList();
            return View(model); 
        }
        public IActionResult _AddPets(OwnerPetInfo info) {
            foreach (var items in info.Pets) {
                items.AccountID = Convert.ToInt32(HttpContext.Session.GetString("SessionID"));
                _db.PetInformation.Add(items);
            }
            _db.SaveChanges();
            return RedirectToAction("Pets");
        }
        public IActionResult _EditPets(int? id) {
            if (id == null) {return NotFound();}
            var result = _db.PetInformation.FirstOrDefault(_ => _.PetId == id);
            if (result == null) { return NotFound(); }
            return View(result);
        }
        [HttpPost]
        public IActionResult _EditPets(PetInformation info) {
            _db.PetInformation.Update(info);
            _db.SaveChanges();
            return RedirectToAction("Pets","UserMenu");
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

        //Events
        public IActionResult Events() {
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
        public IActionResult JoinFree(int? EvID) {
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

        public IActionResult Facilities() { 
            NewModel model = new NewModel();
            model.facilities = _db.Facilities.ToList();
            model.facilitiesreserved = _db.FacilitiesReserved.ToList();
            model.Homeacc = _db.Homeowner_Details.ToList();
            return View(model);
        }
        public IActionResult _ReserveFacility(int? id) {
            var result = _db.Facilities.FirstOrDefault(_=>_.FacilityID == id);
            if (result == null) { return NotFound(); }
            FacilityReservation info = new FacilityReservation { AccountID = Convert.ToInt32(HttpContext.Session.GetString("SessionID")), FacilityID = result.FacilityID };
            return View(info);
        }
        [HttpPost]
        public IActionResult _ReserveFacility(FacilityReservation info)
        {
            info.PaymentStatus = "Paid";
            var result = _db.FacilitiesReserved.Where(_ => _.FacilityID == info.FacilityID);
            var result2 = result.Where(_ => (_.StartTime <= info.EndTime && _.EndTime >= info.StartTime) || (_.StartTime >= info.StartTime && _.StartTime <= info.EndTime) || (_.EndTime >= info.StartTime && _.EndTime <= info.EndTime)).Count();

            if (result2 != 0) {
                ModelState.AddModelError("Reserved", "Sorry Date and time already reserved");
                return View(info);
            }
            _db.FacilitiesReserved.Add(info);
            _db.SaveChanges();
            return RedirectToAction("Facilities");
        }

        //ViolationSanction
        public IActionResult Violation() {
            return View(_db.Violation.ToList());
        }
        public IActionResult _ReportViolation() { return View(); }
        [HttpPost]
        public IActionResult _ReportViolation(ViolationSanction info) {
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
        public IActionResult RulesRegulations() { return View(); }
        public IActionResult DownloadByLaws()
        {
            string filepath = "wwwroot\\Downloadable\\ByLaws.pdf";
            return File(System.IO.File.ReadAllBytes(filepath), "application/pdf", "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "ByLaws.pdf");
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
            if (CheckRole()) {
                var result = _db.User_Accounts.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
                return View(result);
            }
            return RedirectToAction("AccessDenied", "UserMenu");
        }
        public IActionResult _ChangeDetails()
        {
            if (CheckRole())
            {
                var result = _db.Homeowner_Details.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
                return View(result);
            }
                
            return RedirectToAction("AccessDenied", "UserMenu");
        }
        [HttpPost]
        public JsonResult UpdateDetails(Homeowner_details info) {
			if (ModelState.IsValid) { 
                    _db.Homeowner_Details.Update(info);
                    _db.SaveChanges();
                    return Json(new { success = true });
			}
            return Json(new { success = false});
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
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        //Chat
        public IActionResult Chat()
        {
            return PartialView();
        }

    }
}
