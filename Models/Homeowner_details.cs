using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Homeowner_details
    {

        [Key]
        public int AccountID { get; set; }
        public string Username { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please enter your Surname"), MaxLength(15)]
        public string Surname { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please enter your Firstname"), MaxLength(20)]
        public string Firstname { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please enter your Middlename"), MaxLength(15)]
        public string Middlename { get; set; } = string.Empty;
        [Required]
        public DateOnly Birthdate { get; set; }

        [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Not a valid phone number")]
        public string PhoneNo { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Error pls only enter numbers") ]
        public string BlkNO { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Error pls only enter numbers")]
        public string LotNo { get; set; } = string.Empty;
        [ValidateNever]
        public string RFID_number { get; set; } = string.Empty;
        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName { get { return Firstname + " " + Middlename + " " + Surname; } }
    }
}
//[RegularExpression(@"^\(?([0-10]{3})\)?[-. ]?([0-10]{3})[-. ]?([0-10]{4})$", ErrorMessage = "Not a valid phone number")]