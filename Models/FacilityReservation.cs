using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class FacilityReservation
    {
        [Key]
        public int ReservationID { get; set; }
        public int FacilityID { get; set; }
        public int AccountID { get; set; }
        [Required]
        public DateOnly DateOFReservation { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [Required]
        public DateTime StartTime { get; set; } = DateTime.Now;
        [Required]
        public DateTime EndTime { get; set; } = DateTime.Now;
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
