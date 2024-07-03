using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace CookWeb_Ver2.Data
{
    [Table("OrderDetail")]
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MachineId { get; set; }
        public int RecipesID { get; set; }
        public string? Code { get; set; }
        public int OrdinalNumber { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public int ErrorCode { get; set; }
        public string? TypeFood { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime FinishTime { get; set; } = DateTime.Now;
        public Order? Order { get; set; } 
        public Recipes? Recipes { get; set; } 
        public Machine? Machine { get; set; }

    }
}
