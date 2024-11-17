
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Arduino_Serivce;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Hubs;
using Microsoft.AspNetCore.SignalR;
using ClosedXML.Excel;
using System.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.InfoSec;
using Microsoft.Extensions.Options;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.RegularExpressions;
public class GuardMenuController : Controller
{
    private readonly AppDbContext _db;
    private readonly ArduinoLog _arduinoLog;
    private readonly IDbContextFactory<AppDbContext> _asyncdb;
    private readonly IDbContextFactory<OfflineAppDbContext> _asyncdb2;
    private readonly IHubContext<MyHub> _hub;
    private readonly EnvironmentModel _env;
    public int SessionID;
    public GuardMenuController(AppDbContext context, ArduinoLog service,IDbContextFactory<AppDbContext> db2, IDbContextFactory<OfflineAppDbContext> db3, IHubContext<MyHub> hubContext, IOptions<EnvironmentModel> env)
    {
         _db = context;
        _arduinoLog = service;
        _hub = hubContext;
        _asyncdb = db2;
        _asyncdb2 = db3;
        _env = env.Value;
    }
    public async Task<JsonResult> EntryLogging(string rfid)
    {
            
            string data = Regex.Replace(rfid, @"\r\n", ""); ;
            if (data != string.Empty)
            {
                bool online = await ConnectivityChecker.IsOnline();
                if (online)
                {
                    using (var context = _asyncdb.CreateDbContext())
                    {
                        var result = context.Vehicle_Information.Where(_ => _.RFID_number == data && _.RFID_status.Equals("Enabled")).FirstOrDefault();
                        var result2 = context.accessLogs.OrderByDescending(p => p.Time).Where(_ => _.RFID_number == data).FirstOrDefault();
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
                                    await _hub.Clients.All.SendAsync("GetGuardLogSuccess", "RFID Scanned Successfully");
                                }
                                if (result2.LogType == "Entry")
                                {
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
                                        await _hub.Clients.All.SendAsync("GetGuardLogSuccess", "RFID Scanned Successfully");
                                    }
                                }

                            }
                            else
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
                                await _hub.Clients.All.SendAsync("GetGuardLogSuccess", "RFID Scanned Successfully");
                            }
                        }
                        else if (result == null)
                        {
                            await _hub.Clients.All.SendAsync("GetGuardLogFail", "Scan Fail! RFID not Registered");
                        }
                        else
                        {
                            await _hub.Clients.All.SendAsync("GetGuardLogFail", "Scan Fail! Due to Homeowner have pending payments");
                        }
                    }
                }
                else {
                    using (var context = _asyncdb2.CreateDbContext())
                    {
                        var result = context.Vehicle_Information.Where(_ => _.RFID_number == data && _.RFID_status.Equals("Enabled")).FirstOrDefault();
                        var result2 = context.accessLogs.OrderByDescending(p => p.Time).Where(_ => _.RFID_number == data).FirstOrDefault();
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
                                    await _hub.Clients.All.SendAsync("GetGuardLogSuccess", "RFID Scanned Successfully");
                                }
                                if (result2.LogType == "Entry")
                                {
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
                                        await _hub.Clients.All.SendAsync("GetGuardLogSuccess", "RFID Scanned Successfully");
                                    }
                                }

                            }
                            else
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
                                await _hub.Clients.All.SendAsync("GetGuardLogSuccess", "RFID Scanned Successfully");
                            }
                        }
                        else if (result == null) {
                            await _hub.Clients.All.SendAsync("GetGuardLogFail", "Scan Fail! RFID not Registered");
                        }
                        else
                        {
                            await _hub.Clients.All.SendAsync("GetGuardLogFail", "Scan Fail! Due to Homeowner have pending payments");
                        }
                    }
                }
            }
        return Json(0);
    }
    public IActionResult EntryandExitLogs() {
        SessionID = Convert.ToInt32(HttpContext.Session.GetString("SessionID"));
        return View();
    }
    public async Task<IActionResult> PrintLogs(string? nameformat)
    {
        bool online = await ConnectivityChecker.IsOnline();
        if (online) {
            if (nameformat == null)
            {
                nameformat = "ExportPdf";
            }
            string filepath = "wwwroot\\ExportedFiles\\" + nameformat + "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_EntryExitLogs.pdf";
            QuestPDF.Settings.License = LicenseType.Community;
            void ComposeTable(IContainer container)
            {
                container.Border(1).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(100);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Guard ID");
                        header.Cell().Text("Time");
                        header.Cell().Text("Log Type");
                        header.Cell().Text("RFID_Number");
                        header.Cell().Text("Plate No.");
                        header.Cell().Text("Owner Name");
                    });
                    var result = (from access in _db.accessLogs
                                  join
                                    vehicle in _db.Vehicle_Information on access.RFID_number equals vehicle.RFID_number
                                  select new
                                  {
                                      LogID = access.AccessLogID,
                                      GuardID = access.GuardID,
                                      Time = access.Time.ToString("MMMM dd, yyyy h:mm tt"),
                                      LogType = access.LogType,
                                      RFID_number = vehicle.RFID_number,
                                      PlateNo = vehicle.PlateNo,
                                      FullName = vehicle.FullName,
                                  }).ToList();
                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            table.Cell().Text(item.GuardID.ToString());
                            table.Cell().Text(item.Time.ToString());
                            table.Cell().Text(item.LogType.ToString());
                            table.Cell().Text(item.RFID_number.ToString());
                            table.Cell().Text(item.PlateNo.ToString());
                            table.Cell().Text(item.FullName.ToString());
                        }
                    }
                });
            }
            Document.Create(Print =>
            {
                Print.Page(page =>
                {
                    page.Content()
                    .Column(c => ComposeTable(c.Item()));
                    page.Size(PageSizes.A4);
                    page.Header()
                    .Text("Logs")
                    .SemiBold()
                    .FontSize(30);

                });
            }).GeneratePdf(filepath); //RENAMING USING RANDOM WORDS

            return File(System.IO.File.ReadAllBytes(filepath), "application/pdf", nameformat + "_" + Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_EntryExitLogs.pdf");
        }
        await _hub.Clients.All.SendAsync("GetGuardLogFail", "Unable to print while in offine");
        return NoContent();
    }
    public async Task<IActionResult> _ClearLogs()
    {
        bool online = await ConnectivityChecker.IsOnline();
        if (online) {
            //PrintLogs("Backup_");
            var result = _db.accessLogs.ToList();
            _db.accessLogs.RemoveRange(result);
            _db.SaveChanges();
            _db.logsLists.Add(new LogsList
            {
                LogName = "EntryandExit Logs Cleared",
                LogDescription = "Logged In Username: " + HttpContext.Session.GetString("SessionUsername"),
                LogUserRole = "" + HttpContext.Session.GetString("UserType"),
            });
            return RedirectToAction("EntryandExitLogs");
        }
        await _hub.Clients.All.SendAsync("GetGuardLogFail", "Unable to Clear Logs while in Offline");
        return NoContent();
    }
    [HttpPost]
    public async Task<IActionResult> ExportExcel()
    {
        bool online = await ConnectivityChecker.IsOnline();
        if (online) {
            DataTable dt = new DataTable("Student");
            dt.Columns.AddRange(new DataColumn[6] {
                                            new DataColumn("Guard ID"),
                                            new DataColumn("Time"),
                                            new DataColumn("Log Type"),
                                            new DataColumn("RFID_Number"),
                                            new DataColumn("Plate No."),
                                            new DataColumn("Owner Name")
        });

            var Logs = (from access in _db.accessLogs
                        join
                          vehicle in _db.Vehicle_Information on access.RFID_number equals vehicle.RFID_number
                        select new
                        {
                            LogID = access.AccessLogID,
                            GuardID = access.GuardID,
                            Time = access.Time.ToString("MMMM dd, yyyy h:mm tt"),
                            LogType = access.LogType,
                            RFID_number = vehicle.RFID_number,
                            PlateNo = vehicle.PlateNo,
                            FullName = vehicle.FullName,
                        }).ToList();

            foreach (var loglists in Logs)
            {
                dt.Rows.Add(loglists.GuardID, loglists.Time, loglists.LogType, loglists.RFID_number, loglists.PlateNo, loglists.FullName);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("yyyy-MMM-dd") + "_Logs.xlsx");
                }
            }
            
        }
        await _hub.Clients.All.SendAsync("GetGuardLogFail", "Unable to Export while Offline");
        return NoContent();
    }
    [HttpGet]
    public JsonResult GetEntryLogs() {
        var result = (from access in _db.accessLogs join
                      vehicle in _db.Vehicle_Information on access.RFID_number equals vehicle.RFID_number
                      select new { 
                            LogID = access.AccessLogID,
                            GuardID = access.GuardID,
                            Time = access.Time.ToString("MMMM dd, yyyy h:mm tt"),
                            LogType = access.LogType,
                            RFID_number = vehicle.RFID_number,
                            PlateNo = vehicle.PlateNo,
                            FullName = vehicle.FullName,
                      }).ToList();
        return Json(result);
    }

    [HttpGet]
    public JsonResult getCount() {
        var result =  _db.accessLogs.Count();
        return Json(result);
    }
	public IActionResult _ChangeUserPass()
	{
		var result = _db.Guard_Information.FirstOrDefault(_ => _.ID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
		return View(result);
	}
	public IActionResult _ChangeDetails()
	{
		var result = _db.Guard_Information.FirstOrDefault(_ => _.ID == Convert.ToInt32(HttpContext.Session.GetString("SessionID")));
		return View(result);
	}
	[HttpPost]
	public JsonResult UpdateDetails(Guard_Information info)
	{
		if (ModelState.IsValid)
		{
			var result = _db.Guard_Information.Where(_ => _.ID == info.ID).ToList().FirstOrDefault();
			if (result != null)
			{
				_db.Guard_Information.Update(info);
				_db.SaveChanges();
				return Json(new { success = true });
			}
		}
		return Json(new { success = false });
	}
	[HttpPost]
	public JsonResult UpdateProfile(int AccountID, string Password, string Username)
	{
		if (ModelState.IsValid)
		{
			var result = _db.Guard_Information.Where(_ => _.ID == AccountID).ToList().FirstOrDefault();
			if ( result != null)
			{
				try
				{
					if (Encryption.Decrypt(Password, _env.EncryptionKey, _env.IVKey).ToString().Equals(Encryption.Decrypt(result.Password, _env.EncryptionKey, _env.IVKey).ToString()))

					{
						result.Password = Encryption.Encrpyt(Encryption.Decrypt(Password, _env.EncryptionKey, _env.IVKey), _env.EncryptionKey, _env.IVKey);
					}
				}
				catch (Exception)
				{
					result.Password = Encryption.Encrpyt(Password, _env.EncryptionKey, _env.IVKey);

				}
				result.Username = Username;
				_db.Guard_Information.Update(result);
				_db.SaveChanges();
				return Json(new { success = true });
			}

		}
		return Json(new { success = false });
	}
}
