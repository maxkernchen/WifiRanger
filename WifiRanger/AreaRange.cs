
using System;
using System.Globalization;
using System.Windows.Controls;

namespace WifiRanger
{
    /// <summary>
    /// Validation rule class which makes sure the area entered is a number and is 
    /// between 1 and 10,000
    /// <author>Max Kernchen</author>
    /// <date>05/05/2018</date>
    /// </summary>
    public class AreaRange : ValidationRule
    {
        //the min and max set and getter methods
        public int Min { get; set; }
        public int Max { get; set; }
        /// <summary>
        /// Overriden validation result class which returns results for invalid inputs
        /// </summary>
        /// <param name="value">the value passed in in this case an integer for the area covered</param>
        /// <param name="cultureInfo">the locale for this code, not used in this case</param>
        /// <returns>a result if validation has passed</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var area = 0;
            // try to convert the value into an int
            try
            {
                if (((string)value).Length > 0)
                    area = int.Parse((string)value);
            }
            //if not an int then return a failed a result for wrong type of numeric value
            catch (Exception e)
            {
                return new ValidationResult(false, "Please enter a numeric value");
            }
            // if the area is not in the correct range return a failed result for the wrong range
            if ((area < Min) || (area > Max))
            {
                return new ValidationResult(false,
                    "Please enter an area in the range: " + Min + " - " + Max + ".");
            }
            return new ValidationResult(true, null);
        }
    }
}
