using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Dues
    {
        [Key]
        public int payID { get; set; }
        public int AccountID { get; set; }
        [Required(ErrorMessage = "Please not more than 8 digits"), MaxLength(8)]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Error pls only enter numbers")]
        public string Amount { get; set; } = "";

        public string TypeofDues { get; set; } = string.Empty;
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public string Status { get; set; } = "Unpaid";
        [NotMapped]
        public string FullName { get; set; } = string.Empty;
    }
}
