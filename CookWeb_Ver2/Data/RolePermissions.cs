using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookWeb_Ver2.Data
{
    [Table("RolePermissions")]
    public class RolePermissions
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int RoleId { get; set; }
        [Required]
        public int PermissionId { get; set; }
        public Role? Role { get; set; }
    }
}
