using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.SignalR;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Hubs
{
    public class MyHub : Hub
    {
        public async Task JoinRF(string user,string conn) {
              await Clients.All.SendAsync("RecieveMessage",user, conn,DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"));
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
