using AngularAuthAPI.Context;
using AngularAuthAPI.Helpers;
using AngularAuthAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

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


            //check username
            if(await CheckUserNameExistsAsync(userObj.UserName))
                return BadRequest(new { Message = "Username already exists!" });


            //check email
            if (await CheckEmailExistsAsync(userObj.Email))
                return BadRequest(new { Message = "Email already exists!" });


            //check password strength
            var passwordInvalidationMsg = CheckPasswordStrength(userObj.Password);
            if (!string.IsNullOrWhiteSpace(passwordInvalidationMsg))
                return BadRequest(new { Message = passwordInvalidationMsg });


            userObj.Password = PasswordHasher.HashPassword(userObj.Password);
            userObj.Role = "User";
            userObj.Token = "";

            await _db.Users.AddAsync(userObj);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Registration success!" });
        }

        private async Task<bool> CheckUserNameExistsAsync(string userName)
        {
           return await _db.Users.AnyAsync(u => u.UserName == userName);
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email == email);
        }

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();

            if(password.Length < 8)
                sb.Append("Minimum password length should be 8 "+ Environment.NewLine);

            //if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
            //    sb.Append("Password should be AlphaNumeric" + Environment.NewLine);

            if (!(Regex.IsMatch(password, "[a-z A-Z 0-9]+")))
                sb.Append("Password should be AlphaNumeric" + Environment.NewLine);

            

            if (!(Regex.IsMatch(password, "[!@#$%^&*()_+{}\\[\\]:;<>,.?~|\\\\]")))
                sb.Append("Password should be contain special characters" + Environment.NewLine);


            return sb.ToString();
        }
    }
}
