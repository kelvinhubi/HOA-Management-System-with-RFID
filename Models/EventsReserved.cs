using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class EventsReserved
    {
        [Key]
        public int EventReservedID { get; set; }
        public int AccountID { get; set; }
        public int EventID { get; set; }
        public string? Fee { get; set; }
        public string? Status { get; set; } = string.Empty;
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}
