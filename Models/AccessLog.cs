using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class AccessLog
    {
        [Key]
        public int AccessLogID { get; set; }
        public int GuardID { get; set; }
        public int AccountID { get; set; }
        [Required]
        public string RFID_number { get; set; } = string.Empty;
        public string Time { get; set; } = DateTime.Now.ToString("MMMM dd, yyyy h:mm tt");
        public string LogType { get; set; } = string.Empty;
    }
}
