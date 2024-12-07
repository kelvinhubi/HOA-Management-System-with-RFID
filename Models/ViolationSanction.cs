using System.ComponentModel.DataAnnotations;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class ViolationSanction
    {
        [Key]
        public int ViolationID { get; set; }
        public int AccountID { get; set; }
        public string Complainant { get; set; } = string.Empty;
        public string Respondent { get; set; } = string.Empty;
        public DateTime IncidentReportDate { get; set; } = DateTime.Now;
        public string Witness { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Violation { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;

        public string? Sanction { get; set; }
        public string? Remarks { get; set; }
    }
}
