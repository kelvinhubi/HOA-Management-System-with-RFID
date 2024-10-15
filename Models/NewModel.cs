namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class NewModel
    {
        public IEnumerable<Dues> ListDues { get; set; } = Enumerable.Empty<Dues>();
        public IEnumerable<Homeowner_details> Homeacc { get; set; } = Enumerable.Empty<Homeowner_details>();
        public Homeowner_details Homeowner { get; set; }
        public IEnumerable<User_Account> useracc { get; set; } = Enumerable.Empty<User_Account>();
        public IEnumerable<CheckBoxItem> CheckBoxItems { get; set; } = Enumerable.Empty<CheckBoxItem>();
        public CheckBoxItem CheckBoxItem { get; set; }
        public IEnumerable<UserFeesStatus> Userfeestatuses { get; set; } = Enumerable.Empty<UserFeesStatus>();
        public UserFeesStatus Userfeestatus { get; set; }
        public Dues dues { get; set; }
        public string sortOrder { get; set; }
    }
}
