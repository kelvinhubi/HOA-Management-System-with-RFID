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
            if (conn.Contains("/"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, user);
                if (conn == "/FAQ") {
                   
                    await Clients.Group(user).SendAsync("RecieveMessage", "Bot", "FAQs <br>1. How much are the Monthly Fees: The fees are set by the hoa and you can contact or email us at krfortin15@gmail.com for further information.<br>2. What happens if i didnt pay in time:<br> The system will detect if you have any overdue payments and admin will notify you if the said duration is not paid you will be having a violation in the hoa rules", DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"));
                    return;
                }
                if (conn == "/Help")
                {

                    await Clients.Group(user).SendAsync("RecieveMessage", "Bot", "If you are having trouble in using the system pls contact our ADMIN email:krfortin15@gmail.com", DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"));
                    return;
                }
                if (conn == "/") {
                    await Clients.Group(user).SendAsync("RecieveMessage", "Bot", "Commands:<br> 1./FAQ<br>2./Help", DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"));
                    return;
                }
                else
                {
                    await Clients.Group(user).SendAsync("RecieveMessage", "Bot", "I didnt Understand pls type \"/\" for commands", DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"));
                    return;
                }
            }
            else {
                await Clients.All.SendAsync("RecieveMessage", user, conn, DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"));
                _db.chatHistory.Add(new ChatHistory { Message = conn, UserName = user, Date = DateTime.Now });
                await _db.SaveChangesAsync();
            }
              
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
