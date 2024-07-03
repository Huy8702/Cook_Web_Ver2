using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CookWeb_Ver2.Data
{
    [Table("ErrorList")]
    public class ErrorList
    {
        [Key]
        public int ErrorCode { get; set; }
        public int ErrorIndex { get; set; }
        public string? ErrorInfo { get; set; }
    }
}
