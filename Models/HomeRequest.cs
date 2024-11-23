using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class HomeRequest
    {
        [Key]
        public int HomeReqID { get; set;}
        public int AccountID { get; set;}
        public int HomeID { get; set; }
        public int ResidentID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string BlkNO { get; set; } = string.Empty;
        public string HomeName { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set;} = string.Empty;
    }
}
