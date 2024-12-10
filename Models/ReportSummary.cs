namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class ReportSummary
    {
        public string? ReportName{ get; set; }
        public decimal Amount { get; set; }
        public List<NonIncome>? nonIncomes { get; set; }
    }
    public class NonIncome {
        public string EventName { get; set; } = string.Empty;
        public string EventDescription { get; set; } = string.Empty;
    }
    public class ReportInfo {
        public DateOnly startDate { get; set; }
        public DateOnly endDate { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PreparedBy { get; set; } = string.Empty;

    }
}
