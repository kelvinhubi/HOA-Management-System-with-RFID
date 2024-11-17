using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO.Ports;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class RfidController : Controller
    {
        private readonly AppDbContext _db;
        public static SerialPort? _port;
        public RfidController(AppDbContext db)
        {
            _db = db;
		}
        
        [HttpGet]
        public JsonResult ReadRFID()
        {
            
            if (_port != null) {
                try {
                    if (_port.IsOpen == true) { _port.Close(); }
                    if (_port.IsOpen == false)
                    {
                        _port.Open();
                        string value = _port.ReadLine();
                        if (value != null)
                        {
                            Console.WriteLine(value);
                            _port.Close();
                            return Json(value);
                        }
                    }
                } catch (Exception ) { 
                
                }
            }
            return Json("0");
		}
        public JsonResult showPorts() {
            string[] ports = SerialPort.GetPortNames();
            return Json(ports.ToList());
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
