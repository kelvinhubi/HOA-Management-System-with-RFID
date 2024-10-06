using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger _logger;
        MySqlConnection conn = new MySqlConnection(connstr);
        public LoginController(AppDbContext db, ILogger<LoginController> logger)
        {
            _db = db;
            _logger = logger;
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
                    if (!reader["Password"].Equals(info.Password))
                    {
                        
                        ModelState.AddModelError("PasswordError", "Password failed!");
                    }
                    else
                    {
                        HttpContext.Session.SetString("SessionUsername", info.Username);
                        HttpContext.Session.SetString("UserType", "Admin");
                        //JsonConvert.SerializeObject
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
                    if (!reader["Password"].Equals(info.Password))
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
                        Console.WriteLine("Logged In"+ _account.Username);
                        return RedirectToAction("Sign_Up2");
                    }
                    else
                    {
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




        public IActionResult Sign_Up() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sign_Up(User_Account info)//Create User
        {
            if (ModelState.IsValid)
            {
                _db.User_Accounts.Add(info);
                await _db.SaveChangesAsync();
                return RedirectToAction("Sign_Up2");
            }
            return View();
        }
        public IActionResult Sign_Up2(string Username) { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sign_Up2(Homeowner_details info)
        {
            if (ModelState.IsValid)
            {
              
                    Console.WriteLine("This works" + _account.Username);
                    _db.Homeowner_Details.Add(new Homeowner_details
                    {
                        AccountID = _account.AccountID,
                        Username = _account.Username.ToString(),
                        Surname = info.Surname,
                        Firstname = info.Firstname,
                        Middlename = info.Middlename,
                        Birthdate = info.Birthdate,
                        PhoneNo = info.PhoneNo,
                        BlkNO = info.BlkNO,
                        LotNo = info.LotNo,
                        RFID_number = info.RFID_number,
                    });
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
            return View();
            }



        public IActionResult ForgotPassword(){
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
            var SearchData = _db.User_Accounts.Where(x => x.Username == userdata).SingleOrDefault();
            if (SearchData != null)
            {
                return Json(1);
            }
            else { 
                return Json(0);
            }
        }
    }
}
