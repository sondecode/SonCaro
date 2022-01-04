using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SonCaro.Hubs;
using SonCaro.ViewModels;


namespace SonCaro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private IHubContext<CaroRealtimeHub> _hub;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public ChatController(AppDbContext context, IConfiguration config, IHubContext<CaroRealtimeHub> hub)
        {
            _context = context;
            _config = config;
            _hub = hub;
        }
        [HttpPost("message-user")]
        public object MessageUser(MessageUser messageUser)
        {

            var user = _context.Users.FirstOrDefault(x => x.Id == messageUser.UserId);

            if (user == null)
                return BadRequest("Username này không tìm thấy!");

            var room = _context.Rooms.FirstOrDefault(x => x.Id == messageUser.RoomId);

            if (room == null)
                return BadRequest("Phòng này không tìm thấy!");

            _hub.Clients.All.SendAsync("chat-online", messageUser);

            return Ok();
        }
    }
}
