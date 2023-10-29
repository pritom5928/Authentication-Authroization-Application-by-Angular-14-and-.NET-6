using AngularAuthAPI.Context;
using AngularAuthAPI.Helpers;
using AngularAuthAPI.Models;
using AngularAuthAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(5);
            await _db.SaveChangesAsync();

            return Ok(new TokenApiDto
            {
                AccessToken = user.Token,
                RefreshToken = user.RefreshToken
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
                new Claim(ClaimTypes.Name, $"{user.UserName}")
                });

                var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = identity,
                    Expires = DateTime.Now.AddSeconds(10),
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

        #region refresh token
        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _db.Users.Any(a => a.RefreshToken == refreshToken);

            if (tokenInUser)
            {
                return CreateRefreshToken();
            }

            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.UTF8.GetBytes("veryverysecret....");

            var tokenValidationParams = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;

            var principal = tokenHandler.ValidateToken(token, tokenValidationParams, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if(jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("This is invalid token");

            return principal;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(TokenApiDto tokenApiDto)
        {
            if (tokenApiDto == null)
                return BadRequest("Invalid client request");

            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;

            var principal = GetPrincipleFromExpiredToken(accessToken);
            var userName = principal.Identity.Name;

            var user = await _db.Users.FirstOrDefaultAsync(f=>f.UserName == userName);

            if (user == null || (user != null && user.RefreshToken != refreshToken) || (user != null && user.RefreshTokenExpiryTime <= DateTime.Now))
                return BadRequest("Invalid request");

            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();

            user.Token = newAccessToken;
            user.RefreshToken = newRefreshToken;

            await _db.SaveChangesAsync();

            return Ok(new TokenApiDto
            {
                 AccessToken = user.Token,
                 RefreshToken = user.RefreshToken,
            });
        }
        #endregion

        [HttpGet]
        [Authorize]
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
