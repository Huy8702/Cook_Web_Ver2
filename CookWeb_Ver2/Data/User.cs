using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace CookWeb_Ver2.Data
{
    [Table("Users")]
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string Address { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string MoreInfo { get; set; } = string.Empty;
        public bool Activate { get; set; } = true;
    }
}
