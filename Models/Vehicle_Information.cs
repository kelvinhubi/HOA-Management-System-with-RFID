using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Vehicle_Information
    {
        [Key]
        public int VehicleID {  get; set; }
        [Required(ErrorMessage = "Please Enter your plate"),MaxLength(7)]
		[RegularExpression(@"^[A-Z]{3}\d{4}|[A-Z]{1}\d{3}[A-Z]{2}|[A-Z]{3}\d{3}|[A-Z]{3}\d{4}|\d{3}[A-Z]{3}$", ErrorMessage = "Invalid Plate Number")]
		public string PlateNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int AccountID { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Enter only Letter and Numbers")]
        public string VehicleModel { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Enter only Letter and Numbers")]
        public string VehicleType { get; set; } = string.Empty;
        public string? RFID_number { get; set; } = string.Empty;

        public string RFID_status { get; set; } = string.Empty;

    }
}
