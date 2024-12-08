namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class Yearly
    {
        public int Year { get; set; }
        public decimal Amount {  get; set; }
    }
    public class Monthly {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
    }
    public class Weekly
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Week { get; set; }
        public decimal Amount { get; set; }
    }
    public class Daily { 
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }
    }
    public class FacilityUsageReport {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public double TotalUsageHours { get; set; }
        public double AverageDailyUsage { get; set; }
        public DateTime PeakUsageDate { get; set; }
        public double PeakUsageHours { get; set; }
    }
    public class DailyFacilityUsageReport
    {
        public DateTime Date { get; set; }
        public List<FacilityUsage>? FacilityUsages { get; set; }
    }
    public class WeeklyFacilityUsageReport
    {
        public int WeekNumber { get; set; }
        public List<FacilityUsage>? FacilityUsages { get; set; }
    }
    public class MonthlyFacilityUsageReport
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<FacilityUsage>? FacilityUsages { get; set; }
    }
    public class FacilityUsage
    {
        public string FacilityName { get; set; } = string.Empty;
        public double TotalUsageHours { get; set; }
    }
}
