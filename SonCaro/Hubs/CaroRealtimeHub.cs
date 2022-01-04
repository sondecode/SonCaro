using Microsoft.AspNetCore.SignalR;
using SonCaro.Models;

namespace SonCaro.Hubs
{
    public class CaroRealtimeHub : Hub
    {
        public async Task UserOnline(List<User> users) => await Clients.All.SendAsync("user-online", users);
    }
}