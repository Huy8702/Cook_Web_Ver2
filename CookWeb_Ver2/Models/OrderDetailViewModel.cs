using CookWeb_Ver2.Data;

namespace CookWeb_Ver2.Models
{
    public class OrderDetailViewModel
    {
        public int RecipeId { get; set; }
        public int ErrorCode { get; set; }
        public string? TypeFood { get; set; }
        public bool IsChecked { get; set; }
        public int Quantity { get; set; }
    }
}
