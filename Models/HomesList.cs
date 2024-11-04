using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class HomesList
    {
        [Key]
        public int HomeID { get; set; }
        public int AccountID { get; set; }
        [Required]
        public string HomeName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Error pls only enter numbers")]
        public string BlkNO { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
