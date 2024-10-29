using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class LogsList
    {
        [Key]
        public int LogID { get; set; }
        public string LogName { get; set; } = string.Empty;
        public string LogDescription { get; set; } = string.Empty;
        public string LogUserRole { get; set; } = string.Empty;
        public DateTime LogDate { get; set; } = DateTime.Now;
    }
}
