using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Expenses
    {
        [Key]
        public int ExpenseID { get; set; }
        public DateOnly EzpenseDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [Required, MaxLength(20)]
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Enter only Letter")]
        public string PaymentMethod { get; set; }
        [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Enter only Letter")]
        public string Description { get; set; }
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Error pls only enter numbers")]
        public string Amount { get; set; }
        public string Category { get; set; }
    }
}
