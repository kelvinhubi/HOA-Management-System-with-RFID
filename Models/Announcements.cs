using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Announcements
    {
        [Key]
        public int AnnouncementID { get; set; }
        [Required]
        public string AnnouncementTitle {  get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; } = DateTime.Now;
        //public string bgFilePath { get; set; } = string.Empty;
        //public IFormFile? backgroundFile { get; set; }
    }
}
