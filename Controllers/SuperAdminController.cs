using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Hubs;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.InfoSec;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Text;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class SuperAdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly EnvironmentModel _env;
        private readonly IWebHostEnvironment _webenv;
        public SuperAdminController(AppDbContext db, IOptions<EnvironmentModel> Accessor, IWebHostEnvironment environment) {
            _db = db;
            _env = Accessor.Value;
            _webenv = environment;
        }
        public IActionResult Dashboard()
        {
            return View();
        }
        //UserManagement Controller
        public IActionResult UserManagement()
        {

                var model = new NewModel();
                model.admin_Accounts = _db.Admin_Accounts.ToList();

                return View(model);

            //return RedirectToAction("AccessDenied", "MainMenu");
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

            var useracc = _db.Admin_Accounts.FirstOrDefault(m => m.AccountID == ID);
            if (useracc == null)
            {
                return NotFound();
            }

            return PartialView(useracc);
        }
        //Delete all info in the users
        [HttpPost]
        public async Task<IActionResult> UserDelete(Admin_Account obj)
        {
            try
            {
                _db.Admin_Accounts.Remove(obj);
                _db.SaveChanges();
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Delete Admin Account",
                    LogDescription = "Logged In Username:" + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                await _db.SaveChangesAsync();
                return RedirectToAction("UserManagement");
            }
            catch (Exception) { return RedirectToAction("UserManagement"); }

        }


        public async Task<IActionResult> CreatUserAcc(Admin_Account info)
        {
            Console.WriteLine(info.AccountID);
            if (ModelState.IsValid)
            {

                var mailMessage = new MimeMessage();
                var Username = info.Username;
                var Password = info.Password;
                var bodybuild = new BodyBuilder();

                bodybuild.HtmlBody = CreateMailBody(Username, Password);
                mailMessage.From.Add(new MailboxAddress("C.S.H.A", "krfortin15@gmail.com"));
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
                info = new Admin_Account
                {
                    Username = info.Username,
                    Password = Encryption.Encrpyt(info.Password, _env.EncryptionKey, _env.IVKey),
                    AccountID = info.AccountID,
                    Email = info.Email
                };
                _db.Admin_Accounts.Add(info);
                _db.logsLists.Add(new LogsList
                {
                    LogName = "Create User Account",
                    LogDescription = "Logged In Username:" + HttpContext.Session.GetString("SessionUsername"),
                    LogUserRole = "" + HttpContext.Session.GetString("UserType"),
                });
                //This error only happens if created at the same time frame
                var existingUser = await _db.Admin_Accounts.FirstOrDefaultAsync(_ => _.Username == info.Username) != null ? true : false;
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
        [HttpPost]
        public JsonResult CheckUsername(string userdata)
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
        [HttpPost]
        public JsonResult CheckUsernameEmail(string userdata)
        {
            System.Threading.Thread.Sleep(200);
            var SearchData = _db.Admin_Accounts.Where(x => x.Email == userdata).FirstOrDefault();
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
