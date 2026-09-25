using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CareConnect.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}