using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace CookWeb_Ver2.Ultilities
{
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        private readonly string allowedDomain;

        public ValidEmailDomainAttribute(string allowedDomain)
        {
            this.allowedDomain = allowedDomain;
        }

        public override bool IsValid(object? value = null)
        {
            if (value != null)
            {
                string[] strings = value?.ToString()?.Split('@') ?? Array.Empty<string>();
                return strings[1].ToUpper() == allowedDomain.ToUpper();
            }
            else
            {
                return false;
            }
        }
    }
}
