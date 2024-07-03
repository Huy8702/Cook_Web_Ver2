using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookWeb_Ver2.Data
{
    [Table("StepsMakeRecipes")]
    public class StepsMakeRecipes
    {
        [Key]
        public int Id { get; set; }
        //[Required]
        public string? StepCode { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        [Required]
        public int RecipesId { get; set; }
        [Required]
        public int StepNumber { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string _Param { get; set; } = string.Empty;
        public int _Angle { get; set; }
        public int _Speed { get; set; }
        public int _FireLevel { get; set; }
        public int _Capacity { get; set; }
        public int _Method { get; set; }
        public int _Temp { get; set; }
        public int _Second { get; set; }
        public Recipes? Recipes { get; set; }
    }
}
