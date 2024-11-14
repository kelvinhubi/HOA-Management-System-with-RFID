using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Microsoft.EntityFrameworkCore;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Hubs;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers;
using System.IO.Ports;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Arduino_Serivce;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Infrastructure;
using Quartz;
RfidController._port = new SerialPort();
RfidController._port.PortName = "COM4";
RfidController._port.BaudRate = 115200;
bool online = await ConnectivityChecker.IsOnline();
var builder = WebApplication.CreateBuilder(args);
var connectionString1 = builder.Configuration.GetConnectionString("OnlineAppDbConnectionString");
var connectionString2 = builder.Configuration.GetConnectionString("OfflineAppDbConnectionString");
//throw new InvalidOperationException("Connection string 'AppDbConnectionString' not found.");
var serverVersion1 = new MySqlServerVersion(ServerVersion.AutoDetect(connectionString1));
var serverVersion2 = new MySqlServerVersion(ServerVersion.AutoDetect(connectionString2));
var EnvModel = builder.Configuration.GetSection("Env");
builder.Services.Configure<EnvironmentModel>(EnvModel);
//For Entity Framework
builder.Services.AddSignalR();
//builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString, serverVersion));
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseMySql(connectionString1, serverVersion1));
builder.Services.AddDbContextFactory<OfflineAppDbContext>(options => options.UseMySql(connectionString2, serverVersion2));
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddScoped<ArduinoLog>(provider => { return new ArduinoLog("COM4", 115200); });
//For Background Task
builder.Services.AddQuartz(options => {
    var jobkey = JobKey.Create("CheckDuesJob");
    var jobkey2 = JobKey.Create("OnlineChecker");
    options.AddJob<DependencyInjection>(jobkey)
    .AddTrigger(Trigger=> Trigger.ForJob(jobkey).WithSimpleSchedule(s=>s.WithIntervalInHours(5).RepeatForever()));
options.AddJob<SyncEntryExitLogs>(jobkey2).
AddTrigger(Trigger => Trigger.ForJob(jobkey2).WithSimpleSchedule(s => s.WithIntervalInSeconds(60).RepeatForever()));
});
builder.Services.AddQuartzHostedService(options => {
    options.WaitForJobsToComplete = true;
});
//For Identity
//Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
});

//sESSION
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(1800);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else {
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapHub<MyHub>("/chat");
app.UseAuthorization();
app.UseAuthentication();
app.MapRazorPages();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();