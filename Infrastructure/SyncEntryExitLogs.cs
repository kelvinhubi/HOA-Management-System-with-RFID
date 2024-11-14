using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Quartz;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Infrastructure
{
    public class SyncEntryExitLogs :IJob
    {
        private readonly IDbContextFactory<AppDbContext> _db;//online
        private readonly IDbContextFactory<OfflineAppDbContext> _db1;//offline
        public SyncEntryExitLogs(IDbContextFactory<AppDbContext> db, IDbContextFactory<OfflineAppDbContext> db1) { 
            _db = db;
            _db1 = db1;
        }

        public async Task Execute(IJobExecutionContext context) {
            bool online = await ConnectivityChecker.IsOnline();
            if (online) {//if online get first the online updates for all information then upload the latest offline logs then get it again
                using (var db = this._db.CreateDbContext()) {
                    var vehicle_Information = db.Vehicle_Information.ToList();
                    GetUpdateVehicle(vehicle_Information);
                }

                using (var _db1 = this._db1.CreateDbContext()) {
                    //Update to Online Database
                    var accessLogs = _db1.accessLogs.ToList();
                    UpdateAccess(accessLogs);
                    /*
                    var vehicle_Information = _db1.Vehicle_Information.ToList();
                    UpdateVehicle(vehicle_Information);
                    var guard_Information = _db1.Guard_Information.ToList();
                    UpdateGuard(guard_Information);
                    var Useraccounts = _db1.User_Accounts.ToList();
                    UpdateUser(Useraccounts);
                    var admin_Accounts = _db1.Admin_Accounts.ToList();
                    UpdateAdmin(admin_Accounts);
                    var homeowner_Details = _db1.Homeowner_Details.ToList();
                    UpdateHomeowner(homeowner_Details);
                    var dues = _db1.Due_Details.ToList();
                    UpdateDues(dues);
                    var fees = _db1.feesLists.ToList();
                    UpdateFees(fees);
                    var userFees = _db1.userFeesStatuses.ToList();
                    UpdateUserFees(userFees);
                    var loglists = _db1.logsLists.ToList();
                    UpdateLogs(loglists);
                    var announcements = _db1.Announcements.ToList();
                    UpdateAnnouncements(announcements);
                    var homesLists = _db1.homesLists.ToList();
                    UpdateHomes(homesLists);*/
                }
                using (var db = this._db.CreateDbContext())
                {
                    var accessLogs = db.accessLogs.ToList();
                    GetUpdateAccess(accessLogs);
                }
                await Task.CompletedTask;
            }
            return;
        }
        public void GetUpdateVehicle(List<Vehicle_Information> info) {//info online record
            foreach (var account in info)
            {
                using (var _db = this._db1.CreateDbContext())//Online to Offline
                {
                    var offlinerecord = _db.Vehicle_Information.Where(_ => _.VehicleID == account.VehicleID).AsNoTracking().ToList().FirstOrDefault();
                    if (offlinerecord != null) 
                    {
                        _db.Vehicle_Information.Update(account);//Put updated information to offline database
                        _db.SaveChanges();
                    }
                    else
                    {
                        _db.Vehicle_Information.Add(account);//Put added information to offline database
                        _db.SaveChanges();
                    }
                }
            }
        }
        public void GetUpdateAccess(List<AccessLog> info) {//info online record
            foreach (var account in info)
            {
                using (var _db = this._db1.CreateDbContext())//Online to Offline
                {
                    var offlinerecord = _db.accessLogs.Where(_ => _.Time == account.Time).AsNoTracking().ToList().FirstOrDefault();
                    if (offlinerecord != null)
                    {
                        _db.accessLogs.Update(account);//Put updated information to offline database
                        _db.SaveChanges();
                    }
                    else
                    {
                        _db.accessLogs.Add(account);//Put added information to offline database
                        _db.SaveChanges();
                    }
                }
            }
        }
        public void UpdateAccess(List<AccessLog> info)// info offline record
        {
            foreach (var account in info)
            {
                using (var _db = this._db.CreateDbContext())
                {
                    var onlinerecord = _db.accessLogs.Where(_ => _.Time == account.Time).AsNoTracking().ToList().FirstOrDefault();
                    if (onlinerecord != null)
                    {
                        _db.accessLogs.Update(account);//Put updated information to online database
                        _db.SaveChanges();
                    }
                    else
                    {
                        _db.accessLogs.Add(account);//Put added information to online database
                        _db.SaveChanges();
                    }
                }
            }
        }
        /*
         * public void UpdateUser(List<User_Account> info) {
            foreach (var account in info) {
                using (var _db = this._db.CreateDbContext()) {
                    var onlinerecord = _db.User_Accounts.Where(_ => _.AccountID == account.AccountID).AsNoTracking().ToList().FirstOrDefault();
                    if (onlinerecord != null)
                    {
                        _db.User_Accounts.Update(account);
                        _db.SaveChanges();
                    }
                    else
                    {
                        _db.User_Accounts.Add(account);
                        _db.SaveChanges();
                    }
                }
            }
        }
        public  void UpdateAdmin(List<Admin_Account> info)
        {
            foreach (var account in info)
            {
                using (var _db = this._db.CreateDbContext())
                {
                    var onlinerecord = _db.Admin_Accounts.Where(_ => _.AccountID == account.AccountID).AsNoTracking().ToList().FirstOrDefault();
                    if (onlinerecord != null)
                    {
                        _db.Admin_Accounts.Update(account);
                        _db.SaveChanges();
                    }
                    else
                    {
                        _db.Admin_Accounts.Add(account);
                        _db.SaveChanges();
                    }
                }
            }
        }
        public  void UpdateHomeowner(List<Homeowner_details> info)
        {
            foreach (var account in info)
            {
                using (var _db = this._db.CreateDbContext())
                {
                    var onlinerecord = _db.Homeowner_Details.Where(_ => _.AccountID == account.AccountID).AsNoTracking().ToList().FirstOrDefault();
                    if (onlinerecord != null)
                    {
                        _db.Homeowner_Details.Update(account);
                        _db.SaveChanges();
                    }
                    else
                    {
                        _db.Homeowner_Details.Add(account);
                        _db.SaveChanges();
                    }
                }
            }
        }
        public  void UpdateGuard(List<Guard_Information> info)
        {
            foreach (var account in info)
            {
                using (var _db = this._db.CreateDbContext())
                {
                    var onlinerecord = _db.Guard_Information.Where(_ => _.ID == account.ID).AsNoTracking().ToList().FirstOrDefault();
                    if (onlinerecord != null)
                    {
                        _db.Guard_Information.Update(account);
                        _db.SaveChanges();
                    }
                    else
                    {
                        _db.Guard_Information.Add(account);
                        _db.SaveChanges();
                    }
                }
            }
        }
        public  void UpdateVehicle(List<Vehicle_Information> info)
        {
            foreach (var account in info)
            {
                using (var _db = this._db.CreateDbContext())
                {
                    var onlinerecord = _db.Vehicle_Information.Where(_ => _.VehicleID == account.VehicleID).AsNoTracking().ToList().FirstOrDefault();
                    if (onlinerecord != null)
                    {
                        _db.Vehicle_Information.Update(account);
                        _db.SaveChanges();
                    }
                    else
                    {
                        _db.Vehicle_Information.Add(account);
                        _db.SaveChanges();
                    }
                }
            }
        }
public void UpdateDues(List<Dues> info)
{
    foreach (var account in info)
    {
        using (var _db = this._db.CreateDbContext())
        {
            var onlinerecord = _db.Due_Details.Where(_ => _.DueID == account.DueID).AsNoTracking().ToList().FirstOrDefault();
            if (onlinerecord != null)
            {
                _db.Due_Details.Update(account);
                _db.SaveChanges();
            }
            else
            {
                _db.Due_Details.Add(account);
                _db.SaveChanges();
            }
        }
    }
}


public  void UpdateFees(List<FeesList> info)
{
    foreach (var account in info)
    {
        using (var _db = this._db.CreateDbContext())
        {
            var onlinerecord = _db.feesLists.Where(_ => _.IDFees == account.IDFees).AsNoTracking().ToList().FirstOrDefault();
            if (onlinerecord != null)
            {
                _db.feesLists.Update(account);
                _db.SaveChanges();
            }
            else
            {
                _db.feesLists.Add(account);
                _db.SaveChanges();
            }
        }
    }
}
public  void UpdateUserFees(List<UserFeesStatus> info)
{
    foreach (var account in info)
    {
        using (var _db = this._db.CreateDbContext())
        {
            var onlinerecord = _db.userFeesStatuses.Where(_ => _.UserFeeID == account.UserFeeID).AsNoTracking().ToList().FirstOrDefault();
            if (onlinerecord != null)
            {
                _db.userFeesStatuses.Update(account);
                _db.SaveChanges();
            }
            else
            {
                _db.userFeesStatuses.Add(account);
                _db.SaveChanges();
            }
        }
    }
}
public  void UpdateLogs(List<LogsList> info)
{
    foreach (var account in info)
    {
        using (var _db = this._db.CreateDbContext())
        {
            var onlinerecord = _db.logsLists.Where(_ => _.LogID == account.LogID).AsNoTracking().ToList().FirstOrDefault();
            if (onlinerecord != null)
            {
                _db.logsLists.Update(account);
                _db.SaveChanges();
            }
            else
            {
                _db.logsLists.Add(account);
                _db.SaveChanges();
            }
        }
    }
}
public  void UpdateAnnouncements(List<Announcements> info)
{
    foreach (var account in info)
    {
        using (var _db = this._db.CreateDbContext())
        {
            var onlinerecord = _db.Announcements.Where(_ => _.AnnouncementID == account.AnnouncementID).AsNoTracking().ToList().FirstOrDefault();
            if (onlinerecord != null)
            {
                _db.Announcements.Update(account);
                _db.SaveChanges();
            }
            else
            {
                _db.Announcements.Add(account);
                _db.SaveChanges();
            }
        }
    }
}
public  void UpdateHomes(List<HomesList> info)
{
    foreach (var account in info)
    {
        using (var _db = this._db.CreateDbContext())
        {
            var onlinerecord = _db.homesLists.Where(_ => _.HomeID == account.HomeID).AsNoTracking().ToList().FirstOrDefault();
            if (onlinerecord != null)
            {
                _db.homesLists.Update(account);
                _db.SaveChanges();
            }
            else
            {
                _db.homesLists.Add(account);
                _db.SaveChanges();
            }
        }
    }
}




*/



    }

}
