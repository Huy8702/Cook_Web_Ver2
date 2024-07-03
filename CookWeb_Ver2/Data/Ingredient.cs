using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookWeb_Ver2.Data
{
    [Table("Ingredient")]
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public int MeasuarementId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierInfo { get; set; } = string.Empty;
        public Measuarement? Measuarement { get; set; }
        public ICollection<RecipesDetail>? RecipesDetails { get; set; }
    }
}
