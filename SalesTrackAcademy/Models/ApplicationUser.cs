using Microsoft.AspNetCore.Identity;

namespace SalesTrackAcademy.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
