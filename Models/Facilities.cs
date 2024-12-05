using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Facilities
    {
        [Key]
        public int FacilityID { get; set; }
        public string FacilityName { get; set; }= string.Empty;
        public string RentalFee { get; set; } = string.Empty;
    }

}
