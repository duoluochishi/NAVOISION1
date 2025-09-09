using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationResult = System.Windows.Controls.ValidationResult;

namespace NV.CT.ImageViewer.Extensions
{
    public class IntegerValidationRule : ValidationRule
    {
        public string ErrorMessage { get; set; }
        public event Action<bool> ValidationTriggered;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult validationResult= int.TryParse(value?.ToString(), out _)
             ? ValidationResult.ValidResult
             : new ValidationResult(false, ErrorMessage);
            ValidationTriggered?.Invoke(validationResult.IsValid ? true : false);
            return validationResult;    
        }
    }

}
