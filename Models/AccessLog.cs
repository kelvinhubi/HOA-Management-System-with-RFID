namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class AccessLog
    {
        public int AccessLogID { get; set; }
        public int GuardID { get; set; }
        public string HomeID { get; set; } = string.Empty;
        public DateTime EntryTime { get; set; } = DateTime.Now;
        public DateTime? ExitTime { get; set; }
        public string Comments { get; set; } = string.Empty;
    }
}
