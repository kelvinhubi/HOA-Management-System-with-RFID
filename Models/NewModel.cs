namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class NewModel
    {
        public IEnumerable<Dues> ListDues { get; set; } = Enumerable.Empty<Dues>();
        public IEnumerable<Homeowner_details> Homeacc { get; set; } = Enumerable.Empty<Homeowner_details>();
        public Dues dues { get; set; }
        public string sortOrder { get; set; }
    }
}
