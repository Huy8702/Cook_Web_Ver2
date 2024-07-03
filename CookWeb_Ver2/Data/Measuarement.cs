using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookWeb_Ver2.Data
{
    [Table("Measuarement")]
    public class Measuarement
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string DisplayName { get; set; } = string.Empty;
        [Required]
        public string Unit { get; set; } = string.Empty;
        public ICollection<Ingredient>? Ingredients { get; set; }
    }
}
