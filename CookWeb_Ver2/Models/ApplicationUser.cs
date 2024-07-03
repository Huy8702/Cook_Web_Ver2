using Microsoft.AspNetCore.Identity;

namespace CookWeb_Ver2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
