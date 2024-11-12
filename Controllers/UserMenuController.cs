
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.InfoSec;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
                model.ListDues = _db.Due_Details.Where(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID"))).ToList(); 
                return View(model); }
            return RedirectToAction("AccessDenied", "Home");
        }
        public IActionResult AssociationDues()
        {
            if (CheckRole()) {
                NewModel model = new NewModel();
                model.ListDues = _db.Due_Details.Where(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID"))).ToList();
                return View(model);
            }

            return RedirectToAction("AccessDenied", "Home");
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
        public async Task<JsonResult> CheckPayment(string id){
            await Task.Delay(5000);
            var options = new RestClientOptions("https://api.paymongo.com/v1/links/"+id);
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
