using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace notification_service.Hubs
{
    [Authorize]
    public class NotifHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            Console.WriteLine($"User {userId} (Role: {role}) connected to SignalR NotifHub.");

            // Add admins to a broadcast group so we can push to all admins at once
            if (role == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
                Console.WriteLine($"Admin {userId} added to 'admins' SignalR group.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (role == "Admin")
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
            }

            Console.WriteLine($"User {userId} disconnected from SignalR NotifHub.");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
