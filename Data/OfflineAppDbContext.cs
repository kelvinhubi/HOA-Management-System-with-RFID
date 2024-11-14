using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.EntityFrameworkCore;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data
{
    public class OfflineAppDbContext : DbContext
    {
        public OfflineAppDbContext(DbContextOptions<OfflineAppDbContext> options) : base(options)
        {

        }
        public DbSet<User_Account> User_Accounts { get; set; }
        public DbSet<Admin_Account> Admin_Accounts { get; set; }
        public DbSet<Homeowner_details> Homeowner_Details { get; set; }
        public DbSet<Guard_Information> Guard_Information { get; set; }
        public DbSet<Vehicle_Information> Vehicle_Information { get; set; }
        public DbSet<Dues> Due_Details { get; set; }
        public DbSet<FeesList> feesLists { get; set; }
        public DbSet<UserFeesStatus> userFeesStatuses { get; set; }
        public DbSet<LogsList> logsLists { get; set; }
        public DbSet<Announcements> Announcements { get; set; }
        public DbSet<HomesList> homesLists { get; set; }
        public DbSet<AccessLog> accessLogs { get; set; }
    }
}
