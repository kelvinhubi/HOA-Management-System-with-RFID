using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.SignalR;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Hubs
{
    public class MyHub : Hub
    {
        private readonly AppDbContext _db;
        public MyHub(AppDbContext db) {
            _db = db;
        }
        public async Task JoinRF(string user,string conn) {
              await Clients.All.SendAsync("RecieveMessage",user, conn,DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"));
            _db.chatHistory.Add(new ChatHistory { Message = conn, UserName = user, Date = DateTime.Now});
            await _db.SaveChangesAsync();
        }

        public async Task ChatRoom(string conn) {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn);
            await Clients.Group(conn).SendAsync("RecieveMessage", "Admin", $"{conn} has joined");
        }
        public async Task Notification(string message) { 
            await Clients.All.SendAsync("RecieveNotification", message);
        }
    }
}
