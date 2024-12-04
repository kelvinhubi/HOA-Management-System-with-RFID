using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class PetInformation
    {
        [Key]
        public int PetId { get; set; } // Optional for auto-increment in database
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public int AccountID { get; set; }
        public int Age { get; set; }
    }
    public class OwnerPetInfo
    {
        
        public List<PetInformation> Pets { get; set; } = new List<PetInformation>(); // Initialize to avoid null reference
    }
}
