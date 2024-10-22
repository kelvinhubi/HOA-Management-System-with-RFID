using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Microsoft.AspNetCore.Mvc;
using System.IO.Ports;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class RfidController : Controller
    {
        private readonly AppDbContext _db;

        public RfidController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public JsonResult ReadRFID()
        {

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                Console.WriteLine(port);
            }
            return Json(0);
        }

        public IActionResult ShowRFID()
        {

            return View();
        }
        public IActionResult DeleteRFID()
        {
            /*
              if(ID == null){
                return NotFound();
                }
              var result = _db.rfid_Information.SingleOrDefault();
              if(result == null){
                return NotFound();
                }
             */
            return View();//(result);
        }
        [HttpPost]
        public IActionResult _DeleteRFID(RfidController info)
        {
            /*
             * if(ModelState.IsValid){ 
             * _db.rfid_Information.Remove(info); 
             * _db.SaveChanges(); 
             * }
             */
            return RedirectToAction();//MainMenu;
        }
        public IActionResult CreateRFID()
        {

            return PartialView();
        }

        [HttpPost]
        public IActionResult _CreateRFID(RfidController info)
        {
            /*
                if(ModelState.IsValid){
                _db.rfid_Information.Add(info);
                _db.SaveChanges();
            }
             */
            return RedirectToAction();
        }

        public IActionResult UpdateRFID(int ID)
        {
            /*
              if(ID == null){
                return NotFound();
                }
              var result = _db.rfid_Information.SingleOrDefault();
              if(result == null){
                return NotFound();
                }
             */
            return PartialView();
        }
        [HttpPost]
        public IActionResult _UpdateRFID(RfidController info)
        {
            /*
                if(ModelState.IsValid){
                _db.rfid_Information.Update(info);
                _db.SaveChanges();
            }
             */
            return RedirectToAction();
        }
    }
}
