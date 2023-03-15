using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.DTO.Auth;
using UrlShortener.Entities;

namespace UrlShortener.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(DatabaseContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult> PostRegister(RegisterDto payload)
    {
        var existingUser = await _context.Users.Where(q => q.Email == payload.Email).SingleOrDefaultAsync();
        if (existingUser is not null)
        {
            return Problem("Email already in use", null, 400);
        }

        //var existingUser = _context.Users.FindAsync({})
        var pwdHash = BCrypt.Net.BCrypt.HashPassword(payload.Password);

        var newUser = new User
        {
            Id = await Nanoid.Nanoid.GenerateAsync(size: 20),
            Email = payload.Email,
            Password = pwdHash
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        await HandleCookie(newUser);

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<User>> PostLogin(LoginDto payload)
    {
        var existingUser = await _context.Users.SingleOrDefaultAsync(q => q.Email == payload.Email);
        if (existingUser is null)
        {
            return Problem("Invalid credentials", null, 400);
        }

        if (!BCrypt.Net.BCrypt.Verify(payload.Password, existingUser.Password))
        {
            return Problem("Invalid credentials", null, 400);
        }

        await HandleCookie(existingUser);

        return NoContent();
    }

    [HttpGet("profile"), Authorize]
    public async Task<ActionResult> GetProfile()
    {
        var problem = Problem("Unauthorized", null, 400);

        var userPrincipal = this.User;
        var usrClaim = userPrincipal.Claims.FirstOrDefault(c => c.Type == "usr").Value;
        if (string.IsNullOrEmpty(usrClaim))
        {
            return problem;
        }

        var usr = await _context.Users.Where(u => u.Id == usrClaim).FirstOrDefaultAsync();
        return usr is null ? problem : Ok(usr);
    }


    private async Task HandleCookie(User user)
    {
        var claims = new List<Claim>
        {
            new("usr", user.Id)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);
    }
}