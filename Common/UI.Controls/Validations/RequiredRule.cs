using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NV.CT.UI.Controls.Validations
{
    public class RequiredRule : ValidationRule
    {
        public RequiredRule()
        {
            // this.ValidatesOnTargetUpdated = true;
        }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is null)
            {
                IsValid = false;
                return new ValidationResult(false, ErrorMessage);
            }

            if (string.IsNullOrEmpty(value.ToString()))
            {
                IsValid = false;
                return new ValidationResult(false, ErrorMessage);
            }

            IsValid = true;
            return new ValidationResult(true, null);
        }
    }
}
