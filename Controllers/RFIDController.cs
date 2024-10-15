using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc;
using System.IO.Ports;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class RfidController : Controller
    {
        [HttpPost]
        public JsonResult RfidPopMe(string userdata) {
            ;
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports) {
                Console.WriteLine(port);
            }
            return Json(1);
        }
    }
}
