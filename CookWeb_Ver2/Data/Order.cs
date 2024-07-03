using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CookWeb_Ver2.Data
{
    [Table("Order")]
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Status { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }

    }
}
