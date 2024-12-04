using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class MaintenanceRequests
    {
        [Key]
        public int RequestID { get; set; }
        public int AccountID { get; set; }
        public DateOnly RequestDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [Required]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Enter only Letter")]
        public string RequestType { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        [Required]
        public string RequestDexcription {  get; set; } = string.Empty;
        public DateTime CompletionDate {  get; set; }
    }
}
