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
                if (conn.Contains("/") && conn.ToLower().Contains("faq")) {
                   
                    await Clients.Group(user).SendAsync("RecieveMessage", "Bot", "FAQ<br><br>1. What is the RFID system for vehicles?<br>The RFID (Radio Frequency Identification) system uses electronic tags attached to vehicles for secure, automated access to the community. When a registered vehicle approaches the gate, the RFID reader scans the tag and opens the gate automatically.<br><br>2. Why is the HOA implementing an RFID system?<br>The RFID system enhances security, improves traffic flow, and minimizes manual intervention at the gates. It ensures only authorized vehicles can enter the community.<br><br>Registration and Installation<br><br>3. How do I register my vehicle for the RFID system?<br>You can register your vehicle by filling out the RFID application form online or at the HOA office. Be sure to provide your vehicle details and proof of residency.<br><br>4. How long does it take to process my RFID application?<br>Applications are typically processed within 1-2 business days. You will be notified once your RFID tag is ready for installation.<br><br>Usage and Operation<br><br>n6. What happens if my RFID tag is not recognized?<br>If your tag is not recognized, ensure it is clean and undamaged. If the problem persists, contact the HOA office for troubleshooting or replacement.<br><br>7. Can I transfer my RFID tag to another vehicle?<br>No, RFID tags are linked to a specific vehicle for security reasons. If you change vehicles, you must apply for a new tag.<br><br>8. Can guests use the RFID system?<br>The RFID system is reserved for registered residents. Guests must use the visitor lane and follow standard access procedures.<br><br>Security and Privacy<br><br>9. How secure is the RFID system?<br>The system is highly secure, using encrypted technology to ensure that only authorized vehicles gain access. All data is protected and used solely for community access purposes.<br><br>10. Does the RFID system track my location?<br>No, the RFID system only registers your vehicle’s entry and exit times for security and traffic monitoring within the community.<br><br>Troubleshooting and Replacements<br><br>11. What if I lose or damage my RFID tag?<br>If your tag is lost or damaged, notify the HOA immediately. You may need to pay a replacement fee, depending on community policies.<br><br>12. My RFID tag isn’t working. What should I do?<br>Check if the tag is properly installed and free from obstructions like dirt or tint. If it still doesn’t work, contact the HOA office for assistance.<br><br>Fees and Policies<br><br>13. Is there a fee for the RFID tag?<br>Yes, there is a one-time fee for the RFID tag. Replacement fees may apply for lost or damaged tags. Refer to the HOA’s fee schedule for details.<br><br>14. Are there penalties for misuse of the RFID system?<br>Yes, unauthorized use or tampering with the RFID system may result in penalties, including suspension of access or fines.<br><br>Support<br><br>15. Who do I contact for questions or support?<br>You can contact the HOA office via email, phone, or in person during business hours. Emergency support is available for urgent access issues.", DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"));
                    return;
                }
                if (conn.Contains("/") && conn.ToLower().Contains("help"))
                {

                    await Clients.Group(user).SendAsync("RecieveMessage", "Bot", "If you are having trouble in using the system pls contact our ADMIN email:krfortin15@gmail.com", DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"));
                    return;
                }
                if (conn.Contains("/") && conn.ToLower().Contains("thank"))
                {

                    await Clients.Group(user).SendAsync("RecieveMessage", "Bot", "Your Welcome", DateTime.Now.ToString("MMMM dd, yyyy h:mm tt"));
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
