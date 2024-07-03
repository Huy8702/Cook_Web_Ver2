using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookWeb_Ver2.Data
{
    [Table("RolesSystem")]
    public class RolesSystem
    {
        [Key]
        public string? AccountRole { get; set; }
        public bool IsAdmin { get; set; }

    }
}
