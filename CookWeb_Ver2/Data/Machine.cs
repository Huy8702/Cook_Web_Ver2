using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookWeb_Ver2.Data
{
    [Table("Machines")]
    public class Machine
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int MachineId { get; set; }
        [Required]
        public int MachineTypeId { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 4)]
        public string? Code{ get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? typeTray { get; set; }
        public DateTime ProductDate { get; set; } = DateTime.UtcNow;
        public DateTime WarrantyExpDate { get; set; } = DateTime.UtcNow;
        public DateTime LastMaintainDate { get; set; } = DateTime.UtcNow;
        public string? Location { get; set; }
        public string? Description { get; set; }
        public bool IsCooking { get; set; } = false;
        public bool Activate { get; set; } = true;
        public MachineType? MachineType { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
