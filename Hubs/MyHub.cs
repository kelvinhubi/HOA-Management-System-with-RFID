using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;
using Microsoft.AspNetCore.SignalR;
namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Hubs
{
    public class MyHub : Hub
    {
        public async Task JoinRF(Homeowner_details conn) {
            await Clients.All.SendAsync("RecieveMessage", "Admin",$"{conn.FullName} has joined");
        }

        public async Task ChatRoom(Homeowner_details conn) {
            await Groups.AddToGroupAsync(Context.ConnectionId, conn.FullName);
            await Clients.Group(conn.FullName).SendAsync("RecieveMessage", "Admin", $"{conn.FullName} has joined");
        }
    }
}
