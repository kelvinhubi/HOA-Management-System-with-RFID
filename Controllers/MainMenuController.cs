using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MySqlConnector;
using System.Text;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{

    public class MainMenuController : Controller
    {
        const string connstr = "server=localhost;user=root;password=Kelvinfo14;database=hoa_sys";
        private readonly AppDbContext _db;
        private readonly ILogger _logger;
        MySqlConnection conn = new MySqlConnection(connstr);
        public MainMenuController(AppDbContext db, ILogger<MainMenuController> logger)
        {
            _db = db;
            _logger = logger;

        }
        //Test
        public IActionResult Sample() { return View(); }
        //Dashboard Controller
        public IActionResult Dashboard()
        {
            if (CheckRole()) {
                ViewData["User Registrations"] = _db.Homeowner_Details.Count();
                ViewData["RFID Registrations"] = _db.Vehicle_Information.Where(x=> x.RFID_number !=null).Count();
                ViewData["Pending Payments"] = _db.Due_Details.Where(x => x.Status == "Unpaid").Count();
                return View();
            }
            return RedirectToAction("AccessDenied", "Home");

        }

        //UserManagement Controller
        public IActionResult UserManagement()
        {
            if (CheckRole()) { return View(_db.User_Accounts.ToList()); }
            return RedirectToAction("AccessDenied", "Home");
        }

        public IActionResult _UserManagementCreate()
        {
            return PartialView();
        }
        public IActionResult _UserManagementDelete(int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Error", "MainMenu", ID);
            }

            var useracc = _db.User_Accounts.FirstOrDefault(m => m.AccountID == ID);
            if (useracc == null)
            {
                return RedirectToAction("Error", "MainMenu", ID);
            }

            return View(useracc);
        }
        //Delete all info in the users
        public IActionResult UserDelete(User_Account obj)
        {
            try
            {
                var forhomeowner = _db.Homeowner_Details.FirstOrDefault(x=> x.AccountID == obj.AccountID);
                var forUserDues = _db.Due_Details.Where(x => x.AccountID == obj.AccountID).ToList();
                Console.WriteLine("dUES" + forUserDues.ToList());
                _db.User_Accounts.Remove(obj);
                if (forhomeowner != null)
                {
                    Console.WriteLine("Dues should be deleted");
                    _db.Homeowner_Details.Remove(forhomeowner);
                    _db.SaveChanges();
                }
                if (forUserDues != null) {
                    Console.WriteLine("Dues should be deleted");
                    _db.Due_Details.RemoveRange(forUserDues);
                    _db.SaveChanges();
                }
                _db.SaveChanges();
                return RedirectToAction("UserManagement");
            }
            catch (Exception) { return RedirectToAction("UserManagement"); }

        }
        public IActionResult _UserManagementDetails(int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Error", "MainMenu", ID);
            }

            var useracc = _db.Homeowner_Details.FirstOrDefault(m => m.AccountID == ID);
            if (useracc == null)
            {
                return RedirectToAction("Error", "MainMenu", ID);
            }

            return View(useracc);
        }

        public async Task<IActionResult> CreatUserAcc(User_Account info)
        {
            if (ModelState.IsValid)
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("Cessna", "krfortin15@gmail.com"));
                mailMessage.To.Add(new MailboxAddress(info.Username, info.Email));
                mailMessage.Subject = "subject";
                mailMessage.Body = new TextPart("plain")
                {
                    Text = "Username:" + info.Username + " " +
                    "Password: " + info.Password
                };
                using (var smtpclient = new SmtpClient())
                {
                    smtpclient.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    smtpclient.Authenticate("krfortin15@gmail.com", "axji zqrc cooa ymwv");
                    smtpclient.Send(mailMessage);
                    smtpclient.Disconnect(true);
                }
                _db.User_Accounts.Add(info);
                await _db.SaveChangesAsync();
                return RedirectToAction("UserManagement");
            }
            return View();
        }
        [HttpPost]
        public JsonResult GeneratePass(string userdata)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            int i = 0;
            while (i <= 32)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
                i++;
            }
            Console.WriteLine(userdata);
            return Json(res.ToString());

        }

        //AssciationDues Controller
        public IActionResult AssociationDues(string sortOrder)
        {

            if (CheckRole()) {
                ViewData["NameSortParam"] = String.IsNullOrEmpty(sortOrder) ? "nameDesc" : "";
                ViewData["DateSortParam"] = sortOrder == "date" ? "dateDesc" : "date";
                ViewData["AmountSortParam"] = sortOrder == "amount" ? "amountDesc" : "amount";
                var result = (from due in _db.Due_Details
                              join
                              homeacc in _db.Homeowner_Details on due.AccountID equals homeacc.AccountID
                              select new Dues
                              {
                                  AccountID = due.AccountID,
                                  FullName = homeacc.Firstname + " " + homeacc.Middlename + " " + homeacc.Surname,
                                  Amount = due.Amount,
                                  Date = due.Date,
                                  TypeofDues = due.TypeofDues,
                                  Status = due.Status
                              });
                switch (sortOrder)
                {
                    case "nameDesc":
                        result = result.OrderByDescending(r => r.FullName);
                        break;
                    case "date":
                        result = result.OrderBy(r => r.Date); break;
                    case "dateDesc":
                        result = result.OrderByDescending(r => r.Date); break;
                    case "amount":
                        result = result.OrderBy(r => r.Amount); break;
                    case "amountDesc":
                        result = result.OrderByDescending(r => r.Amount); break;
                    default:
                        result = result.OrderBy(r => r.FullName); break;
                }
                ViewData["ListOwners"] = _db.Homeowner_Details.ToList();
                return View(result.AsNoTracking().ToList());
            }
            return RedirectToAction("AccessDenied", "Home");

        }
        public IActionResult _CreateDues() {
            return PartialView();
        }
        public async Task <IActionResult> CreateUserdue(Dues info) {
            if (ModelState.IsValid) {
                _db.Due_Details.Add(info);
                await _db.SaveChangesAsync();
                return RedirectToAction("AssociationDues");
            }
            return RedirectToAction("AssociationDues");
        }

        [HttpPost]
        public JsonResult CheckName(string userdata)
        {
            if (userdata!= null) {
                Console.WriteLine(userdata);
                char[] trimchars = { ' ', '!' };
                string[] trimmedArray = userdata.Split(trimchars, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                if (trimmedArray.Count() == 4)
                {
                    var SearchData = _db.Homeowner_Details.Where(x => x.Firstname == trimmedArray[0] + " " + trimmedArray[1]).SingleOrDefault();
                    if (SearchData != null)
                    {
                        return Json(SearchData.AccountID);
                    }
                    else
                    {
                        return Json(0);
                    }
                }
                else
                {
                    var SearchData = _db.Homeowner_Details.Where(x => x.Firstname == trimmedArray[0]).SingleOrDefault();

                    if (SearchData != null)
                    {
                        return Json(SearchData.AccountID);
                    }
                    else
                    {
                        return Json(0);
                    }

                }
            }
            return Json(0);
        }

        //Logs Controller
        public IActionResult Logs()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Home");
        }

        public IActionResult _PrintLogs() { return PartialView(); }

        //Guards Controller
        public IActionResult Guards()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Home");
        }
        
        //Vehicle List

        //RFID Management Controller
        public IActionResult VehicleManagement(string sortOrder)
        {
            if (CheckRole())
            {
                ViewData["NameSortParam"] = String.IsNullOrEmpty(sortOrder) ? "nameDesc" : "";
                var result = (from vehicle in _db.Vehicle_Information
                              join
                              homeacc in _db.Homeowner_Details on vehicle.AccountID equals homeacc.AccountID
                              select new Vehicle_Information
                              {
                                  AccountID = vehicle.AccountID,
                                  FullName = homeacc.Firstname + " " + homeacc.Middlename + " " + homeacc.Surname,
                                  PlateNo = vehicle.PlateNo,
                                  VehicleModel = vehicle.VehicleModel,
                                  VehicleType = vehicle.VehicleType,
                                  RFID_number = vehicle.RFID_number
                              });
                switch (sortOrder)
                {
                    case "nameDesc":
                        result = result.OrderByDescending(r => r.FullName);
                        break;
                    default:
                        result = result.OrderBy(r => r.FullName); break;
                }
                ViewData["ListOwners"] = _db.Homeowner_Details.ToList();
                return View(result.AsNoTracking().ToList());
            }
            return RedirectToAction("AccessDenied", "Home");
        }

        public IActionResult _CreateVehicle() { return PartialView(); }
        [HttpPost]
        public async Task<IActionResult> _CreateVehicle(Vehicle_Information info)
        {
            if (ModelState.IsValid)
            {
                _db.Vehicle_Information.Add(info);
                await _db.SaveChangesAsync();
                return RedirectToAction("RFIDManagement");
            }
            return RedirectToAction("RFIDManagement");
        }

        public IActionResult _DeleteVehicle() { return View(); }

        public IActionResult _EditVehicle() { return View(); }

        public IActionResult Error(Homeowner_details ID)
        {
            return View(ID);
        }


        public bool CheckRole()
        {
            var usertype = HttpContext.Session.GetString("UserType");
            Console.WriteLine(usertype);
            if (usertype != null)
            {
                if (usertype == "Admin")
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
    }
}
