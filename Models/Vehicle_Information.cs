using System.ComponentModel.DataAnnotations;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Vehicle_Information
    {
        [Key]
        public int VehicleID {  get; set; }
        public String PlateNo { get; set; } = "";

        public int AccountID { get; set; }
        [Required]
        public String VehicleModel { get; set; } = string.Empty;
        [Required]
        public String VehicleType { get; set; } = string.Empty;
        [Required]
        public String RFID_number { get; set; } = string.Empty;

    }
}
