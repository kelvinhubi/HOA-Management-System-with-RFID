using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Events
    {
        [Key]
        public int EventID { get; set; }
        [Required]
        public string EventTitle { get; set; } = string.Empty;
        [Required]
        public string EventDescription { get; set; } = string.Empty;
        [Required]
        public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly EventDateEnd { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [Required]
        public string EventLocation {  get; set; } = string.Empty;
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Error pls only enter numbers")]
        public string? EventFee { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Error pls only enter numbers")]
        public int EventCapacity { get; set; }
        [Required]
        public DateTime EventStartDate { get; set; } = DateTime.Now;
        [Required]
        public DateTime EventEndDate { get; set; }= DateTime.Now;
    }
}
