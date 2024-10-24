namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class EnvironmentModel
    {
        public string? Host {  get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Port { get; set; }
        public string EncryptionKey { get; set; } = String.Empty;
        public string IVKey { get; set; } = string.Empty;
	}
}
