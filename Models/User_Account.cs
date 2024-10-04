using System.ComponentModel.DataAnnotations;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class User_Account
    {
        [Key]
        public int AccountID { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

    }
}
