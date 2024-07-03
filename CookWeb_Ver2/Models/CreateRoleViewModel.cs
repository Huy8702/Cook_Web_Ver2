using System.ComponentModel.DataAnnotations;

namespace CookWeb_Ver2.Models
{
    public class CreateRoleViewModel
    {
        [Required]
        public string? RoleName { get; set; }
    }
}
