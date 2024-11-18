using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.EntityFrameworkCore;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data
{
    public class OfflineAppDbContext : DbContext
    {
        public OfflineAppDbContext(DbContextOptions<OfflineAppDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dues>()
                .HasIndex(c => c.Invoice)
                .IsUnique();
            modelBuilder.Entity<User_Account>()
                .HasIndex(c => c.Username)
                .IsUnique();
            modelBuilder.Entity<FeesList>()
                .HasIndex(c => c.FeesName)
                .IsUnique();
            modelBuilder.Entity<Vehicle_Information>()
                .HasIndex(c => c.PlateNo)
                .IsUnique();
            modelBuilder.Entity<Guard_Information>()
                .HasIndex(c => c.Username)
                .IsUnique();
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
