using Microsoft.AspNetCore.Mvc;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class RFIDController : Controller
    {
        public IActionResult RFID() { return View(); }
        public IActionResult AddRFID() { return View(); }


        [HttpPost]
        public IActionResult AddRFID() {   return View(); }

        public IActionResult DeleteRFID() { return View(); }



        public IActionResult EditRFID() { return View(); }


    }
}
