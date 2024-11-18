using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class FeesList
    {
        [Key]
        public int IDFees { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Enter only Letter")]
        public string FeesName { get; set; }  = string.Empty;
        [Required]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Enter only Letter")]
        public string TypeOfFees { get; set; } = string.Empty;
        [Required, MaxLength(8)]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Error pls only enter numbers")]
        public string Amount { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = "Disabled";
    }
}
