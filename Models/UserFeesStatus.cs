using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class UserFeesStatus
    {
        [Key]
        public int UserFeeID { get; set; }
        public int IDFees {  get; set; }
        public int AccountID { get; set; }
        public string TypeOfFees { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please not more than 8 digits"), MaxLength(8)]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Error pls only enter numbers")]
        public string Amount { get; set; } = string.Empty;
        public string Status { get; set; } = "Disabled";
        [NotMapped]
        public List<CheckBoxItem> EnabledItems { get; set; }
    }
}
