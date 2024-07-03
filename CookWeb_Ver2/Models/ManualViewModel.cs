using System.ComponentModel.DataAnnotations;

namespace CookWeb_Ver2.Models
{
    public class ManualViewModel 
    {
        public int MachineId { get; set; }
        public int MachineTypeId { get; set; }
        public string? MachineName { get; set; }
    }
}
