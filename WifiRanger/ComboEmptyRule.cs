using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WifiRanger
{
    class ComboEmptyRule: ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
          if(value == null)
            {
                return new ValidationResult(false, "Please enter a floor");
            }
            else
            {
                return new ValidationResult(true, null);
            }

        }
    }
}
