using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class ChangePassword
    {
        [NotMapped]
        [Required(ErrorMessage = "New Password is required")]
        public string NewPassword { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "Confirmation Password is required")]
        [Compare("NewPassword", ErrorMessage = "Password not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
