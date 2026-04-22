using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace notification_service.Hubs
{
    [Authorize]
    public class NotifHub : Hub
    {
        // Clients connect to this hub using their JWT.
        // SignalR automatically uses the ClaimTypes.NameIdentifier to identify the "User".
        // This allows us to send messages to specific users via: Clients.User(userId.ToString()).SendAsync(...)

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            // Optionally log connection
            Console.WriteLine($"User {userId} connected to SignalR NotifHub.");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"User {userId} disconnected from SignalR NotifHub.");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
