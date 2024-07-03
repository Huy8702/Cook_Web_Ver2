using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using CookWeb_Ver2.Ultilities;

namespace CookWeb_Ver2.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Remote(action: "IsEmailInUse", controller: "Account")]
        [ValidEmailDomain("gmail.com", ErrorMessage = "Email domain must be gmail.com")]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password",
            ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}
