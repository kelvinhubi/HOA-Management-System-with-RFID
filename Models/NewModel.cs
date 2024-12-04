using Microsoft.AspNetCore.Mvc;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models
{
    public class NewModel
    {
        public IEnumerable<Dues> ListDues { get; set; } = Enumerable.Empty<Dues>();
        public IEnumerable<Homeowner_details> Homeacc { get; set; } = Enumerable.Empty<Homeowner_details>();
        public Homeowner_details Homeowner { get; set; }
        public IEnumerable<User_Account> useracc { get; set; } = Enumerable.Empty<User_Account>();
        public List<CheckBoxItem> CheckMe { get; set; }
        public CheckBoxItem CheckM { get; set; }
        public IEnumerable<UserFeesStatus> Userfeestatuses { get; set; } = Enumerable.Empty<UserFeesStatus>();
        public UserFeesStatus Userfeestatus { get; set; }
        public IEnumerable<FeesList> feesLists { get; set; } = Enumerable.Empty<FeesList>();
        public FeesList feesList { get; set; }
        public Dues dues { get; set; }
        public Vehicle_Information Vehicle { get; set; }
        public IEnumerable<Vehicle_Information> Vehicles { get; set; } = Enumerable.Empty<Vehicle_Information>();
        public string sortOrder { get; set; }
        public IEnumerable<Announcements> Announcements { get; set; }
        public Announcements Announcement { get; set; }
        public IEnumerable<HomesList> Homes { get; set; } = Enumerable.Empty<HomesList>();
        public HomesList Home { get; set; }
        public IEnumerable<AccessLog> accessLogs { get; set; } = Enumerable.Empty<AccessLog>();
        public AccessLog AccessLog { get; set; }
        public IEnumerable<Admin_Account> admin_Accounts { get; set; } = Enumerable.Empty<Admin_Account>();
        public Admin_Account AdminAccount { get; set; }
        public IEnumerable<Guard_Information> guards { get; set; }
        public Guard_Information Guard { get; set; }
        public IEnumerable<HomeRequest> HomeRequests {get; set;} = Enumerable.Empty<HomeRequest>();
        public IEnumerable<MaintenanceRequests> maintenances { get; set; } = Enumerable.Empty<MaintenanceRequests>();
        public MaintenanceRequests maintenance { get; set; }
    }
}
