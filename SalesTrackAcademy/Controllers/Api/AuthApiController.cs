using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalesTrackAcademy.Models;

namespace SalesTrackAcademy.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : ControllerBase
{
    public record LoginRequest(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password." });

        var result = await signInManager.PasswordSignInAsync(user, req.Password, isPersistent: true, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid email or password." });

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            role = roles.FirstOrDefault() ?? "Agent"
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok();
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { message = "Not authenticated." });

        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            role = roles.FirstOrDefault() ?? "Agent"
        });
    }
}
