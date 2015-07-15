using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;
using System.Threading;
using System.Globalization;

namespace Validators
{
    public class ComboboxValidator : ValidationRule
    {

        public ComboboxValidator()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Chose value!");
            }

            return new ValidationResult(true, null);

        }
    }
}

