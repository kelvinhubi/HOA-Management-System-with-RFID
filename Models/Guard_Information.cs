using System.ComponentModel.DataAnnotations;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Guard_Information
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public String? Username { get; set; }

        [Required]
        public String? Password { get; set; }

        public String? Name { get; set; }
    }
}
