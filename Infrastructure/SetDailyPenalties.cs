using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Quartz;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Infrastructure
{
    [DisallowConcurrentExecution]
    public class SetDailyPenalties:IJob
    {
        private readonly AppDbContext _db;
        public SetDailyPenalties(AppDbContext db) { 
            _db = db;
        }
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                var result = _db.Homeowner_Details.ToList();
                foreach (var user in result)
                {

                    if (user.Role.Equals("Homeowner"))
                    {
                        var homeownerhome = _db.homesLists.FirstOrDefault(_ => _.AccountID == user.AccountID);
                        var dues = _db.Due_Details.Where(_ => _.AccountID == user.AccountID && _.Status.Equals("Unpaid") && DateOnly.FromDateTime(DateTime.Now) > _.Date).Count();
                        if (dues == 0)
                        {
                          //If no dues no penalties
                        }
                        else
                        {
                            var update = _db.Due_Details.Where(_ => _.AccountID == user.AccountID && _.Status.Equals("Unpaid") && DateOnly.FromDateTime(DateTime.Now) > _.Date).ToList();
                            foreach (var items in update)
                            {
                                var penalty = _db.userFeesStatuses.FirstOrDefault(_ => _.AccountID == items.AccountID);
                                int userpenalty = Convert.ToInt32(items.Amount) + Convert.ToInt32(items.Penalty);
                                items.Amount = userpenalty.ToString();
                                _db.Due_Details.Update(items);
                                _db.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception) { }
            return Task.CompletedTask;
        }
    }
}
