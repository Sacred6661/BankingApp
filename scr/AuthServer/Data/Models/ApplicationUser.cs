using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public bool IsProfileComplete { get; set; }
}