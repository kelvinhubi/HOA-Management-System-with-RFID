using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class SuperAdmin
    {
        [Key]
        public int AccountID { get; set; }

        [Required]
        public String? Username { get; set; }

        [Required]
        public String? Password { get; set; }
    }
}
