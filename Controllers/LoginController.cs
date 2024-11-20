using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.InfoSec;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using DocumentFormat.OpenXml.EMMA;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using MySqlConnector;
using Newtonsoft.Json;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class LoginController : Controller
    {
        const string connstr = "server=localhost;user=root;password=Kelvinfo14;database=hoa_sys";
        public static User _account = new User();
        private readonly AppDbContext _db;
        private readonly EnvironmentModel _env;
        private readonly ILogger _logger;
        MySqlConnection conn = new MySqlConnection(connstr);
        public LoginController(AppDbContext db, ILogger<LoginController> logger, IOptions<EnvironmentModel> Accessor)
        {
            _db = db;
            _logger = logger;
            _env = Accessor.Value;
        }



        public IActionResult Admin_LoginForm() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Admin_LoginForm(Admin_Account info) {
            conn.Open();
            MySqlCommand mySqlCommand = new MySqlCommand("Select Username,Password From admin_accounts", conn);
            MySqlDataReader reader = mySqlCommand.ExecuteReader();
            while (reader.Read())
            {
                if (reader["Username"].Equals(info.Username))
                {
                    string password = Encryption.Decrypt(reader["Password"].ToString(), _env.EncryptionKey, _env.IVKey);
                    if (!password.Equals(info.Password))
                    {

                        ModelState.AddModelError("PasswordError", "Password failed!");
                    }
                    else
                    {
                        HttpContext.Session.SetString("SessionUsername", info.Username);
                        var result = _db.Admin_Accounts.Where(_ => _.Username == info.Username && _.Password == Encryption.Encrpyt(info.Password, _env.EncryptionKey, _env.IVKey)).Select(_ => _.AccountID).FirstOrDefault();
                        HttpContext.Session.SetString("SessionID", Convert.ToString(result));
                        HttpContext.Session.SetString("UserType", "Admin");
                        _db.logsLists.Add(new LogsList
                        {
                            LogName = "Log In",
                            LogDescription = "Logged In Username:" + HttpContext.Session.GetString("SessionUsername"),
                            LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                        });
                        _db.SaveChanges();
                        return RedirectToAction("Dashboard", "MainMenu");
                    }
                }
            }
            ModelState.AddModelError("UsernameError", "Username Not Found");
            return View();
        }


        public IActionResult LoginForm()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginForm(User info)
        {
            var pass = info.Password;
            _account.Username = info.Username;
            _account.AccountID = _db.User_Accounts.Where(x => x.Username == info.Username).Select(x => x.AccountID).FirstOrDefault();
            MySqlCommand mySqlCommand = new MySqlCommand("Select Username,Password From user_accounts", conn);
            conn.Open();
            bool isLoggedIn = false;
            MySqlDataReader reader = mySqlCommand.ExecuteReader();
            while (reader.Read())
            {
                if (reader["Username"].Equals(info.Username))
                {
                    string password = Encryption.Decrypt(reader["Password"].ToString(), _env.EncryptionKey, _env.IVKey);
                    if (!password.Equals(info.Password))
                    {

                        ModelState.AddModelError("PasswordError", "Password failed!");
                    }
                    else {
                        HttpContext.Session.SetString("SessionUsername", info.Username);
                        Console.WriteLine("Hello");
                        isLoggedIn = true;
                    }

                }

            }
            if (isLoggedIn == true) {
                try
                {
                    var SearchData = _db.Homeowner_Details.FirstOrDefault(x => x.Username == info.Username);
                    if (SearchData == null)
                    {
                        Console.WriteLine("Logged In" + _account.Username);
                        return RedirectToAction("Sign_Up2");
                    }
                    else
                    {
                        HttpContext.Session.SetString("SessionUsername", _account.Username);
                        HttpContext.Session.SetString("SessionID", Convert.ToString(SearchData.AccountID));
                        HttpContext.Session.SetString("UserType", "User");//JsonConvert.SerializeObject
                        Console.WriteLine("Logged In");
                        return RedirectToAction("Dashboard", "UserMenu");
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

            }

            ModelState.AddModelError("UsernameError", "Username Not Found");
            return View();
        }

        //Guard Login
        public IActionResult GuardLoginForm()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardLoginForm(User info)
        {
            var pass = info.Password;
            _account.Username = info.Username;
            _account.AccountID = _db.Guard_Information.Where(x => x.Username == info.Username).Select(x => x.ID).FirstOrDefault();
            MySqlCommand mySqlCommand = new MySqlCommand("Select Username,Password From guard_information", conn);
            conn.Open();
            bool isLoggedIn = false;
            MySqlDataReader reader = mySqlCommand.ExecuteReader();
            while (reader.Read())
            {
                if (reader["Username"].Equals(info.Username))
                {
                    string password = Encryption.Decrypt(reader["Password"].ToString(), _env.EncryptionKey, _env.IVKey);
                    if (!password.Equals(info.Password))
                    {

                        ModelState.AddModelError("PasswordError", "Password failed!");
                    }
                    else
                    {
                        HttpContext.Session.SetString("SessionID", _account.AccountID.ToString());
                        HttpContext.Session.SetString("SessionUsername", info.Username);
                        isLoggedIn = true;
                    }

                }

            }
            if (isLoggedIn == true)
            {
                try
                {

                    HttpContext.Session.SetString("UserType", "Guard");//JsonConvert.SerializeObject
                    return RedirectToAction("EntryandExitLogs", "GuardMenu");
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

            }

            ModelState.AddModelError("UsernameError", "Username Not Found");
            return View();
        }
        public IActionResult Sign_Up2 (){ return View(); }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sign_Up2(Homeowner_details info)
        {
            if (ModelState.IsValid)
            {
                _db.Homeowner_Details.Add(new Homeowner_details
                {
                    AccountID = _account.AccountID,
                    Username = _account.Username.ToString(),
                    Surname = info.Surname,
                    Firstname = info.Firstname,
                    Middlename = info.Middlename,
                    Birthdate = info.Birthdate,
                    PhoneNo = info.PhoneNo,
                });
                _db.SaveChanges();
                var data1 = _db.feesLists.ToList();
                foreach (var x in data1)
                {
                    var data2 = new UserFeesStatus
                    {
                        AccountID = _account.AccountID,
                        FeesName = x.FeesName,
                        IDFees = x.IDFees,
                        TypeOfFees = x.TypeOfFees,
                        Amount = x.Amount,
                        Status = x.Status
                    };
                    _db.userFeesStatuses.Add(data2);
                }
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpGet]
        public JsonResult GetSessionName() {
            return Json(HttpContext.Session.GetString("SessionUsername"));
        }
        public IActionResult UserForgotPassword() {
            return View();
        }
        public IActionResult AdminForgotPassword()
        {
            return View();
        }
        public IActionResult GuardForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UserForgotPassword(User_Account info){
            var result = _db.User_Accounts.Where(_ => _.Email == info.Email).FirstOrDefault();
            if (result != null)
            {
                var mailMessage = new MimeMessage();
                var Username = result.Username;
                var Password = Encryption.Decrypt(result.Password, _env.EncryptionKey, _env.IVKey);
                var bodybuild = new BodyBuilder();
                bodybuild.HtmlBody = MainMenuController.CreateMailBody(Username, Password);
                mailMessage.From.Add(new MailboxAddress("Cessna", _env.Email));
                mailMessage.To.Add(new MailboxAddress(result.Username, result.Email));
                mailMessage.Subject = "User Forgot Username and Password";
                mailMessage.Body = bodybuild.ToMessageBody();
                using (var smtpclient = new SmtpClient())
                {
                    smtpclient.Connect(_env.Host, Convert.ToInt32(_env.Port), SecureSocketOptions.StartTls);
                    smtpclient.Authenticate(_env.Email, _env.Password);
                    smtpclient.Send(mailMessage);
                    smtpclient.Disconnect(true);
                }
                ModelState.AddModelError("EmailError", "Email Sent!");
            }
            else {
                ModelState.AddModelError("EmailError", "Email Not Found");
            }
            
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminForgotPassword(Admin_Account info)
        {
            var result = _db.Admin_Accounts.Where(_ => _.Email == info.Email).FirstOrDefault();
            if (result != null)
            {
                var mailMessage = new MimeMessage();
                var Username = result.Username;
                var Password = Encryption.Decrypt(result.Password, _env.EncryptionKey, _env.IVKey);
                var bodybuild = new BodyBuilder();
                bodybuild.HtmlBody = MainMenuController.CreateMailBody(Username, Password);
                mailMessage.From.Add(new MailboxAddress("Cessna", _env.Email));
                mailMessage.To.Add(new MailboxAddress(result.Username, result.Email));
                mailMessage.Subject = "User Forgot Username and Password";
                mailMessage.Body = bodybuild.ToMessageBody();
                using (var smtpclient = new SmtpClient())
                {
                    smtpclient.Connect(_env.Host, Convert.ToInt32(_env.Port), SecureSocketOptions.StartTls);
                    smtpclient.Authenticate(_env.Email, _env.Password);
                    smtpclient.Send(mailMessage);
                    smtpclient.Disconnect(true);
                }
                ModelState.AddModelError("EmailError", "Email Sent!");
            }
            else
            {
                ModelState.AddModelError("EmailError", "Email Not Found");
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardForgotPassword(Guard_Information info)
        {
            var result = _db.Guard_Information.Where(_ => _.Email == info.Email).FirstOrDefault();
            if (result != null)
            {
                var mailMessage = new MimeMessage();
                var Username = result.Username;
                var Password = Encryption.Decrypt(result.Password, _env.EncryptionKey, _env.IVKey);
                var bodybuild = new BodyBuilder();
                bodybuild.HtmlBody = MainMenuController.CreateMailBody(Username, Password);
                mailMessage.From.Add(new MailboxAddress("Cessna", _env.Email));
                mailMessage.To.Add(new MailboxAddress(result.Username, result.Email));
                mailMessage.Subject = "User Forgot Username and Password";
                mailMessage.Body = bodybuild.ToMessageBody();
                using (var smtpclient = new SmtpClient())
                {
                    smtpclient.Connect(_env.Host, Convert.ToInt32(_env.Port), SecureSocketOptions.StartTls);
                    smtpclient.Authenticate(_env.Email, _env.Password);
                    smtpclient.Send(mailMessage);
                    smtpclient.Disconnect(true);
                }
                ModelState.AddModelError("EmailError", "Email Sent!");
            }
            else
            {
                ModelState.AddModelError("EmailError", "Email Not Found");
            }

            return View();
        }


        public IActionResult ResetPassword() {
            return View();
        }
        public IActionResult ChangePassword() {
            return View(); 
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePassword info) {

            if (ModelState.IsValid) { 
            var Username = HttpContext.Session.GetString("SessionUsername");
            var SearchData = _db.User_Accounts.FirstOrDefault(x => x.Username == Username);
            if (SearchData != null) {
                SearchData.Password = info.ConfirmPassword;
                _db.SaveChangesAsync();
            }
            return View();
        }
            
            return View();
        }
        [HttpPost]
        public JsonResult CheckUsername(string userdata) {
            System.Threading.Thread.Sleep(200);
            var SearchData = _db.User_Accounts.Where(x => x.Username == userdata).FirstOrDefault();
            if (SearchData != null)
            {
                return Json(1);
            }
            else { 
                return Json(0);
            }
        }
        [HttpPost]
        public JsonResult CheckGuard(string userdata)
        {
            System.Threading.Thread.Sleep(200);
            var SearchData = _db.Guard_Information.Where(x => x.Username == userdata).SingleOrDefault();
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
        public JsonResult CheckAdmin(string userdata)
        {
            System.Threading.Thread.Sleep(200);
            var SearchData = _db.Admin_Accounts.Where(x => x.Username == userdata).FirstOrDefault();
            if (SearchData != null)
            {
                return Json(1);
            }
            else
            {
                return Json(0);
            }
        }
    }
}
