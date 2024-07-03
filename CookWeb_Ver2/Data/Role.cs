using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookWeb_Ver2.Data
{
    [Table("Roles")]
    public class Role : IdentityRole
    {
        [Required]
        public string? Namee { get; set; }

        [Required]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<User>? Users { get; set; }
        public ICollection<RolePermissions>? RolePermissions { get; set; }
    }
}
