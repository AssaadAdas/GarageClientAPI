using Microsoft.AspNetCore.SignalR;

namespace GarageClientAPI.Data
{
    public class NotificationHub : Hub
    {
        public async Task SendNotificationToUser(string ClientID, string message)
        {
            await Clients.Client(ClientID).SendAsync("ReceiveNotification", message);
        }
    }
}
