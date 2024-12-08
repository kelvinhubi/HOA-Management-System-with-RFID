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

}
