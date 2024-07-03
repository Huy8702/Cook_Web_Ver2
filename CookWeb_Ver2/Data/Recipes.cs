using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookWeb_Ver2.Data
{
    [Table("Recipes")]
    public class Recipes
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int CookingTime { get; set; }
        [Required]
        public int NumberTrayBox { get; set; }
        public int TypeCookId { get; set; }
        public string? ImagePath { get; set; }
        public string? Description { get; set; }
		public bool IsInvisible { get; set; }
        public int TypeDishId { get; set; }
		public ICollection<RecipesDetail>? RecipesDetails { get; set; }
        public ICollection<StepsMakeRecipes>? StepsMakeRecipes { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
