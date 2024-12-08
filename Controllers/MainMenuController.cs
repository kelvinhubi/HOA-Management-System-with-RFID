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
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Hubs;
using Microsoft.AspNetCore.SignalR;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using QuestPDF.Companion;
using Humanizer;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.VisualBasic;
using System.Globalization;
using SQLitePCL;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{


    public class MainMenuController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger _logger;
        private readonly IHubContext<MyHub> _hub;
        private readonly EnvironmentModel _env;
        private readonly IWebHostEnvironment _webenv;
        public MainMenuController(AppDbContext db, ILogger<MainMenuController> logger, IOptions<EnvironmentModel> Accessor, IWebHostEnvironment environment, IHubContext<MyHub> hubContext)
        {
            _db = db;
            _logger = logger;
            _env = Accessor.Value;
            _webenv = environment;
            _hub = hubContext;
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
                ViewData["RFID Registrations"] = _db.Vehicle_Information.Where(_=>_.RFID_number!="N/A").Count();
                ViewData["Pending Payments"] = _db.Due_Details.Where(x => x.Status == "Unpaid").Count();
                ViewData["HoaMembers"] = _db.Homeowner_Details.Where(_=>_.Role =="Homeowner").Count();
                ViewData["DeliquentMembers"] = _db.Due_Details.Where(x => x.Status == "Unpaid" && DateOnly.FromDateTime(DateTime.Now) < x.Date).Count();
                ViewData["TotalVehicles"] = _db.Vehicle_Information.Count();
                ViewData["Maintenance"] = _db.maintenanceRequests.Where(x => x.RequestStatus != "Completed").Count();
                ViewData["Pets"] = _db.PetInformation.Count();
                ViewData["Officers"] = _db.Officers.Count();
                return View();
            }

            return RedirectToAction("AccessDenied", "MainMenu");

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
            return RedirectToAction("AccessDenied", "MainMenu");

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

        public IActionResult _DeleteAnnouncement(int? id) {
            if (id == null) {
                return NotFound();
            }
            var info = _db.Announcements.FirstOrDefault(_ => _.AnnouncementID == id);
            if (info == null) {
                return NotFound();
            }
            _db.Announcements.Remove(info);
            _db.SaveChanges();
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
            return RedirectToAction("AccessDenied", "MainMenu");
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
                var forUserDues = _db.Due_Details.Where(x => x.AccountID == obj.AccountID && obj.Role == "Homeowner").ToList();
                var forUserfeestatus = _db.userFeesStatuses.Where(_ => _.AccountID == obj.AccountID && obj.Role == "Homeowner").ToList();
                var forHomes = _db.homesLists.Where(x => x.AccountID == obj.AccountID).ToList();
                var homerequests = _db.homeRequests.Where(x => x.ResidentID == obj.AccountID).ToList();
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
                if (forHomes != null) {
                    _db.homesLists.RemoveRange(forHomes);
                }
                if (homerequests != null) {
                    _db.homeRequests.RemoveRange(homerequests);
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
            if (useracc == null)
            {
                return RedirectToAction("Error", "MainMenu", ID);
            }
            var cars = _db.Vehicle_Information.AsNoTracking().Where(_ => _.AccountID == ID);
            if (useracc.Role.Equals("Homeowner"))
            {
                ViewData["HomeCount"] = _db.homesLists.Where(_ => _.AccountID == ID).AsNoTracking().Count() == 0 ? "N/A" : _db.homesLists.Where(_ => _.AccountID == ID).AsNoTracking().Count();
            }
            else {
                var count = _db.homeRequests.Where(_ => _.ResidentID == ID && _.Status.Equals("Approved")).AsNoTracking().Count();
                ViewData["HomeCount"] = _db.homeRequests.Where(_ => _.ResidentID == ID && _.Status.Equals("Approved")).AsNoTracking().Count() == 0 ? "N/A" : _db.homeRequests.Where(_ => _.ResidentID == ID && _.Status.Equals("Approved")).AsNoTracking().Count();
            }
            model.CheckMe = _db.userFeesStatuses.Where(x => x.AccountID == ID).Select(vm => new CheckBoxItem()
            {
                ID = vm.UserFeeID,
                FeesName = vm.FeesName,
                TypeOfFees = vm.TypeOfFees,
                IsChecked = vm.Status.Equals("Enabled") ? true : false
            }).ToList();
            
            model.Vehicles = cars;
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
                    AccountID = info.AccountID,
                    Role = info.Role
                };
                _db.User_Accounts.Add(info);
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Create User Account",
                    LogDescription = "Logged In Username:" + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                //This error only happens if created at the same time frame
                var existingUser = await _db.User_Accounts.FirstOrDefaultAsync(_ => _.Username == info.Username) != null ? true : false;
                if (existingUser)
                {

                    return BadRequest("Error Bad Request Username already Exist");
                }
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
                var amount = _db.Due_Details.Where(x => x.Status == "Unpaid").ToList();
                ViewData["Total Amount Pending"] = amount.Sum(i => Convert.ToInt32(i.Amount));
                var result = (from due in _db.Due_Details
                              join
                              homeacc in _db.Homeowner_Details on due.AccountID equals homeacc.AccountID
                              where homeacc.Role == "Homeowner"
                              select new Dues
                              {
                                  AccountID = due.AccountID,
                                  FullName = homeacc.Firstname + " " + homeacc.Middlename + " " + homeacc.Surname,
                                  Amount = due.Amount,
                                  Date = due.Date,
                                  Invoice = due.Invoice,
                                  FeesName = due.FeesName,
                                  TypeOfFee = due.TypeOfFee,
                                  Status = due.Status,
                              });
                NewModel model = new NewModel();
                model.Homeacc = _db.Homeowner_Details.ToList();
                model.Userfeestatuses = _db.userFeesStatuses.ToList();
                model.feesLists = _db.feesLists.OrderBy(_ => _.TypeOfFees).ToList();
                model.dues = new Dues();
                model.ListDues = result.ToList();
                return View(model);
            }
            return RedirectToAction("AccessDenied", "MainMenu");

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
                          userfees in _db.userFeesStatuses on homeacc.AccountID equals userfees.AccountID where userfees.Status.Equals("Enabled") && userfees.TypeOfFees.Equals(userdata) && homeacc.Role == "Homeowner"
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
        public async Task<IActionResult> _CreateDues(string AccountID, string Amount,string Penalty ,string TypeofDues, DateOnly Date, string Status, string FeesName)
        {

            if (ModelState.IsValid)
            {
                _db.Due_Details.Add(new Dues
                {
                    FeesName = FeesName.Substring(0, FeesName.Length - 2),
                    AccountID = Convert.ToInt32(AccountID),
                    Invoice = GenerateText(6),
                    Amount = Amount,
                    Penalty= Penalty,
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

        [HttpPost]
        public IActionResult _ExportDuesExcel(string startDate,string endDate ,string frequency) {
            string nameformat = string.Empty;
            var Dues = (from dues in _db.Due_Details
                        join
                        homeacc in _db.Homeowner_Details on dues.AccountID equals homeacc.AccountID
                        where homeacc.Role == "Homeowner"
                        select new
                        {
                            FullName = homeacc.FullName,
                            Invoice = dues.Invoice,
                            FeesName = dues.FeesName,
                            TypeOfFee = dues.TypeOfFee,
                            Date = dues.Date,
                            Status = dues.Status,
                            Amount = dues.Amount,
                        });

            DataTable dt = new DataTable("Student");
            


            var result = Dues.ToList();
            int total = 0;
            switch (frequency)
            {
                case "Daily":
                    dt.Columns.AddRange(new DataColumn[3] {
                                            new DataColumn("Date"),
                                            new DataColumn("Amount"),
                                            new DataColumn("")});
                    result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Paid").ToList();
                    total = result.Sum(_ => Convert.ToInt32(_.Amount));
                    var dailySummary = result
                            .GroupBy(d => d.Date)
                            .Select(g => new Daily
                            {
                                Date = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Date).ToList();
                    foreach (var loglists in dailySummary)
                    {
                        dt.Rows.Add(loglists.Date, loglists.Amount);

                    }
                    dt.Rows.Add("", "Total Collected Dues: " + total);
                    dt.Rows.Add();
                    break;
                case "Weekly":
                    dt.Columns.AddRange(new DataColumn[] {
                                            new DataColumn("Year"),
                                            new DataColumn("Month"),
                                            new DataColumn("Week"),
                                            new DataColumn("Amount"),
                                            new DataColumn("")});
                    result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Paid").ToList();
                    total = result.Sum(_ => Convert.ToInt32(_.Amount));
                    var weeklySummary = result
                            .GroupBy(d => new { Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.Date.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday), Months = d.Date.Month, Years = d.Date.Year })
                            .Select(g => new Weekly
                            {
                                Year = g.Key.Years,
                                Month = g.Key.Months,
                                Week = g.Key.Week,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Year).ThenBy(_ => _.Week).ToList();
                    foreach (var loglists in weeklySummary)
                    {
                        var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(loglists.Month);
                        dt.Rows.Add(loglists.Year,thismonth,loglists.Week, loglists.Amount);

                    }
                    dt.Rows.Add("", "", "", "Total Collected Dues: " + total);
                    dt.Rows.Add();
                    break;
                case "Monthly":
                    dt.Columns.AddRange(new DataColumn[] {
                                            new DataColumn("Year"),
                                            new DataColumn("Month"),
                                            new DataColumn("Amount"),
                                            new DataColumn("")});
                    result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Paid").ToList();
                    total = result.Sum(_ => Convert.ToInt32(_.Amount));
                    var monthlySummary = result
                            .GroupBy(d => new { d.Date.Year, d.Date.Month })
                            .Select(g => new Monthly
                            {
                                Year = g.Key.Year,
                                Month = g.Key.Month,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Month).ToList();
                    foreach (var loglists in monthlySummary)
                    {
                        var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(loglists.Month);
                        dt.Rows.Add(loglists.Year, thismonth, loglists.Amount);

                    }
                    dt.Rows.Add("", "", "Total Collected Dues: " + total);
                    dt.Rows.Add();
                    break;
                case "Yearly":
                    dt.Columns.AddRange(new DataColumn[] {
                                            new DataColumn("Year"),
                                            new DataColumn("Amount"),
                                            new DataColumn("")});
                    result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(startDate).AddYears(1) && _.Status == "Paid").ToList();
                    total = result.Sum(_ => Convert.ToInt32(_.Amount));
                    var yearlySummary = result
                            .GroupBy(d => d.Date.Year)
                            .Select(g => new Yearly
                            {
                                Year = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ToList();
                    foreach (var loglists in yearlySummary)
                    {
                        dt.Rows.Add(loglists.Year, loglists.Amount);

                    }
                    dt.Rows.Add("", "Total Collected Dues: " + total);
                    dt.Rows.Add();
                    break;
            }
            
            result = Dues.ToList();
            switch (frequency)
            {
                case "Daily":
                    result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(startDate) && _.Status == "Unpaid").ToList();
                    
                    total = result.Sum(_ => Convert.ToInt32(_.Amount));
                    var dailySummary = result
                            .GroupBy(d => d.Date)
                            .Select(g => new Daily
                            {
                                Date = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Date).ToList();
                    foreach (var loglists in dailySummary)
                    {
                        dt.Rows.Add(loglists.Date, loglists.Amount);

                    }
                    dt.Rows.Add("", "Total Non Collected Dues: " + total);
                    dt.Rows.Add("", "");
                    break;
                case "Weekly":
                    result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(startDate).AddDays(7) && _.Status == "Unpaid").ToList();
                    total = result.Sum(_ => Convert.ToInt32(_.Amount));
                    var weeklySummary = result
                            .GroupBy(d => new { Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.Date.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday), Months = d.Date.Month, Years = d.Date.Year })
                            .Select(g => new Weekly
                            {
                                Year = g.Key.Years,
                                Month = g.Key.Months,
                                Week = g.Key.Week,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Year).ThenBy(_ => _.Week).ToList();
                    foreach (var loglists in weeklySummary)
                    {
                        var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(loglists.Month);
                        dt.Rows.Add(loglists.Year, thismonth, loglists.Week, loglists.Amount);

                    }
                    dt.Rows.Add("", "", "", "Total  Non Collected Dues: " + total);
                    dt.Rows.Add();
                    break;
                case "Monthly":
                    result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Unpaid").ToList();
                    total = result.Sum(_ => Convert.ToInt32(_.Amount));
                    var monthlySummary = result
                            .GroupBy(d => new { d.Date.Year, d.Date.Month })
                            .Select(g => new Monthly
                            {
                                Year = g.Key.Year,
                                Month = g.Key.Month,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Month).ToList();
                    foreach (var loglists in monthlySummary)
                    {
                        var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(loglists.Month);
                        dt.Rows.Add(loglists.Year, thismonth, loglists.Amount);

                    }
                    dt.Rows.Add("", "", "Total: " + total);
                    dt.Rows.Add();
                    break;
                case "Yearly":
                    result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Unpaid").ToList();
                    total = result.Sum(_ => Convert.ToInt32(_.Amount));
                    total = result.Sum(_ => Convert.ToInt32(_.Amount));
                    var yearlySummary = result
                            .GroupBy(d => d.Date.Year)
                            .Select(g => new Yearly
                            {
                                Year = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ToList();
                    foreach (var loglists in yearlySummary)
                    {
                        dt.Rows.Add(loglists.Year, loglists.Amount);

                    }
                    dt.Rows.Add("", "Total Non Collected Dues: " + total);
                    dt.Rows.Add();
                    break;
            }
            
            dt.Rows.Add("Report Name: " + frequency + " Dues Report From " + startDate +" To "+endDate, "Generated By: ADMIN", "Generated Date: " + DateTime.Now.ToString("MMM dd, yyyy"));
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    _db.logsLists.Add(new LogsList
                    {
                        LogName = "Export Dues",
                        LogDescription = "Export FileType: Excel " + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                        LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                    });
                    _db.SaveChanges();
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_Logs.xlsx");
                }
            }
        }

        [HttpPost]
        public IActionResult _ExportDuesPDF(string startDate,string endDate, string frequency) {
            string nameformat = string.Empty;
            var Dues = (from dues in _db.Due_Details
                        join
                        homeacc in _db.Homeowner_Details on dues.AccountID equals homeacc.AccountID
                        where homeacc.Role == "Homeowner"
                        select new
                        {
                            FullName = homeacc.FullName,
                            Invoice = dues.Invoice,
                            FeesName = dues.FeesName,
                            TypeOfFee = dues.TypeOfFee,
                            Date = dues.Date,
                            Status = dues.Status,
                            Amount = dues.Amount
                        });

            if (nameformat == null)
            {
                nameformat = "ExportPdf";
            }
            string filepath = "wwwroot\\ExportedFiles\\" + nameformat + "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_Logs.pdf";
            QuestPDF.Settings.License = LicenseType.Community;
            void ComposeTable(IContainer container,IContainer container2)
            {
                container.Border(5).Table(table =>
                {
                    var result = Dues.ToList();
                    int total = 0;
                    switch (frequency)
                    {
                        case "Daily":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Paid").ToList();

                            var dailySummary = result
                            .GroupBy(d => d.Date)
                            .Select(g => new Daily
                            {
                                Date = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Date).ToList();

                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Collected Dues");
                                header.Cell().Element(Block).Text("Date");
                                header.Cell().Element(Block).Text("Amount");
                            });

                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in dailySummary)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.Date.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Collected Dues: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Weekly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Paid").ToList();

                            var weeklySummary = result
                            .GroupBy(d => new { Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.Date.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday), Months = d.Date.Month, Years= d.Date.Year })
                            .Select(g => new Weekly
                            {
                                Year = g.Key.Years,
                                Month = g.Key.Months,
                                Week = g.Key.Week,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Year).ThenBy(_ => _.Week).ToList();

                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(4).Element(Block).Text("Collected Dues");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Week");
                                header.Cell().Element(Block).Text("Amount");
                            });

                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in weeklySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Week.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(4).Element(Block).Text("Total Collected Dues: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Monthly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Paid").ToList();
                            var monthlySummary = result
                            .GroupBy(d => new { d.Date.Year, d.Date.Month })
                            .Select(g => new Monthly
                            {
                                Year = g.Key.Year,
                                Month = g.Key.Month,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Month).ToList();

                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(3).Element(Block).Text("Collected Dues");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Amount");
                            });

                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in monthlySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(3).Element(Block).Text("Total Collected Dues: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Yearly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Paid").ToList();
                            var yearlySummary = result
                            .GroupBy(d => d.Date.Year)
                            .Select(group => new Yearly
                            {
                            Year = group.Key,
                            Amount = group.Sum(d => decimal.Parse(d.Amount ?? "0")) // Handle potential null Amount values
                            })
                            .OrderBy(_ => _.Year).ToList();

                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Collected Dues");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Amount");
                            });



                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in yearlySummary)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Collected Dues: " + total.ToString()).FontSize(11);
                            }
                            break;
                    }

                    
                });
                container2.Border(5).Table(table =>
                {

                    var result = Dues.ToList();
                    int total = 0;
                    switch (frequency)
                    {
                        case "Daily":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Unpaid").ToList();

                            var dailySummary = result
                            .GroupBy(d => d.Date)
                            .Select(g => new Daily
                            {
                                Date = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Date).ToList();

                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Non Collected Dues");
                                header.Cell().Element(Block).Text("Date");
                                header.Cell().Element(Block).Text("Amount");
                            });

                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in dailySummary)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.Date.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Non Collected Dues: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Weekly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Unpaid").ToList();
                            var weeklySummary = result
                            .GroupBy(d => new { Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.Date.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday), Months = d.Date.Month, Years = d.Date.Year })
                            .Select(g => new Weekly
                            {
                                Year = g.Key.Years,
                                Month = g.Key.Months,
                                Week = g.Key.Week,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Year).ThenBy(_ => _.Week).ToList();

                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(4).Element(Block).Text("Non Collected Dues");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Week");
                                header.Cell().Element(Block).Text("Amount");
                            });

                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in weeklySummary)
                                {

                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Week.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(4).Element(Block).Text("Total Non Collected Dues: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Monthly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Unpaid").ToList();
                            var monthlySummary = result
                            .GroupBy(d => new { d.Date.Year, d.Date.Month })
                            .Select(g => new Monthly
                            {
                                Year = g.Key.Year,
                                Month = g.Key.Month,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Month).ToList();

                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(3).Element(Block).Text("Non Collected Dues");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Amount");
                            });

                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in monthlySummary)
                                {

                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(3).Element(Block).Text("Total Non Collected Dues: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Yearly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            result = Dues.Where(_ => _.Date >= DateOnly.Parse(startDate) && _.Date <= DateOnly.Parse(endDate) && _.Status == "Unpaid").ToList();
                            var yearlySummary = result
                            .GroupBy(d => d.Date.Year)
                            .Select(group => new Yearly
                            {
                                Year = group.Key,
                                Amount = group.Sum(d => decimal.Parse(d.Amount ?? "0")) // Handle potential null Amount values
                            })
                            .OrderBy(_ => _.Year).ToList();

                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Non Collected Dues");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Amount");
                            });

                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in yearlySummary)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Non Collected Dues: " + total.ToString()).FontSize(11);
                            }
                            break; 
                    }
                });
                
            }
            Document.Create(Print =>
            {
                Print.Page(page =>
                {
                    page.Margin(10);
                    page.Header()
                    .Row(row => {
                        row.RelativeItem().Column(column => {
                            column.Item()
                            .Text("Report Name: "+frequency+" Dues Report Start Date: "+startDate).FontSize(11).AlignLeft();
                            column.Item()
                            .Text("Generated By: Admin").FontSize(11).AlignLeft();
                            
                        });
                        row.RelativeItem().Column(column => {
                            column.Item()
                            .Text("Generated Date: " + DateTime.Now.ToString("MMM dd yyyy"))
                            .FontSize(11).AlignRight();
                        });
                    });
                    page.Content()
                    .Column(c =>ComposeTable(c.Item().PaddingBottom(25).PaddingTop(25),c.Item()));
                    page.Size(PageSizes.A4);
                });
            }).ShowInCompanion(12500); //RENAMING USING RANDOM WORDS ShowInCompanion(12500) GeneratePdf(filepath)
            _db.logsLists.Add(new LogsList
            {
                LogName = "Export Dues",
                LogDescription = "Export FileType: PDF " + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                LogUserRole = "" + HttpContext.Session.GetString("UserType"),
            });
            _db.SaveChanges();
            return File(System.IO.File.ReadAllBytes(filepath), "application/pdf", nameformat + "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_ExportDues.pdf");
        }
        static IContainer Block(IContainer container)
        {
            return container
                .Border(1)
                .Background(Colors.Grey.Lighten3)
                .ShowOnce()
                .MinWidth(50)
                .MinHeight(50)
                .AlignCenter()
                .AlignMiddle();
        }
        //Logs Controller
        public IActionResult Logs()
        {
            if (CheckRole())
            {

                return View(_db.logsLists.ToList());
            }
            return RedirectToAction("AccessDenied", "MainMenu");
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
                        header.Cell().ColumnSpan(7).Element(Block).Text("Logs");
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
                    .Row(row => {
                        row.RelativeItem().Column(column => {
                            column.Item()
                            .Text("Report Name: Log Report").FontSize(11).AlignCenter();
                            column.Item()
                            .Text("Generated By: Admin").FontSize(11).AlignLeft();

                        });
                        row.RelativeItem().Column(column => {
                            column.Item()
                            .Text("Generated Date: " + DateTime.Now.ToString("MMM dd yyyy"))
                            .FontSize(11).AlignRight();

                        });
                    });

               });
           }).GeneratePdf(filepath); //RENAMING USING RANDOM WORDS
            _db.logsLists.Add(new LogsList
            {
                LogName = "Export Logs",
                LogDescription = "Export FileType: PDF " + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
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
            dt.Rows.Add("Report Name: Logs","Generated By: ADMIN", "Generated Date: " + DateTime.Now.ToString("MMM dd, yyyy"));
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
                LogDescription = "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                LogUserRole = "" + HttpContext.Session.GetString("UserType"),
            });
            return RedirectToAction("Logs");

        }
        //Chat
        public IActionResult Chat() {
            return PartialView();
        }
        //Guards Controller
        public IActionResult Guards()
        {
            if (CheckRole()) {
                return View(_db.Guard_Information.ToList());
            }
            return RedirectToAction("AccessDenied", "MainMenu");
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
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
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
            return RedirectToAction("AccessDenied", "MainMenu");
        }

        public IActionResult _CreateVehicle() { return PartialView(); }
        [HttpPost]
        public async Task<IActionResult> _CreateVehicle(NewModel info,string vehicletype)
        {
            if (info.Vehicle.RFID_number == null) {
                info.Vehicle.RFID_number = "N/A";
            }
            info.Vehicle = new Vehicle_Information{ FullName = info.Vehicle.FullName, AccountID = info.Vehicle.AccountID, PlateNo = info.Vehicle.PlateNo, RFID_number = info.Vehicle.RFID_number, VehicleModel = info.Vehicle.VehicleModel, VehicleType = vehicletype, VehicleID = info.Vehicle.VehicleID };
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
                    LogDescription = "Vehicle Owner: " + info.FullName + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                _db.SaveChanges();
            }
            return RedirectToAction("VehicleManagement");
        }


        //Fees Controller
        public IActionResult Fees() {
            if (CheckRole()) {
                return View(_db.feesLists.ToList());
            }
            return RedirectToAction("AccessDenied", "MainMenu");
        }

        public IActionResult _CreateFees() { return PartialView(); }
        [HttpPost]
        public async Task<IActionResult> _CreateFees(FeesList info)
        {
            if (ModelState.IsValid)
            {
                _db.feesLists.Add(info);
                _db.SaveChanges();
                var data1 = _db.Homeowner_Details.Where(_ => _.Role == "Homeowner").ToList();
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
            if (CheckRole()) {
                var result = _db.Admin_Accounts.FirstOrDefault(_ => _.AccountID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
                return View(result);
            }
            return RedirectToAction("AccessDenied", "MainMenu");
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
                model.Homeacc = _db.Homeowner_Details.Where(_ => _.Role == "Homeowner").ToList();
                return View(model);
            }
            return RedirectToAction("AccessDenied", "MainMenu");
        }
        public IActionResult _AddHomesList() {
            return PartialView();
        }
        [HttpPost]
        public IActionResult _AddHomesList(NewModel info) {
            if (info.Home != null) {
                _db.homesLists.Add(info.Home);
                _db.SaveChanges();
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Add Homes List",
                    LogDescription = "Added Home" + info.Home.HomeName + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                _db.SaveChanges();
            }
            return RedirectToAction("HomesList");
        }
        public IActionResult _EditHomesList(int? id) {
            var result = _db.homesLists.Where(_ => _.HomeID == id).FirstOrDefault();
            if (result == null) {
                return NotFound();
            }
            return PartialView(result);
        }
        [HttpPost]
        public IActionResult _EditHomesList(HomesList info)
        {
            if (ModelState.IsValid) {
                _db.homesLists.Update(info);
                _db.SaveChanges();
                return RedirectToAction("HomesList", "MainMenu");
            }
            return RedirectToAction("HomesList", "MainMenu");
        }
        public IActionResult _DeleteHomesList(int? id)
        {
            var result = _db.homesLists.Where(_ => _.HomeID == id).FirstOrDefault();
            if (result == null)
            {
                return NotFound();
            }
            return PartialView(result);
        }
        //MaintenanceRequests
        public IActionResult MaintenanceRequest()
        {
            NewModel model = new NewModel();
            model.maintenances = _db.maintenanceRequests.ToList();
            model.Homeacc = _db.Homeowner_Details.ToList();
            return View(model);
        }
        public IActionResult _MaintenanceDetails(int? id) {
            if (id == null)
            {
                return NotFound();
            }
            NewModel model = new NewModel();
            model.maintenance = _db.maintenanceRequests.FirstOrDefault(_ => _.RequestID == id);
            model.Homeowner = _db.Homeowner_Details.FirstOrDefault(_=>_.AccountID == model.maintenance.AccountID);
            if (model.Homeowner == null && model.maintenance==null)
            {
                return NotFound();
            }
            
            return View(model);
        }
        public IActionResult _MaintenancePending(int? id) {
            if (id == null)
            {
                return NotFound();
            }
            var result = _db.maintenanceRequests.FirstOrDefault(_ => _.RequestID == id);
            if (result == null)
            {
                return NotFound();
            }
            result.RequestStatus = "Pending";
            _db.maintenanceRequests.Update(result);
            _db.SaveChanges();
            return RedirectToAction("MaintenanceRequest");
        }
        public IActionResult _MaintenanceComplete(int? id) {
            if (id == null)
            {
                return NotFound();
            }
            var result = _db.maintenanceRequests.FirstOrDefault(_ => _.RequestID == id);
            if (result == null)
            {
                return NotFound();
            }
            result.RequestStatus = "Completed";
            result.CompletionDate = DateTime.Now;
            _db.maintenanceRequests.Update(result);
            _db.SaveChanges();
            return RedirectToAction("MaintenanceRequest");
        }
        public IActionResult _MaintenanceReject(int? id) {
            if (id == null)
            {
                return NotFound();
            }
            var result = _db.maintenanceRequests.FirstOrDefault(_ => _.RequestID == id);
            if (result == null)
            {
                return NotFound();
            }
            result.RequestStatus = "Rejected";
            _db.maintenanceRequests.Update(result);
            _db.SaveChanges();
            return RedirectToAction("MaintenanceRequest");
        }

        public IActionResult Pets() {
            NewModel model = new NewModel();
            model.pets = _db.PetInformation.ToList();
            model.Homeacc = _db.Homeowner_Details.ToList();
            return View(model);
        }
        [HttpPost]
        public IActionResult _DeleteHomesList(HomesList info) {
            if (ModelState.IsValid)
            {
                _db.homesLists.Remove(info);
                _db.SaveChanges();
                return RedirectToAction("HomesList", "MainMenu");
            }
            return RedirectToAction("HomesList", "MainMenu");
        }

        //Officers

        public IActionResult Officers() {
            NewModel model = new NewModel();
            model.Homeacc = _db.Homeowner_Details.ToList();
            model.Officers = _db.Officers.ToList();
            return View(model);
        }
        public IActionResult _SetPosition(int ID)
        {
            Officers officers = new Officers { AccountID = ID,Position =string.Empty, ID=0 };
            return View(officers);
        }
        [HttpPost]
        public IActionResult _SetPosition(Officers  info)
        {
            _db.Officers.Add(info);
            _db.SaveChanges();
            return RedirectToAction("Officers");
        }
        public IActionResult _EditPosition(int? ID) {
            if (ID == null) { return NotFound(); }
            var result = _db.Officers.FirstOrDefault(x => x.ID == ID);
            if (result == null) {
                return NotFound();
            }
            return View(result);
        }
        [HttpPost]
        public IActionResult _EditPosition(Officers info)
        {
            _db.Officers.Update(info);
            _db.SaveChanges();
            return RedirectToAction("Officers");
        }
        public IActionResult _DeletePosition(int? ID)
        {
            if (ID == null) { return NotFound(); }
            var result = _db.Officers.FirstOrDefault(x => x.ID == ID);
            if (result == null)
            {
                return NotFound();
            }
            _db.Officers.Remove(result);
            _db.SaveChanges();
            return RedirectToAction("Officers");
        }

        //Income And Non Income Generating Project
        public IActionResult Events() {
            NewModel model = new NewModel();
            model.Events = _db.Events.ToList();
            model.EventsReserved = _db.EventsReserved.ToList();
            return View(model);
        }
        public IActionResult _JoinedMembers(int? id) {
            if (id == null) {
                return NotFound();
            }
            NewModel model = new NewModel();
            model.EventsReserved = _db.EventsReserved.Where(_=>_.EventID == id).ToList();
            model.Homeacc = _db.Homeowner_Details.ToList();
            return View(model);
        }
        public IActionResult _AddEvents(Events info) {
            _db.Events.Add(info);
            _db.SaveChanges();
            return RedirectToAction("Events");
        }
        public IActionResult _EditEvents(int? id) {
            if (id == null) { return NotFound(); }
            var result = _db.Events.FirstOrDefault(_=>_.EventID == id);
            if (result == null) {
                return NotFound();
            }
            return View(result); 
        }

        [HttpPost]
        public IActionResult _EditEvents(Events info)
        {
            _db.Events.Update(info);
            _db.SaveChanges();
            return RedirectToAction("Events");
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
        
        public IActionResult _DeleteEvents(int? id) {
            if (id == null) { return NotFound(); }
            var result = _db.Events.FirstOrDefault(_ => _.EventID == id);
            if (result == null)
            {
                return NotFound();
            }
            _db.Events.Remove(result);
            _db.SaveChanges();
            return RedirectToAction("Events");
        }
        //Facilities
        [HttpPost]
        public IActionResult _ExportFacilityPDF(string startDate, string endDate, string frequency)
        {
            
            string nameformat = string.Empty;
            
            if (nameformat == null)
            {
                nameformat = "ExportPdf";
            }
            string filepath = "wwwroot\\ExportedFiles\\" + nameformat + "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_Logs.pdf";
            QuestPDF.Settings.License = LicenseType.Community;
            void ComposeTable(IContainer container,IContainer container2)
            {
                container.Border(5).Table(table =>
                {
                    var facilities = _db.Facilities.ToList();
                    var reservations = _db.FacilitiesReserved.ToList();
                    var facilityUsageReports = new List<FacilityUsageReport>();
                    switch (frequency)
                    {
                        case "Daily":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            var dailyReports = new List<DailyFacilityUsageReport>();

                            var dailyReservations = reservations
                                .Where(_ => DateOnly.FromDateTime(_.StartTime) >= DateOnly.Parse(startDate) && DateOnly.FromDateTime(_.EndTime) <= DateOnly.Parse(endDate))
                                .GroupBy(r => r.StartTime.Date);

                            foreach (var group in dailyReservations)
                            {
                                var date = group.Key;
                                var facilityUsages = new List<FacilityUsage>();

                                foreach (var facility in facilities)
                                {
                                    var facilityReservations = group.Where(r => r.FacilityID == facility.FacilityID);
                                    var totalUsageHours = facilityReservations.Sum(r => (r.EndTime - r.StartTime).TotalHours);

                                    facilityUsages.Add(new FacilityUsage
                                    {
                                        FacilityName = facility.FacilityName,
                                        TotalUsageHours = totalUsageHours
                                    });
                                }

                                dailyReports.Add(new DailyFacilityUsageReport
                                {
                                    Date = date,
                                    FacilityUsages = facilityUsages
                                });
                            }
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(3).Element(Block).Text("Daily Facility Usage");
                                header.Cell().Element(Block).Text("Date");
                                header.Cell().Element(Block).Text("Facility Name");
                                header.Cell().Element(Block).Text("Total Usage Hours");

                            });

                            if (dailyReports.Count != 0)
                            {
                                foreach (var item in dailyReports)
                                {

                                    foreach (var item2 in item.FacilityUsages) {
                                        table.Cell().RowSpan(2).Element(Block).Text(item.Date.ToString("yyyy-MM-dd")).FontSize(11);
                                        table.Cell().RowSpan(2).Element(Block).Text(item2.FacilityName.ToString()).FontSize(11);
                                        table.Cell().RowSpan(2).Element(Block).Text(item2.TotalUsageHours.ToString()).FontSize(11);
                                    }

                                }
           
                            }
                            break;
                        case "Weekly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            var weeklyReports = new List<WeeklyFacilityUsageReport>();

                            // Group reservations by week
                            var weeklyReservations = reservations
                                .Where(_ => DateOnly.FromDateTime(_.StartTime) >= DateOnly.Parse(startDate) && DateOnly.FromDateTime(_.EndTime) <= DateOnly.Parse(endDate))
                                .GroupBy(r => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(r.StartTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday));

                            foreach (var group in weeklyReservations)
                            {
                                var weekNumber = group.Key;
                                var facilityUsages = new List<FacilityUsage>();

                                foreach (var facility in facilities)
                                {
                                    var facilityReservations = group.Where(r => r.FacilityID == facility.FacilityID);
                                    var totalUsageHours = facilityReservations.Sum(r => (r.EndTime - r.StartTime).TotalHours);

                                    facilityUsages.Add(new FacilityUsage
                                    {
                                        FacilityName = facility.FacilityName,
                                        TotalUsageHours = totalUsageHours
                                    });
                                }

                                weeklyReports.Add(new WeeklyFacilityUsageReport
                                {
                                    WeekNumber = weekNumber,
                                    FacilityUsages = facilityUsages
                                });
                            }
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(3).Element(Block).Text("Weekly Facility Usage");
                                header.Cell().Element(Block).Text("Week");
                                header.Cell().Element(Block).Text("Facility Name");
                                header.Cell().Element(Block).Text("Total Usage Hours");

                            });

                            if (weeklyReports.Count != 0)
                            {
                                foreach (var item in weeklyReports)
                                {
                                    foreach (var item2 in item.FacilityUsages) {
                                        table.Cell().RowSpan(2).Element(Block).Text(item.WeekNumber.ToString()).FontSize(11);
                                        table.Cell().RowSpan(2).Element(Block).Text(item2.FacilityName).FontSize(11);
                                        table.Cell().RowSpan(2).Element(Block).Text(item2.TotalUsageHours.ToString()).FontSize(11);
                                    }
                                }
                            }
                            break;
                        case "Monthly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            var monthlyReports = new List<MonthlyFacilityUsageReport>();

                            var monthlyReservations = reservations
                                .Where(_ => DateOnly.FromDateTime(_.StartTime) >= DateOnly.Parse(startDate) && DateOnly.FromDateTime(_.EndTime) <= DateOnly.Parse(endDate))
                                .GroupBy(r => new {r.StartTime.Year, r.EndTime.Month });

                            foreach (var group in monthlyReservations)
                            {
                                var year = group.Key.Year;
                                var month = group.Key.Month;
                                var facilityUsages = new List<FacilityUsage>();

                                foreach (var facility in facilities)
                                {
                                    var facilityReservations = group.Where(r => r.FacilityID == facility.FacilityID);
                                    var totalUsageHours = facilityReservations.Sum(r => (r.EndTime - r.StartTime).TotalHours);

                                    facilityUsages.Add(new FacilityUsage
                                    {
                                        FacilityName = facility.FacilityName,
                                        TotalUsageHours = totalUsageHours
                                    });
                                }

                                monthlyReports.Add(new MonthlyFacilityUsageReport
                                {
                                    Year = year,
                                    Month = month,
                                    FacilityUsages = facilityUsages
                                });
                            }
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(4).Element(Block).Text("Monthly Facility Usage");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Facility Name");
                                header.Cell().Element(Block).Text("Usage Hours");
                            });
                            if (monthlyReports.Count != 0)
                            {
                                foreach (var item in monthlyReports)
                                {
                                    foreach (var item2 in item.FacilityUsages) {
                                        var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                        table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                        table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                        table.Cell().RowSpan(2).Element(Block).Text(item2.FacilityName).FontSize(11);
                                        table.Cell().RowSpan(2).Element(Block).Text(item2.TotalUsageHours.ToString()).FontSize(11);
                                    }

                                }
                            }
                            break;
                        case "Yearly":
                            reservations = reservations.Where(_ => DateOnly.FromDateTime(_.StartTime) >= DateOnly.Parse(startDate) && DateOnly.FromDateTime(_.EndTime) <= DateOnly.Parse(endDate)).ToList();
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            foreach (var facility in facilities) {
                               var result2 = reservations.Where(_ => _.FacilityID == facility.FacilityID);
                                double totalUsageHours = 0;
                                DateTime peakUsageDate = DateTime.MinValue;
                                double peakUsageHours = 0;
                                foreach (var reservation in result2) {
                                    var usageHours = (reservation.EndTime - reservation.StartTime).TotalHours;
                                    totalUsageHours += usageHours;
                                    if (usageHours > peakUsageHours) {
                                        peakUsageDate = reservation.StartTime;
                                        peakUsageHours = usageHours;
                                    }
                                }
                                var averageDailyUsage = totalUsageHours / 365;
                                facilityUsageReports.Add(new FacilityUsageReport
                                {
                                    FacilityId = facility.FacilityID,
                                    FacilityName = facility.FacilityName,
                                    TotalUsageHours = totalUsageHours,
                                    AverageDailyUsage = averageDailyUsage,
                                    PeakUsageDate = peakUsageDate,
                                    PeakUsageHours = peakUsageHours
                                });
                            }

                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(5).Element(Block).Text("Facility Usage");
                                header.Cell().Element(Block).Text("Facility Name");
                                header.Cell().Element(Block).Text("Total Usage Hours");
                                header.Cell().Element(Block).Text("Average Daily Usage");
                                header.Cell().Element(Block).Text("Peak Usage Date");
                                header.Cell().Element(Block).Text("Peak Usage Hours");
                            });



                            if (facilityUsageReports.Count != 0)
                            {
                                foreach (var item in facilityUsageReports)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.FacilityName.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.TotalUsageHours.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.AverageDailyUsage.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.PeakUsageDate.ToString("yyyy")).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.PeakUsageHours.ToString()).FontSize(11);

                                }
                               
                            }
                            break;
                    }


                });
                container2.Border(5).Table(table =>
                {
                    var result = (from facility in _db.Facilities 
                                  join facilityreserved in _db.FacilitiesReserved on facility.FacilityID equals facilityreserved.FacilityID
                                  select new { 
                                    FacilityName = facility.FacilityName,
                                    StartTime = facilityreserved.StartTime,
                                    EndTime = facilityreserved.EndTime,
                                    Amount = facility.RentalFee,
                                    Date = facilityreserved.DateOFReservation
                                  }).ToList();
                    var facilities = _db.Facilities.ToList();
                    var reservations = _db.FacilitiesReserved.ToList();
                    int total = 0;
                    switch (frequency) {
                        case "Daily":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            
                            var dailyIncome = result
                                .Where(_ => DateOnly.FromDateTime(_.StartTime) >= DateOnly.Parse(startDate) && DateOnly.FromDateTime(_.EndTime) <= DateOnly.Parse(endDate))
                                .GroupBy(d => DateOnly.FromDateTime(d.StartTime))
                                .Select(g => new Daily
                            {
                                Date = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Date).ToList();
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Facility Income");
                                header.Cell().Element(Block).Text("Income Date");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count() != 0)
                            {
                                foreach (var item in dailyIncome)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.Date.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Daily Income: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Weekly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            var weeklyIncome = result
                            .Where(_ => DateOnly.FromDateTime(_.StartTime) >= DateOnly.Parse(startDate) && DateOnly.FromDateTime(_.EndTime) <= DateOnly.Parse(endDate))
                                .GroupBy(d => new { Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.Date.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday), Months = d.Date.Month, Years = d.Date.Year })
                                .Select(g => new Weekly
                                {
                                Year = g.Key.Years,
                                Month = g.Key.Months,
                                Week = g.Key.Week,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                                }).OrderBy(_ => _.Year).ThenBy(_ => _.Year).ThenBy(_ => _.Week).ToList();
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(4).Element(Block).Text("Facility Income");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Week");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count() != 0)
                            {
                                foreach (var item in weeklyIncome)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Week.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(4).Element(Block).Text("Total Weekly Income: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Monthly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            var monthlyIncome = result
                            .Where(_ => DateOnly.FromDateTime(_.StartTime) >= DateOnly.Parse(startDate) && DateOnly.FromDateTime(_.EndTime) <= DateOnly.Parse(endDate))
                                .GroupBy(d => new { d.Date.Year, d.Date.Month })
                           .Select(g => new Monthly
                           {
                               Year = g.Key.Year,
                               Month = g.Key.Month,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ThenBy(_ => _.Month).ToList();
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(3).Element(Block).Text("Facility Income");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count() != 0)
                            {
                                foreach (var item in monthlyIncome)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(3).Element(Block).Text("Total Monthly Income: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Yearly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            var YearlyIncome = result.Where(_ => DateOnly.FromDateTime(_.StartTime) >= DateOnly.Parse(startDate) && DateOnly.FromDateTime(_.EndTime) <= DateOnly.Parse(endDate))
                                .GroupBy(d => d.Date.Year)
                                .Select(g => new Yearly
                                {
                                    Year = g.Key,
                                    Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                                }).OrderBy(_ => _.Year).ToList();
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Facility Income");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count() != 0)
                            {
                                foreach (var item in YearlyIncome)
                                {
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Yearly Income: " + total.ToString()).FontSize(11);
                            }
                            break;
                    }

                });
            }
            Document.Create(Print =>
            {
                Print.Page(page =>
                {
                    page.Margin(10);
                    page.Header()
                    .Row(row => {
                        row.RelativeItem().Column(column => {
                            column.Item()
                            .Text("Report Name: " + frequency + " Facility Report Start Date: " + startDate).FontSize(11).AlignLeft();
                            column.Item()
                            .Text("Generated By: Admin").FontSize(11).AlignLeft();

                        });
                        row.RelativeItem().Column(column => {
                            column.Item()
                            .Text("Generated Date: " + DateTime.Now.ToString("MMM dd yyyy"))
                            .FontSize(11).AlignRight();
                        });
                    });
                    page.Content()
                    .Column(c => ComposeTable(c.Item().PaddingBottom(25).PaddingTop(25), c.Item().PaddingBottom(25).PaddingTop(25)));
                    page.Size(PageSizes.A4);
                });
            }).ShowInCompanion(12500); //RENAMING USING RANDOM WORDS ShowInCompanion(12500) GeneratePdf(filepath)
            _db.logsLists.Add(new LogsList
            {
                LogName = "Export Facility",
                LogDescription = "Export FileType: PDF " + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                LogUserRole = "" + HttpContext.Session.GetString("UserType"),
            });
            _db.SaveChanges();
            return File(System.IO.File.ReadAllBytes(filepath), "application/pdf", nameformat + "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_ExportDues.pdf");
        }


        public IActionResult Facilities() {
            NewModel model = new NewModel();
            model.facilities = _db.Facilities.ToList();
            model.facilitiesreserved = _db.FacilitiesReserved.ToList();
            model.Homeacc = _db.Homeowner_Details.ToList();
            return View(model);
        }
        public IActionResult _FacilityDetails() {
            NewModel model = new NewModel();
            model.facilities = _db.Facilities.ToList();
            model.facilitiesreserved = _db.FacilitiesReserved.ToList();
            model.Homeacc = _db.Homeowner_Details.ToList();
            return View(model);
        }
        public IActionResult _AddFacility(Facilities info) { 
            _db.Facilities.Add(info);
            _db.SaveChanges();
            return RedirectToAction("Facilities");
        }
        public IActionResult _EditFacility(int? id)
        {
            if (id == null) { return NotFound(); }
            var result = _db.Facilities.FirstOrDefault(_ => _.FacilityID== id);
            if (result == null)
            {
                return NotFound();
            }
            return View(result);
        }
        [HttpPost]
        public IActionResult _EditFacility(Facilities info)
        {
            _db.Facilities.Update(info);
            _db.SaveChanges();
            return RedirectToAction("Facilities");
        }
        public IActionResult _DeleteFacility(int? id)
        {
            if (id == null) { return NotFound(); }
            var result = _db.Facilities.FirstOrDefault(_ => _.FacilityID == id);
            var result2 = _db.FacilitiesReserved.Where(_ => _.FacilityID == id);
            if (result == null)
            {
                return NotFound();
            }
            if (result2 != null) {
                _db.FacilitiesReserved.RemoveRange(result2);
            }
            _db.Facilities.Remove(result);
            _db.SaveChanges();
            return RedirectToAction("Facilities");
        }

        //Expense

        public IActionResult Expenses() {
            return View(_db.Expenses.ToList());
        }

        public IActionResult _AddExpense(Expenses info) {
            _db.Expenses.Add(info);
            _db.SaveChanges();
            return RedirectToAction("Expenses");
        }
        public IActionResult _EditExpense(int? id) {
            if (id == null) {
                return NotFound();
            }
            var result = _db.Expenses.FirstOrDefault(_ => _.ExpenseID == id);
            if (result == null) {
                return NotFound();
            }
            return View(result);
        }
        [HttpPost]
        public IActionResult _EditExpense(Expenses info)
        {
            _db.Expenses.Update(info);
            _db.SaveChanges();
            return RedirectToAction("Expenses");
        }
        public IActionResult _DeleteExpense(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var result = _db.Expenses.FirstOrDefault(_ => _.ExpenseID == id);
            if (result == null)
            {
                return NotFound();
            }
            _db.Expenses.Remove(result);
            _db.SaveChanges();
            return RedirectToAction("Expenses");
        }

        [HttpPost]
        public IActionResult _ExportExpensePDF(string startDate,string endDate, string frequency)
        {
            string nameformat = string.Empty;
            var Expense = _db.Expenses.ToList();

            if (nameformat == null)
            {
                nameformat = "ExportPdf";
            }
            string filepath = "wwwroot\\ExportedFiles\\" + nameformat + "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_Logs.pdf";
            QuestPDF.Settings.License = LicenseType.Community;
            void ComposeTable(IContainer container, IContainer container2, IContainer container3, IContainer container4, IContainer container5)
            {
                container.Border(5).Table(table =>
                {

                    var result = Expense.ToList();
                    int total = 0;
                    switch (frequency)
                    {
                        case "Daily":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Electricity");
                                header.Cell().Element(Block).Text("Expense Date");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) &&_.Category == "Electricity").ToList();
                            var dailySummary = result
                            .GroupBy(d => d.EzpenseDate)
                            .Select(g => new Daily
                            {
                                Date = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Date).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in dailySummary)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.Date.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Electricity Expense: " + total.ToString()).FontSize(11);
                            }

                            break;
                        case "Weekly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(4).Element(Block).Text("Electricity");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Week");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Electricity").ToList();
                            var weeklySummary = result
                            .GroupBy(d => new { Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.EzpenseDate.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday), Months = d.EzpenseDate.Month, Years = d.EzpenseDate.Year })
                            .Select(g => new Weekly
                            {
                                Year = g.Key.Years,
                                Month = g.Key.Months,
                                Week = g.Key.Week,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Year).ThenBy(_ => _.Week).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in weeklySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Week.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(4).Element(Block).Text("Total Electricity Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Monthly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(3).Element(Block).Text("Electricity");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Electricity").ToList();
                            var monthlySummary = result
                           .GroupBy(d => new { d.EzpenseDate.Year,d.EzpenseDate.Month })
                           .Select(g => new Monthly
                           {
                               Year = g.Key.Year,
                               Month = g.Key.Month,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ThenBy(_ => _.Month).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in monthlySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(3).Element(Block).Text("Total Electricity Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Yearly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Electricity");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Electricity").ToList();
                            var yearlySummary = result
                           .GroupBy(d =>d.EzpenseDate.Year)
                           .Select(g => new Yearly
                           {
                               Year = g.Key,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in yearlySummary)
                                {
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Electricity Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                    }
                    
                });
                //Water
                container2.Border(5).Table(table =>
                {
                    var result = Expense.ToList();
                    int total = 0;
                    switch (frequency)
                    {
                        case "Daily":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Water");
                                header.Cell().Element(Block).Text("Expense Date");
                                header.Cell().Element(Block).Text("Amount");

                            });

                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Water").ToList();
                            var dailySummary = result
                            .GroupBy(d => d.EzpenseDate)
                            .Select(g => new Daily
                            {
                                Date = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Date).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in dailySummary)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.Date.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Water Expense: " + total.ToString()).FontSize(11);
                            }

                            break;
                        case "Weekly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(4).Element(Block).Text("Water");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Week");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Water").ToList();
                            var weeklySummary = result
                            .GroupBy(d => new { Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.EzpenseDate.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday), Months = d.EzpenseDate.Month, Years = d.EzpenseDate.Year })
                            .Select(g => new Weekly
                            {
                                Year = g.Key.Years,
                                Month = g.Key.Months,
                                Week = g.Key.Week,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Year).ThenBy(_ => _.Week).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in weeklySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Week.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(4).Element(Block).Text("Total Water Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Monthly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(3).Element(Block).Text("Water");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Water").ToList();
                            var monthlySummary = result
                           .GroupBy(d => new { d.EzpenseDate.Year, d.EzpenseDate.Month })
                           .Select(g => new Monthly
                           {
                               Year = g.Key.Year,
                               Month = g.Key.Month,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ThenBy(_ => _.Month).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in monthlySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(3).Element(Block).Text("Total Water Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Yearly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Water");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Water").ToList();
                            var yearlySummary = result
                           .GroupBy(d => d.EzpenseDate.Year)
                           .Select(g => new Yearly
                           {
                               Year = g.Key,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in yearlySummary)
                                {
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Water Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                    }

                });
                //Maintenance
                container3.Border(5).Table(table =>
                {

                    var result = Expense.ToList();
                    int total = 0;
                    switch (frequency)
                    {
                        case "Daily":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Maintenance");
                                header.Cell().Element(Block).Text("Expense Date");
                                header.Cell().Element(Block).Text("Amount");

                            });

                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Maintenance").ToList();
                            var dailySummary = result
                            .GroupBy(d => d.EzpenseDate)
                            .Select(g => new Daily
                            {
                                Date = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Date).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in dailySummary)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.Date.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Maintenance Expense: " + total.ToString()).FontSize(11);
                            }

                            break;
                        case "Weekly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(4).Element(Block).Text("Maintenance");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Week");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Maintenance").ToList();
                            var weeklySummary = result
                            .GroupBy(d => new { Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.EzpenseDate.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday), Months = d.EzpenseDate.Month, Years = d.EzpenseDate.Year })
                            .Select(g => new Weekly
                            {
                                Year = g.Key.Years,
                                Month = g.Key.Months,
                                Week = g.Key.Week,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Year).ThenBy(_ => _.Week).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in weeklySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Week.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(4).Element(Block).Text("Total Maintenance Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Monthly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(3).Element(Block).Text("Maintenance");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Maintenance").ToList();
                            var monthlySummary = result
                           .GroupBy(d => new { d.EzpenseDate.Year, d.EzpenseDate.Month })
                           .Select(g => new Monthly
                           {
                               Year = g.Key.Year,
                               Month = g.Key.Month,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ThenBy(_ => _.Month).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in monthlySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(3).Element(Block).Text("Total Maintenance Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Yearly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Maintenance");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Maintenance").ToList();
                            var yearlySummary = result
                           .GroupBy(d => d.EzpenseDate.Year)
                           .Select(g => new Yearly
                           {
                               Year = g.Key,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in yearlySummary)
                                {
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Maintenance Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                    }
                });
                //Manpower
                container4.Border(5).Table(table =>
                {

                    var result = Expense.ToList();
                    int total = 0;
                    switch (frequency)
                    {
                        case "Daily":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Manpower");
                                header.Cell().Element(Block).Text("Expense Date");
                                header.Cell().Element(Block).Text("Amount");

                            });

                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Manpower").ToList();
                            var dailySummary = result
                            .GroupBy(d => d.EzpenseDate)
                            .Select(g => new Daily
                            {
                                Date = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Date).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in dailySummary)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.Date.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Manpower Expense: " + total.ToString()).FontSize(11);
                            }

                            break;
                        case "Weekly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(4).Element(Block).Text("Manpower");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Week");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Manpower").ToList();
                            var weeklySummary = result
                            .GroupBy(d => new { Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.EzpenseDate.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday), Months = d.EzpenseDate.Month, Years = d.EzpenseDate.Year })
                            .Select(g => new Weekly
                            {
                                Year = g.Key.Years,
                                Month = g.Key.Months,
                                Week = g.Key.Week,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Year).ThenBy(_ => _.Week).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in weeklySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Week.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(4).Element(Block).Text("Total Manpower Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Monthly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(3).Element(Block).Text("Manpower");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Manpower").ToList();
                            var monthlySummary = result
                           .GroupBy(d => new { d.EzpenseDate.Year, d.EzpenseDate.Month })
                           .Select(g => new Monthly
                           {
                               Year = g.Key.Year,
                               Month = g.Key.Month,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ThenBy(_ => _.Month).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in monthlySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(3).Element(Block).Text("Total Manpower Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Yearly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Manpower");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Manpower").ToList();
                            var yearlySummary = result
                           .GroupBy(d => d.EzpenseDate.Year)
                           .Select(g => new Yearly
                           {
                               Year = g.Key,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in yearlySummary)
                                {
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Manpower Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                    }
                });
                //Other
                container5.Border(5).Table(table =>
                {

                    var result = Expense.ToList();
                    int total = 0;
                    switch (frequency)
                    {
                        case "Daily":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Other");
                                header.Cell().Element(Block).Text("Expense Date");
                                header.Cell().Element(Block).Text("Amount");

                            });

                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Other").ToList();
                            var dailySummary = result
                            .GroupBy(d => d.EzpenseDate)
                            .Select(g => new Daily
                            {
                                Date = g.Key,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Date).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in dailySummary)
                                {

                                    table.Cell().RowSpan(2).Element(Block).Text(item.Date.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Other Expense: " + total.ToString()).FontSize(11);
                            }

                            break;
                        case "Weekly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(4).Element(Block).Text("Other");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Week");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Other").ToList();
                            var weeklySummary = result
                            .GroupBy(d => new { Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d.EzpenseDate.ToDateTime(TimeOnly.MinValue), CalendarWeekRule.FirstDay, DayOfWeek.Monday), Months = d.EzpenseDate.Month, Years = d.EzpenseDate.Year })
                            .Select(g => new Weekly
                            {
                                Year = g.Key.Years,
                                Month = g.Key.Months,
                                Week = g.Key.Week,
                                Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                            }).OrderBy(_ => _.Year).ThenBy(_ => _.Year).ThenBy(_ => _.Week).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in weeklySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Week.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(4).Element(Block).Text("Total Other Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Monthly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(3).Element(Block).Text("Other");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Month");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Other").ToList();
                            var monthlySummary = result
                           .GroupBy(d => new { d.EzpenseDate.Year, d.EzpenseDate.Month })
                           .Select(g => new Monthly
                           {
                               Year = g.Key.Year,
                               Month = g.Key.Month,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ThenBy(_ => _.Month).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in monthlySummary)
                                {
                                    var thismonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(thismonth).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(3).Element(Block).Text("Total Other Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                        case "Yearly":
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();

                            });
                            table.Header(header =>
                            {
                                header.Cell().ColumnSpan(2).Element(Block).Text("Other");
                                header.Cell().Element(Block).Text("Year");
                                header.Cell().Element(Block).Text("Amount");

                            });
                            result = Expense.Where(_ => _.EzpenseDate >= DateOnly.Parse(startDate) && _.EzpenseDate <= DateOnly.Parse(endDate) && _.Category == "Other").ToList();
                            var yearlySummary = result
                           .GroupBy(d => d.EzpenseDate.Year)
                           .Select(g => new Yearly
                           {
                               Year = g.Key,
                               Amount = g.Sum(d => decimal.Parse(d.Amount ?? "0"))
                           }).OrderBy(_ => _.Year).ToList();
                            total = result.Sum(_ => Convert.ToInt32(_.Amount));
                            if (result.Count != 0)
                            {
                                foreach (var item in yearlySummary)
                                {
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Year.ToString()).FontSize(11);
                                    table.Cell().RowSpan(2).Element(Block).Text(item.Amount.ToString()).FontSize(11);

                                }
                                table.Cell().ColumnSpan(2).Element(Block).Text("Total Other Expense: " + total.ToString()).FontSize(11);
                            }
                            break;
                    }
                });

            }
            Document.Create(Print =>
            {
                Print.Page(page =>
                {
                    page.Margin(10);
                    page.Header()
                    .Row(row => {
                        row.RelativeItem().Column(column => {
                            column.Item()
                            .Text("Report Name: " + frequency + " Expense Report Start Date: " + startDate + " To"+ endDate).FontSize(11).AlignLeft();
                            column.Item()
                            .Text("Generated By: Admin").FontSize(11).AlignLeft();

                        });
                        row.RelativeItem().Column(column => {
                            column.Item()
                            .Text("Generated Date: " + DateTime.Now.ToString("MMM dd yyyy"))
                            .FontSize(11).AlignRight();
                        });
                    });
                    page.Content()
                    .Column(c => ComposeTable(c.Item().PaddingBottom(25).PaddingTop(25), c.Item().PaddingBottom(25).PaddingTop(25), c.Item().PaddingBottom(25).PaddingTop(25), c.Item().PaddingBottom(25).PaddingTop(25), c.Item().PaddingBottom(25).PaddingTop(25)));
                    page.Size(PageSizes.A4);
                });
            }).GeneratePdf(filepath); //RENAMING USING RANDOM WORDS 
            _db.logsLists.Add(new LogsList
            {
                LogName = "Export Expense",
                LogDescription = "Export FileType: PDF " + "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                LogUserRole = "" + HttpContext.Session.GetString("UserType"),
            });
            _db.SaveChanges();
            return File(System.IO.File.ReadAllBytes(filepath), "application/pdf", nameformat + "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_ExportExpense.pdf");
        }

        //ViolationSanction
        public IActionResult Violation() {
            return View(_db.Violation.ToList());
        }
        public IActionResult _ActionViolation(int? id)
        {
            var result = _db.Violation.FirstOrDefault(_ => _.ViolationID == id);
            if (result == null) { return NotFound(); }
            return View(result);
        }
        [HttpPost]
        public IActionResult _ActionViolation(ViolationSanction info)
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
        public IActionResult DownloadByLaws() {
            string filepath = "wwwroot\\Downloadable\\ByLaws.pdf";
            return File(System.IO.File.ReadAllBytes(filepath), "application/pdf","_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "ByLaws.pdf");
        }
        //Json Result
        [HttpPost]
        public JsonResult CheckAmount(string userdata, string AccountID)
        {
            if (userdata != null)
            {

                var SearchData = _db.userFeesStatuses.Where(x => x.TypeOfFees.Equals(userdata) && x.Status.Equals("Enabled") && x.AccountID == Convert.ToInt32(AccountID));
                if (SearchData != null)
                {
                    return Json(new { Amount = SearchData.Sum(i => Convert.ToInt32(i.Amount)),Penalty = SearchData.Sum(i => Convert.ToInt32(i.Penalty)) });
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
        [HttpPost]
        public JsonResult CheckFeesName(string userdata)
        {
            System.Threading.Thread.Sleep(200);
            var SearchData = _db.feesLists.Where(x => x.FeesName == userdata).FirstOrDefault();
            if (SearchData != null)
            {
                return Json(1);
            }
            else
            {
                return Json(0);
            }
        }
        [HttpPost]
        public JsonResult CheckHome(string userdata) {
            if (userdata != null)
            {

                var SearchData = _db.homesLists.Where(x => x.AccountID == Convert.ToInt32(userdata)).ToList();
                if (SearchData != null)
                {
                    return Json(SearchData.OrderBy(_ => _.HomeName).ToList());
                }
                else
                {
                    return Json(0);
                }
            }
            return Json(0);
        }
        [HttpPost]
        public JsonResult CheckPlateNo(string userdata)
        {
            if (userdata != null)
            {

                var SearchData = _db.Vehicle_Information.Where(x => x.PlateNo == userdata).Count();
                if (SearchData != 0)
                {
                    return Json(1);
                }
                else
                {
                    return Json(0);
                }
            }
            return Json(0);
        }
        [HttpPost]
        public JsonResult CheckRfidNumber(string userdata)
        {
            if (userdata != null)
            {

                var SearchData = _db.Vehicle_Information.Where(x => x.RFID_number == userdata).Count();
                if (SearchData != 0)
                {
                    return Json(1);
                }
                else
                {
                    return Json(0);
                }
            }
            return Json(0);
        }
        [HttpGet]
        public JsonResult LoadChatHistory() {

            var result = _db.chatHistory.ToList().OrderBy(t => t.Date.Date.Minute);
            List<ChatPopulate> list = new List<ChatPopulate>();
            foreach (var item in result) {
                list.Add(new ChatPopulate { ChatID = item.ChatID, Date = item.Date.ToString("MMMM dd, yyyy h:mm tt"), Message = item.Message, UserName = item.UserName });
            }
            if (result != null) { 
                return Json(list);
            }
            return Json(0);
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
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult AccessDenied()
        {
            return View();
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
    static class SimpleExtension
    {
        private static IContainer Cell(this IContainer container, bool dark)
        {
            return container
                .Border(1)
                .Background(dark ? Colors.Grey.Lighten2 : Colors.White)
                .Padding(10);
        }

        // displays only text label
        public static void LabelCell(this IContainer container, string text) => container.Cell(true).Text(text).Medium();

        // allows you to inject any type of content, e.g. image
        public static IContainer ValueCell(this IContainer container) => container.Cell(false);
    }
}
