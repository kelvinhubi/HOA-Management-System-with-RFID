using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Vehicle_Information
    {
        [Key]
        public int VehicleID {  get; set; }
        [Required(ErrorMessage = "Please Enter your plate"),MaxLength(8)]
        public string PlateNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int AccountID { get; set; }
        [Required]
        public string VehicleModel { get; set; } = string.Empty;
        [Required]
        public string VehicleType { get; set; } = string.Empty;
        public string RFID_number { get; set; } = "N/A";
        public string RFID_status { get; set; } = "Disabled";

    }
}
