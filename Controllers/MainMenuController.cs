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
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using System.Data;
using DocumentFormat.OpenXml.EMMA;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{


    public class MainMenuController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger _logger;
        private readonly EnvironmentModel _env;
        private readonly IWebHostEnvironment _webenv;
        public MainMenuController(AppDbContext db, ILogger<MainMenuController> logger, IOptions<EnvironmentModel> Accessor, IWebHostEnvironment environment)
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
                model.Announcements = _db.Announcements.OrderByDescending(_ => _.DatePosted);
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
                /*if (info.backgroundFile != null) {
                    string folder = "Images\\";
                    folder += Guid.NewGuid().ToString() + info.backgroundFile.FileName;
                    string serverFolder = Path.Combine(_webenv.ContentRootPath, folder);
                    info.backgroundFile.CopyToAsync(new FileStream(serverFolder, FileMode.Create));
                }*/
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
            var cars = _db.Vehicle_Information.AsNoTracking().Where(_ => _.AccountID == ID);
            var homes = _db.homesLists.AsNoTracking().Where(_ => _.AccountID == ID);
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
            model.Vehicles = cars;
            model.Homeowner = useracc;
            model.Homes = homes;
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
            _db.logsLists.Add(new LogsList
            {
                LogName = "Edit UserFees",
                LogDescription = "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                LogUserRole = "" + HttpContext.Session.GetString("UserType"),
            });
            _db.SaveChanges();
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
                                  Invoice = due.Invoice,
                                  FeesName = due.FeesName,
                                  TypeOfFee = due.TypeOfFee,
                                  Status = due.Status
                              });
                NewModel model = new NewModel();
                model.Homeacc = _db.Homeowner_Details.ToList();
                model.Userfeestatuses = _db.userFeesStatuses.ToList();
                model.feesLists = _db.feesLists.OrderBy(_ => _.TypeOfFees).ToList();
                model.dues = new Dues();
                model.ListDues = result.ToList();
                return View(model);
            }
            return RedirectToAction("AccessDenied", "Home");

        }
        [HttpPost]
        public JsonResult FindHomeOwners(string userdata)
        {
            if (userdata == null)
            {
                return Json(new { success = false });
            }
            var result = (from homeacc in _db.Homeowner_Details
                          join
                          userfees in _db.userFeesStatuses on homeacc.AccountID equals userfees.AccountID where userfees.Status.Equals("Enabled") && userfees.TypeOfFees.Equals(userdata)
                          select new Homeowner_details
                          {
                              Firstname = homeacc.Firstname,
                              Surname = homeacc.Surname,
                              Middlename = homeacc.Middlename,
                              AccountID = homeacc.AccountID,
                          }).ToList();
            var recentval = "";
            var anotherresult = new List<Homeowner_details>();
            foreach (var obj in result.OrderBy(_ => _.FullName)) {
                if (!obj.FullName.Equals(recentval)) {
                    recentval = obj.FullName;
                    anotherresult.Add(obj);
                }
            }
            return Json(anotherresult.ToList());
        }
        public IActionResult _CreateDues()
        {
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> _CreateDues(string AccountID, string Amount, string TypeofDues, DateOnly Date, string Status, string FeesName)
        {

            if (ModelState.IsValid)
            {
                _db.Due_Details.Add(new Dues
                {
                    FeesName = FeesName.Substring(0, FeesName.Length - 2),
                    AccountID = Convert.ToInt32(AccountID),
                    Invoice = GenerateText(6),
                    Amount = Amount,
                    TypeOfFee = TypeofDues,
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
        public IActionResult _CreateDuesMultiple() {
            return PartialView();
        }
        [HttpPost]
        public JsonResult AddDuesToUsers(string[] userIds, string TypeofDues, DateOnly date)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    var getuser = _db.userFeesStatuses.Where(_ => _.AccountID == Convert.ToInt32(userId) && _.Status.Equals("Enabled") && _.TypeOfFees.Equals(TypeofDues)).ToList();
                    if (getuser.Count() != 0) {
                        string str = "";
                        int Amount = 0;
                        foreach (var x in getuser)
                        {
                            str += x.FeesName + ", ";
                            Amount += Convert.ToInt32(x.Amount);
                        }
                        Dues info = new Dues
                        {
                            AccountID = Convert.ToInt32(userId),
                            FeesName = str.Substring(0, str.Length - 2),
                            Invoice = GenerateText(6),
                            TypeOfFee = TypeofDues,
                            Amount = Amount.ToString(),
                            Status = "Unpaid",
                            Date = date
                        };
                        _db.logsLists.Add(new LogsList
                        {
                            LogName = "Add Dues to Multiple Users",
                            LogDescription = "Dues Added: " + info.TypeOfFee + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                            LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                        });
                        _db.Due_Details.Add(info);
                    }
                    else {

                    }
                    // Example: _duesService.CreateDuesForUser(userId);
                    
                }
                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return Json(new { success = false, message = ex.Message });
            }
            
        }
        public string GenerateText(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            int i = 0;
            while (i <= length)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
                i++;
            }
            return res.ToString();
        }
        public IActionResult ClearPaid()
        {
            var result = _db.Due_Details.Where(_ => _.Status.Equals("Paid"));
            _db.Due_Details.RemoveRange(result);
            _db.SaveChanges();
            _db.logsLists.Add(new LogsList
            {
                LogName = "Clear Paid Dues",
                LogDescription = "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                LogUserRole = "" + HttpContext.Session.GetString("UserType"),
            });
            _db.SaveChanges();
            return RedirectToAction("AssociationDues");
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
            if (nameformat == null) {
                nameformat = "ExportPdf";
            }
            string filepath = "wwwroot\\ExportedFiles\\" + nameformat + "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_Logs.pdf";
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
           }).GeneratePdf(filepath); //RENAMING USING RANDOM WORDS
            _db.logsLists.Add(new LogsList
            {
                LogName = "Export Logs",
                LogDescription = "Export FileType: PDF " +"Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                LogUserRole = "" + HttpContext.Session.GetString("UserType"),
            });
            _db.SaveChanges();
            return File(System.IO.File.ReadAllBytes(filepath), "application/pdf", nameformat + "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_Logs.pdf");
        }
        [HttpPost]
        public IActionResult ExportExcel() {
            DataTable dt = new DataTable("Student");
            dt.Columns.AddRange(new DataColumn[4] { 
                                            new DataColumn("Log Name"),
                                            new DataColumn("Description"),
                                            new DataColumn("User Role"),
                                            new DataColumn("Date")});

            var Logs = _db.logsLists.ToList();

            foreach (var loglists in Logs)
            {
                dt.Rows.Add(loglists.LogName, loglists.LogDescription, loglists.LogUserRole, loglists.LogDate);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    _db.logsLists.Add(new LogsList
                    {
                        LogName = "Export Logs",
                        LogDescription = "Export FileType: Excel " + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                        LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                    });
                    _db.SaveChanges();
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_Logs.xlsx");
                }
            }
        }
        public IActionResult _ClearLogs()
        {
            //PrintLogs("Backup_");
            var result = _db.logsLists.ToList();
            _db.logsLists.RemoveRange(result);
            _db.SaveChanges();
            _db.logsLists.Add(new LogsList
            {
                LogName = "Logs Cleared",
                LogDescription ="Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                LogUserRole = "" + HttpContext.Session.GetString("UserType"),
            });
            return RedirectToAction("Logs");

        }
        //Chat
        public IActionResult Chat() {
            return View();
        }
        //Guards Controller
        public IActionResult Guards()
        {
            if (CheckRole()) { return View(_db.Guard_Information.ToList()); }
            return RedirectToAction("AccessDenied", "Home");
        }
        public IActionResult _GuardManagementCreate() {
            return PartialView();
        }
        [HttpPost]
        public IActionResult GuardCreate(Guard_Information info)
        {
            if (ModelState.IsValid)
            {
                var mailMessage = new MimeMessage();
                var Username = info.Username;
                var Password = info.Password;
                var bodybuild = new BodyBuilder();

                bodybuild.HtmlBody = CreateMailBody(Username, Password);
                mailMessage.From.Add(new MailboxAddress("Cessna", _env.Email));
                mailMessage.To.Add(new MailboxAddress(info.Username, info.Email));
                mailMessage.Subject = "Guard Username and Password";
                mailMessage.Body = bodybuild.ToMessageBody();
                using (var smtpclient = new SmtpClient())
                {
                    smtpclient.Connect(_env.Host, Convert.ToInt32(_env.Port), SecureSocketOptions.StartTls);
                    smtpclient.Authenticate(_env.Email, _env.Password);
                    smtpclient.Send(mailMessage);
                    smtpclient.Disconnect(true);
                }
                info = new Guard_Information
                {
                    FirstName = info.FirstName,
                    LastName = info.LastName,
                    Username = info.Username,
                    MiddleName = info.MiddleName,
                    Password = Encryption.Encrpyt(info.Password, _env.EncryptionKey, _env.IVKey),
                    PhoneNumber = info.PhoneNumber,
                    Email = info.Email,
                    ID = info.ID
                };
                _db.Guard_Information.Add(info);
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Create Guard Account",
                    LogDescription = "Logged In Username:" + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole =""+ HttpContext.Session.GetString("UserType"),
                });
                _db.SaveChanges();

                return RedirectToAction("Guards");
            }
            return View();
        }
        public IActionResult _GuardManagementDelete(int? ID)
        {
            if (ID == null)
            {
                return RedirectToAction("Error", "MainMenu", ID);
            }

            var useracc = _db.Guard_Information.FirstOrDefault(m => m.ID == ID);
            if (useracc == null)
            {
                return RedirectToAction("Error", "MainMenu", ID);
            }

            return PartialView(useracc);
        }
        [HttpPost]
        public async Task<IActionResult> GuardDelete(Guard_Information obj)
        {
            try
            {

                _db.Guard_Information.Remove(obj);
                _db.SaveChanges();
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Delete Guard Account",
                    LogDescription = "Logged In Username:" + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                await _db.SaveChangesAsync();
                return RedirectToAction("Guards");
            }
            catch (Exception) { return RedirectToAction("Guards"); }

        }
        //Vehicle List

        //Vehicle Management Controller
        public IActionResult VehicleManagement(string sortOrder)
        {
            if (CheckRole())
            {
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
                                  RFID_number = vehicle.RFID_number,
                                  RFID_status = vehicle.RFID_status

                              });
                NewModel model = new NewModel();
                model.Vehicle = new Vehicle_Information();
                model.Vehicles = result.ToList();
                model.Homeacc = _db.Homeowner_Details.ToList();
                return View(model);
            }
            return RedirectToAction("AccessDenied", "Home");
        }

        public IActionResult _CreateVehicle() { return PartialView(); }
        [HttpPost]
        public async Task<IActionResult> _CreateVehicle(NewModel info)
        {
            _db.Vehicle_Information.Add(info.Vehicle);
            _db.logsLists.Add(new LogsList
            {
                LogName = "Add Vehicle",
                LogDescription = "Vehicle Owner" + info.Vehicle.FullName + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                LogUserRole = "" + HttpContext.Session.GetString("UserType"),
            });
            await _db.SaveChangesAsync();
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
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Delete Vehicle",
                    LogDescription = "Vehicle Owner: " + info.FullName + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
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
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Edit Vehicle",
                    LogDescription = "Vehicle Owner: "+info.FullName+"Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
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
                        LogName = "Edit Fees",
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
            var result = _db.Admin_Accounts.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
            return View(result);
        }
		[HttpPost]
		public JsonResult UpdateProfile(int AccountID, string Password, string Username)
		{
			if (ModelState.IsValid)
			{
				var result = _db.Admin_Accounts.Where(_ => _.AccountID == AccountID).ToList().FirstOrDefault();
				if (result != null)
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
					_db.Admin_Accounts.Update(result);
					_db.SaveChanges();
                    _db.logsLists.Add(new LogsList
                    {
                        LogName = "Update Profile",
                        LogDescription = "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                        LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                    });
                    _db.SaveChanges();
                    return Json(new { success = true });
				}

			}
			return Json(new { success = false });
		}
		//HomesList
		public IActionResult HomesList() {
            if (CheckRole()) {
                NewModel model = new NewModel();
                model.Homes = _db.homesLists.ToList();
                model.Home = new HomesList();
                model.Homeacc = _db.Homeowner_Details.ToList();
                return View(model);
            }
            return RedirectToAction("AccessDenied", "Home");
        }
        [HttpPost]
        public IActionResult _AddHomesList(NewModel info){
            if (info.Home != null) { 
                _db.homesLists.Add(info.Home);
                _db.SaveChanges();
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Add Homes List",
                    LogDescription = "Added Home"+info.Home.HomeName+"Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                _db.SaveChanges();
            }
            return RedirectToAction("HomesList");
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

                var SearchData = _db.userFeesStatuses.Where(x => x.AccountID == Convert.ToInt32(userdata) && x.Status == "Enabled").ToList();
                if (SearchData != null)
                {
                    return Json(SearchData.OrderBy(_ => _.TypeOfFees).ToList());
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
        public static string CreateMailBody(string Username, string Password)
        {
            string? dir = System.IO.Path.GetFullPath("wwwroot\\Htmls\\index.html");
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
