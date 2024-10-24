using System.ComponentModel.DataAnnotations;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Announcements
    {
        [Key]
        public int AnnouncementID { get; set; }
        [Required]
        public string AnnouncemenType {  get; set; } = string.Empty;
        [RegularExpression(@"([A-Z][a-z][,()][0-9])\w+$", ErrorMessage = "Character input not supported")]
        public string Description { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; } = DateTime.Now;
    }
}
