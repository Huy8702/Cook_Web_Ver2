using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookWeb_Ver2.Data
{
    [Table("RecipesDetail")]
    public class RecipesDetail
    {
        [Key]
        public int Id { get; set; }
        public int RecipesId { get; set; }
        public int IngredientId { get; set; }
        public int Amounts { get; set; }
        public int TraysNumber { get; set; }
        public Recipes? Recipes { get; set; }
        public Ingredient? Ingredient { get; set; }
    }
}
