using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Quartz;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Infrastructure
{
    [DisallowConcurrentExecution]
    public class DependencyInjection :IJob
    {
        private readonly AppDbContext _db;
        public DependencyInjection(AppDbContext db)
        {
            _db = db;
        }

        public Task Execute(IJobExecutionContext context) {
            try
            {
                var result = _db.Homeowner_Details.ToList();
                foreach (var user in result)
                {
                    var dues = _db.Due_Details.Where(_ => _.AccountID == user.AccountID && _.Status.Equals("Unpaid") && DateOnly.FromDateTime(DateTime.Now) >= _.Date).Count();
                    if (dues == 0)
                    {
                        var update = _db.Vehicle_Information.Where(_ => _.AccountID == user.AccountID).ToList();
                        foreach (var car in update)
                        {
                            car.RFID_status = "Enabled";
                            _db.Vehicle_Information.Update(car);
                            _db.SaveChanges();
                        }
                    }
                    else
                    {
                        var update = _db.Vehicle_Information.Where(_ => _.AccountID == user.AccountID).ToList();
                        foreach (var car in update)
                        {
                            car.RFID_status = "Disabled";
                            _db.Vehicle_Information.Update(car);
                            _db.SaveChanges();
                        }
                    }
                }

                
            }
            catch (Exception) { }
            return Task.CompletedTask;
        }
    }
}
