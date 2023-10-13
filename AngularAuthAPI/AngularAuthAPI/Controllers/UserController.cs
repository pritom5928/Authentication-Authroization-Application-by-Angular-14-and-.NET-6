using AngularAuthAPI.Context;
using AngularAuthAPI.Helpers;
using AngularAuthAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

            var user = await _db.Users.FirstOrDefaultAsync(f => f.UserName == userObj.UserName);

            if (user == null)
                return NotFound(new { Message = "User not found!" });

            if (!PasswordHasher.Verifypassword(userObj.Password, user.Password))
                return NotFound(new { Message = "Incorrect password!" });


            user.Token = CreateJwt(user);

            return Ok(new
            {
                Token = user.Token,
                Message = "Login success!"
            });
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token;
            try
            {

                var key = Encoding.ASCII.GetBytes("veryverysecret....");

                var identity = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
                });

                var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = identity,
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = credentials
                };

                token = jwtTokenHandler.CreateToken(tokenDescriptor);
            }
            catch (Exception ex)
            {
                throw;
            }

            return jwtTokenHandler.WriteToken(token);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _db.Users.ToListAsync());
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();


            //check username
            if (await CheckUserNameExistsAsync(userObj.UserName))
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

            if (password.Length < 8)
                sb.Append("Minimum password length should be 8 " + Environment.NewLine);

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
