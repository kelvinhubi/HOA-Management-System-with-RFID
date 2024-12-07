using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Facilities
    {
        [Key]
        public int FacilityID { get; set; }
        [Required, MaxLength(20)]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Enter only Letter")]
        public string FacilityName { get; set; }= string.Empty;
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Error pls only enter numbers")]
        public string RentalFee { get; set; } = string.Empty;
    }

}
