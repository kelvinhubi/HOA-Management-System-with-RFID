using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.InfoSec;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{


    public class MainMenuController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger _logger;
        private readonly EnvironmentModel _env;
        private readonly IWebHostEnvironment _webenv;
        public MainMenuController(AppDbContext db, ILogger<MainMenuController> logger,IOptions<EnvironmentModel> Accessor,IWebHostEnvironment environment)
        {
            _db = db;
            _logger = logger;
            _env = Accessor.Value;
            _webenv = environment;
        }
        //Test
        public IActionResult Sample()
        {

            return View(_db.Vehicle_Information.ToList());
        }
        //Dashboard Controller
        public IActionResult Dashboard()
        {
            if (CheckRole())
            {

                ViewData["User Registrations"] = _db.Homeowner_Details.Count();
                ViewData["RFID Registrations"] = _db.Vehicle_Information.Where(x => x.RFID_number != "").Count();
                ViewData["Pending Payments"] = _db.Due_Details.Where(x => x.Status == "Unpaid").Count();
				return View();
            }
            
            return RedirectToAction("AccessDenied", "Home");

        }
        //Announcements
        public IActionResult Announcements() {
            if (CheckRole())
            {
                NewModel model = new NewModel();
                model.Announcements = _db.Announcements.OrderByDescending(_=>_.DatePosted);
                model.Announcement = new Announcements();
                return View(model);
            }
            return RedirectToAction("AccessDenied", "Home");

        }

        public IActionResult _AddAnnouncement() {
            return PartialView();
        }

        [HttpPost]
        public IActionResult _AddAnnouncement(Announcements info) 
        {

            if (ModelState.IsValid) {
                if (info.backgroundFile != null) {
                    string folder = "Images\\";
                    folder += Guid.NewGuid().ToString() + info.backgroundFile.FileName;
                    string serverFolder = Path.Combine(_webenv.ContentRootPath,folder);
                    info.backgroundFile.CopyToAsync(new FileStream(serverFolder,FileMode.Create));
                }

                _db.Announcements.Add(info);
                _db.SaveChanges();
            }

            return RedirectToAction("Announcements");
        }




        //UserManagement Controller
        public IActionResult UserManagement()
        {
            if (CheckRole())
            {
                var model = new NewModel();
                model.useracc = _db.User_Accounts.ToList();

                return View(model);
            }
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

            return PartialView(useracc);
        }
        //Delete all info in the users
        [HttpPost]
        public async Task<IActionResult> UserDelete(User_Account obj)
        {
            try
            {
                var forhomeowner = _db.Homeowner_Details.FirstOrDefault(x => x.AccountID == obj.AccountID);
                var forUserDues = _db.Due_Details.Where(x => x.AccountID == obj.AccountID).ToList();
                var forUserfeestatus = _db.userFeesStatuses.Where(_ => _.AccountID == obj.AccountID).ToList();
                _db.User_Accounts.Remove(obj);
                if (forhomeowner != null)
                {
                    Console.WriteLine("Homeowners should be deleted");
                    _db.Homeowner_Details.Remove(forhomeowner);
                    _db.SaveChanges();
                }
                if (forUserDues != null)
                {
                    Console.WriteLine("Dues should be deleted");
                    _db.Due_Details.RemoveRange(forUserDues);
                    _db.SaveChanges();
                }
                if (forUserfeestatus != null)
                {
                    _db.userFeesStatuses.RemoveRange(forUserfeestatus);
                    _db.SaveChanges();
                }
                _db.SaveChanges();
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Delete User Account",
                    LogDescription = "Logged In Username:" + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                await _db.SaveChangesAsync();
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
            var model = new NewModel();
            var useracc = _db.Homeowner_Details.AsNoTracking().FirstOrDefault(m => m.AccountID == ID);
            model.CheckMe = _db.userFeesStatuses.Where(x => x.AccountID == ID).Select(vm => new CheckBoxItem()
            {
                ID = vm.UserFeeID,
                FeesName = vm.FeesName,
                TypeOfFees = vm.TypeOfFees,
                IsChecked = vm.Status.Equals("Enabled") ? true : false
            }).ToList();
            if (useracc == null)
            {
                return RedirectToAction("Error", "MainMenu", ID);
            }
            model.Homeowner = useracc;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> _UserManagementDetails(NewModel info, int ID)
        {
            var userfeestatus = _db.userFeesStatuses.Where(_ => _.AccountID == ID);

            if (userfeestatus == null) { return NotFound(); }
            var x = userfeestatus.ToList();
            var y = info.CheckMe;
            for (int i = 0; i < info.CheckMe.Count(); i++)
            {
                x[i].Status = y[i].IsChecked == true ? "Enabled" : "Disabled";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("UserManagement");
        }


        public async Task<IActionResult> CreatUserAcc(User_Account info)
        {
            Console.WriteLine(info.AccountID);
            if (ModelState.IsValid)
            {
                var mailMessage = new MimeMessage();
                var Username = info.Username;
                var Password = info.Password;
                var bodybuild = new BodyBuilder();
                
                bodybuild.HtmlBody = CreateMailBody(Username, Password);
                mailMessage.From.Add(new MailboxAddress("Cessna", "krfortin15@gmail.com"));
                mailMessage.To.Add(new MailboxAddress(info.Username, info.Email));
                mailMessage.Subject = "Username and Password";
                mailMessage.Body = bodybuild.ToMessageBody();
                using (var smtpclient = new SmtpClient())
                {
                    smtpclient.Connect(_env.Host, Convert.ToInt32(_env.Port), SecureSocketOptions.StartTls);
                    smtpclient.Authenticate(_env.Email, _env.Password);
                    smtpclient.Send(mailMessage);
                    smtpclient.Disconnect(true);
                }
                info = new User_Account {
					Username = info.Username,
					Password = Encryption.Encrpyt(info.Password, _env.EncryptionKey, _env.IVKey),
					Email = info.Email,
					AccountID = info.AccountID
				};
                _db.User_Accounts.Add(info);
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Create User Account",
                    LogDescription = "Logged In Username:" + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
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
            while (i <= 8)
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

            if (CheckRole())
            {
                var result = (from due in _db.Due_Details
                              join
                              homeacc in _db.Homeowner_Details on due.AccountID equals homeacc.AccountID
                              select new Dues
                              {
                                  AccountID = due.AccountID,
                                  FullName = homeacc.Firstname + " " + homeacc.Middlename + " " + homeacc.Surname,
                                  Amount = due.Amount,
                                  Date = due.Date,
                                  FeesName = due.FeesName,
                                  TypeofDues = due.TypeofDues,
                                  Status = due.Status
                              });
                NewModel model = new NewModel();
                model.Homeacc = _db.Homeowner_Details.ToList();
                model.Userfeestatuses = _db.userFeesStatuses.ToList();
                model.dues = new Dues();
                model.ListDues = result.ToList();
                return View(model);
            }
            return RedirectToAction("AccessDenied", "Home");

        }
        public IActionResult _CreateDues()
        {
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> _CreateDues(string AccountID, string Amount, string TypeofDues, DateOnly Date, string Status,string FeesName)
        {

            if (ModelState.IsValid)
            {
                
                _db.Due_Details.Add(new Dues
                {
                    FeesName = FeesName.Substring(0, FeesName.Length -2),
                    AccountID = Convert.ToInt32(AccountID),
                    Amount = Amount,
                    TypeofDues = TypeofDues,
                    Date = Date,
                    Status = Status,
                });
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Create Dues",
                    LogDescription = "Dues Name:" + TypeofDues + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                await _db.SaveChangesAsync();
                return RedirectToAction("AssociationDues");
            }
            return RedirectToAction("AssociationDues");

        }
        public IActionResult _ClearPaid()
        {
            var result = _db.Due_Details.Where(_ => _.Status.Equals("Paid"));
            _db.Due_Details.RemoveRange(result);
            _db.SaveChanges();
            return RedirectToAction("AssociationDues");
        }
        [HttpPost]
        public JsonResult CheckName(string userdata)
        {
            if (userdata != null)
            {
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
            if (CheckRole())
            {

                return View(_db.logsLists.ToList());
            }
            return RedirectToAction("AccessDenied", "Home");
        }
        public IActionResult PrintLogs(string? nameformat)
        {

            QuestPDF.Settings.License = LicenseType.Community;
            void ComposeTable(IContainer container)
            {
                container.Border(1).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Log Name");
                        header.Cell().Text("Log Description");
                        header.Cell().Text("User Role");
                        header.Cell().Text("Log Date");
                    });
                    var result = _db.logsLists.ToList();
                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            table.Cell().Text(item.LogName.ToString());
                            table.Cell().Text(item.LogDescription.ToString());
                            table.Cell().Text(item.LogUserRole.ToString());
                            table.Cell().Text(item.LogDate.ToString());
                        }
                    }
                });
            }
            Document.Create(Print =>
           {
               Print.Page(page =>
               {
                   page.Content()
                   .Column(c => ComposeTable(c.Item()));
                   page.Size(PageSizes.A4);
                   page.Header()
                   .Text("Logs")
                   .SemiBold()
                   .FontSize(30);

               });
           }).GeneratePdf(nameformat+"_"+Guid.NewGuid().ToString()+"_"+DateTime.Now.ToString("yyyy-MMM-dd")+"_Logs.pdf"); //RENAMING USING RANDOM WORDS

            return RedirectToAction("Logs");
        }
        public IActionResult _ClearLogs()
        {
            PrintLogs("Backup_");
            var result = _db.logsLists.ToList();
            _db.logsLists.RemoveRange(result);
            _db.SaveChanges();
            return RedirectToAction("Logs");

        }
        //Guards Controller
        public IActionResult Guards()
        {
            if (CheckRole()) { return View(); }
            return RedirectToAction("AccessDenied", "Home");
        }

        //Vehicle List

        //Vehicle Management Controller
        public IActionResult VehicleManagement(string sortOrder)
        {
            if (CheckRole())
            {
                ViewData["NameSortParam"] = System.String.IsNullOrEmpty(sortOrder) ? "nameDesc" : "";
                var result = (from vehicle in _db.Vehicle_Information
                              join
                              homeacc in _db.Homeowner_Details on vehicle.AccountID equals homeacc.AccountID
                              select new Vehicle_Information
                              {
                                  VehicleID = vehicle.VehicleID,
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
                return RedirectToAction("VehicleManagement");
            }
            return RedirectToAction("VehicleManagement");
        }

        public IActionResult _DeleteVehicle(int ID)
        {
            var result = _db.Vehicle_Information.SingleOrDefault(_ => _.VehicleID == ID);
            if (result == null) { return NotFound(); }
            return PartialView(result);
        }
        [HttpPost]
        public IActionResult _DeleteVehicle(Vehicle_Information info)
        {
            if (ModelState.IsValid)
            {
                _db.Vehicle_Information.Remove(info);
                _db.SaveChanges();
            }
            return RedirectToAction("VehicleManagement");
        }
        public IActionResult _EditVehicle(int ID)
        {
            var result = _db.Vehicle_Information.SingleOrDefault(_ => _.VehicleID == ID);
            if (result == null) { return NotFound(); }
            return PartialView(result);
        }
        [HttpPost]
        public IActionResult _EditVehicle(Vehicle_Information info)
        {
            if (ModelState.IsValid)
            {
                _db.Vehicle_Information.Update(info);
                _db.SaveChanges();
            }
            return RedirectToAction("VehicleManagement");
        }


        //Fees Controller
        public IActionResult Fees() { return View(_db.feesLists.ToList()); }

        public IActionResult _CreateFees() { return PartialView(); }
        [HttpPost]
        public async Task<IActionResult> _CreateFees(FeesList info)
        {
            if (ModelState.IsValid)
            {
                _db.feesLists.Add(info);
                _db.SaveChanges();
                var data1 = _db.Homeowner_Details.ToList();
                foreach (var x in data1)
                {
                    var data2 = new UserFeesStatus
                    {
                        AccountID = x.AccountID,
                        FeesName = info.FeesName,
                        TypeOfFees = info.TypeOfFees,
                        IDFees = info.IDFees,
                        Amount = info.Amount,
                        Status = info.Status
                    };
                    _db.userFeesStatuses.Add(data2);
                }
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Create Fees",
                    LogDescription = "Fees Name:" + info.TypeOfFees + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                await _db.SaveChangesAsync();
                return RedirectToAction("Fees");
            }
            return RedirectToAction("Fees");

        }
        public IActionResult _EditFees(int? ID)
        {

            if (ID == null)
            {
                return NotFound();
            }

            var result = _db.feesLists.FirstOrDefault(x => x.IDFees == ID);
            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> _EditFees(FeesList info)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.feesLists.Update(info);
                    _db.SaveChanges();
                    var feeid = _db.userFeesStatuses.Where(_ => _.IDFees == info.IDFees);
                    if (feeid != null)
                    {
                        foreach (var x in feeid)
                        {
                            x.FeesName = info.FeesName;
                            x.TypeOfFees = info.TypeOfFees;
                            x.Status = info.Status;
                            x.Amount = info.Amount;
                        }

                    }
                    _db.logsLists.Add(new LogsList
                    {
                        LogName = "Create Fees",
                        LogDescription = "Fees Name:" + info.TypeOfFees + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                        LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                    });
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Fees");
                }
                return RedirectToAction("Fees");
            }
            return RedirectToAction("Fees");

        }
        public IActionResult _DeleteFees(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var result = _db.feesLists.SingleOrDefault(_ => _.IDFees == id);

            if (result == null)
            {
                return NotFound();
            }
            return View(result);
        }
        [HttpPost]
        public IActionResult _DeleteFees(int id)
        {
            var result = _db.feesLists.SingleOrDefault(_ => _.IDFees == id);
            if (result != null)
            {

                _db.feesLists.Remove(result);
                _db.SaveChanges();
                var userfeesstatus = _db.userFeesStatuses.Where(_d => _d.IDFees == id);
                _db.userFeesStatuses.RemoveRange(userfeesstatus);
                _db.SaveChanges();
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Delete Fees",
                    LogDescription = "Fees Name:" + result.TypeOfFees + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                _db.SaveChanges();
            }
            else
            {
                return NotFound();
            }

            return RedirectToAction("Fees");
        }

        //Profile
        public IActionResult Profile() {
            Console.WriteLine(HttpContext.Session.GetString("SessionID"));
            var result = _db.Admin_Accounts.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
             return View(result);
        }
        //Json Result
        [HttpPost]
        public JsonResult CheckAmount(string userdata, string AccountID)
        {
            Console.WriteLine("Check aMOUNT:" + userdata);
            if (userdata != null)
            {

                var SearchData = _db.userFeesStatuses.Where(x => x.TypeOfFees.Equals(userdata) && x.Status.Equals("Enabled") && x.AccountID == Convert.ToInt32(AccountID));
                if (SearchData != null)
                {
                    return Json(SearchData.Sum(i=> Convert.ToInt32(i.Amount)));
                }
                else
                {
                    return Json(0);
                }
            }
            return Json(0);
        }
        [HttpPost]
        public JsonResult CheckFees(string userdata)
        {
            Console.WriteLine("Hello World" + userdata);
            if (userdata != null)
            {

                var SearchData = _db.userFeesStatuses.Where(x => x.AccountID == Convert.ToInt32(userdata) && x.Status == "Enabled").OrderBy(_=>_.FeesName).ToList();
                if (SearchData != null)
                {
                    return Json(SearchData);
                }
                else
                {
                    return Json(0);
                }
            }
            return Json(0);
        }
        public IActionResult Error(Homeowner_details ID)
        {
            return View(ID);
        }


        public bool CheckRole()
        {
            string? usertype = HttpContext.Session.GetString("UserType");
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
        private string CreateMailBody(string Username, string Password)
        {
            string? dir = System.IO.Path.GetFullPath("Views\\Home\\index.html");
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(dir))
            {
                body = reader.ReadToEnd();
            };

            body = body.Replace("{Username}", Username);
            body = body.Replace("{Password}", Password);
            return body;
        }

    }
}
