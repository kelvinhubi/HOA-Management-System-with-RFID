using System.ComponentModel.DataAnnotations;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Guard_Information
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please enter your Name"), MaxLength(15)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^(09|\+639)\d{9}$", ErrorMessage = "Not a valid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
