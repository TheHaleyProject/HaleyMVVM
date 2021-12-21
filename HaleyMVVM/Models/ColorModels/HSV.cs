using System.Text;
using System.Windows.Input;

namespace Haley.Models
{
    /// <summary>
    /// Holder for Hue, Saturation, Value (Brightness)
    /// </summary>
    public class HSV
    {
        public double Hue { get; set; } //To represent 0-360
        public double Saturation { get; set; } //To represent 0 to 1
        public double Value { get; set; }
        /// <summary>
        /// HSV Holder
        /// </summary>
        /// <param name="hue">0-360</param>
        /// <param name="saturation">0-1</param>
        /// <param name="value">0-1</param>
        public HSV(double hue, double saturation, double value)
        {
            Hue = hue;
            Saturation = saturation;
            Value = value;
        }
        public override string ToString()
        {
            return $"{this.Hue}° {this.Saturation}% {this.Value}%";
        }
    }
}