using AngularAuthAPI.Context;
using AngularAuthAPI.Helpers;
using AngularAuthAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AngularAuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UserController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = await _db.Users.FirstOrDefaultAsync(f => f.UserName == userObj.UserName &&f.Password == userObj.Password );

            if (user == null)
                return NotFound(new { Message = "User not found!" });

            return Ok(new { Message = "Login success!" });
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            userObj.Role = "User";
            userObj.Token = "";

            await _db.Users.AddAsync(userObj);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Registration success!" });
        }

    }
}
