
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Arduino_Serivce;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

public class GuardMenuController : Controller
{
    private readonly AppDbContext _db;
    private readonly ArduinoLog _arduinoLog;
    private readonly IDbContextFactory<AppDbContext> _asyncdb;
    public int SessionID;
    public GuardMenuController(AppDbContext context, ArduinoLog service,IDbContextFactory<AppDbContext> db2)
    {
         _db = context;
        _arduinoLog = service;
        _asyncdb = db2;
        
        Task.Run(EntryLogging);
    }
    public async Task EntryLogging()
    {
        await _arduinoLog.InitializeAsync();
        
        while (true)
        {
            string data = await _arduinoLog.ReadLineAsync();

            if (data != string.Empty )
            {
                using (var context = _asyncdb.CreateDbContext())
                {
                    var result = context.Vehicle_Information.Where(_ => _.RFID_number == data).ToList().SingleOrDefault();
                    var result2 = context.accessLogs.Where(_ => _.RFID_number == data).OrderByDescending(p => p.Time).ToList().FirstOrDefault();
                    if (result != null)
                    {
                        if (result2 != null)
                        {
                            if (result2.LogType == "Exit")
                            {
                                var info = new AccessLog
                                {
                                    GuardID = SessionID,
                                    AccountID = result.AccountID,
                                    RFID_number = result.RFID_number,
                                    LogType = "Entry",
                                };
                                context.accessLogs.Add(info);
                                await context.SaveChangesAsync();
                            }
                            else
                            {
                                var info = new AccessLog
                                {
                                    GuardID = SessionID,
                                    AccountID = result2.AccountID,
                                    RFID_number = result.RFID_number,
                                    LogType = "Exit",
                                };
                                context.accessLogs.Add(info);
                                await context.SaveChangesAsync();
                            }

                        }
                        else {
                            var info = new AccessLog
                            {
                                GuardID = SessionID,
                                AccountID = result.AccountID,
                                RFID_number = result.RFID_number,
                                LogType = "Entry",
                            };
                            context.accessLogs.Add(info);
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
           await Task.Delay(1000);
        }
    }
    public IActionResult Dashboard() {
        return View();
    }
    public IActionResult EntryandExitLogs() {
        /*SessionID = Convert.ToInt32(HttpContext.Session.GetString("SessionID"));
        NewModel model = new NewModel();
        model.accessLogs = _db.accessLogs.ToList();
        model.Vehicles = _db.Vehicle_Information.ToList();*/
        return View();
    }
    [HttpGet]
    public JsonResult GetEntryLogs() {
        var result = (from access in _db.accessLogs join
                      vehicle in _db.Vehicle_Information on access.RFID_number equals vehicle.RFID_number
                      select new { 
                            LogID = access.AccessLogID,
                            GuardID = access.GuardID,
                            Time = access.Time,
                            LogType = access.LogType,
                            RFID_number = vehicle.RFID_number,
                            PlateNo = vehicle.PlateNo,
                            FullName = vehicle.FullName,
                      }).OrderByDescending(_=>_.Time).ToList();
        return Json(result);
    }
    [HttpGet]
    public JsonResult getCount() {
        var result = _db.accessLogs.Count();
        return Json(result);
    }
}
