// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Windows.Controls;

namespace WifiRanger
{
    public class AreaRange : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var area = 0;

            try
            {
                if (((string)value).Length > 0)
                    area = int.Parse((string)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Please enter a numeric value");
            }

            if ((area < Min) || (area > Max))
            {
                return new ValidationResult(false,
                    "Please enter an area in the range: " + Min + " - " + Max + ".");
            }
            return new ValidationResult(true, null);
        }
    }
}
