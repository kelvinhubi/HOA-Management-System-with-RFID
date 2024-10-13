using Microsoft.AspNetCore.Mvc;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class RFIDController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
