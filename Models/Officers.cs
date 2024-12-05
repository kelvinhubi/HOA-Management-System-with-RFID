using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Officers
    {
        [Key]
        public int ID { get; set; }
        public int AccountID { get; set; }
        public string Position { get; set; } = string.Empty;
    }
}
