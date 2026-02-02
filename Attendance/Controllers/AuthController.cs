using Attendance.Data;
using Attendance.DTOs;
using Attendance.Models;
using Attendance.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Attendance.Controllers
{
    [Microsoft.AspNetCore.Components.Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserAuthRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.UserName))
            { 
                return BadRequest("そのユーザーIDは既に使用されています。");
            }
            String passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.UserName,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "ユーザー登録が完了しました！" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserAuthRequest request)
        { 
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.UserName);

            if (user == null) 
            {
                return BadRequest("ユーザが見つかりません");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("パスワードが間違えています。");
            }

            String token = CreateToken(user);

            return Ok(new { message = token });
        }

        private String CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            // 秘密鍵を取り出す
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("JwtSettings:Key").Value!));

            // 署名を作る
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // トークンを作成
            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1), // 有効期限: 1日
                    signingCredentials: creds
                );

            // 文字列に変換して返す
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
