using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SonCaro.Hubs;
using SonCaro.Models;
using SonCaro.ViewModels;

namespace SonCaro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IHubContext<CaroRealtimeHub> _hub;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserController(AppDbContext context, IConfiguration config, IHubContext<CaroRealtimeHub> hub)
        {
            _context = context;
            _config = config;
            _hub = hub;
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public object Register(RegisterUser request)
        {

            var userDb = _context.Users.FirstOrDefault(x => x.UserName == request.UserName);

            if (userDb != null)
                return BadRequest("Username này đã được đăng ký!");

            if (request.Password != request.ConfirmPassword)
                return BadRequest("Mật khẩu xác nhận không đúng!");

            var user = new User();
            user.Id = Guid.NewGuid();
            user.IsAdmin = request.IsAdmin;
            user.Name = request.Name;
            user.UserName = request.UserName;

            user.Password = request.Password;
            user.Score = 100;

            _context.Users.Add(user); // lưu vào ram
            _context.SaveChanges();     // lưu vào cơ sở dữ liệu

            return Ok("Đăng ký tài khoản thành công");
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public object Login(LoginUser request)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == request.UserName);

            if (user == null)
                return BadRequest("Tài khoản này chưa được đăng ký!");

            if (user.Password != request.Password)
                return BadRequest("Sai mật khẩu!");


            // ở đây chúng ta rằng là user có tồn tại và đúng mật khẩu

            /// thực hiện xử lý cập nhật trường status thành true.

            user.Status = true;

            _context.Users.Update(user);
            _context.SaveChanges();


            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("userName", user.UserName),
                new Claim("password", user.Password),
                new Claim("isAdmin", user.IsAdmin.ToString()),
                new Claim("name", user.Name),
                new Claim("score", user.Score.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Tokens:Issuer"],
                _config["Tokens:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var users = _context.Users.ToList();

            // gửi danh sách user đi
            _hub.Clients.All.SendAsync("user-online", users);

            return Ok(new JwtSecurityTokenHandler().WriteToken(token));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public object Logout(LoginUser request)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == request.UserName);

            if (user == null)
                return BadRequest("Tài khoản này chưa được đăng ký!");

            if (user.Password != request.Password)
                return BadRequest("Sai mật khẩu!");


            // ở đây chúng ta rằng là user có tồn tại và đúng mật khẩu

            /// thực hiện xử lý cập nhật trường status thành true.

            user.Status = false;

            _context.Users.Update(user);
            _context.SaveChanges();

            var users = _context.Users.ToList();

            // gửi danh sách user đi
            _hub.Clients.All.SendAsync("user-online", users);

            return Ok("Đăng xuất thành công");

        }


        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
