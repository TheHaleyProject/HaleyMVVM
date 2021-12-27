using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Haley.Enums;
using Haley.Models;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using dwg = System.Drawing;

namespace Haley.Utils
{
    public sealed class ColorUtils
    {
        private static Dictionary<string, Color> _systemColors = new Dictionary<string, Color>();
        public static void ClampLimits(ref int actual,int min = 0, int max = 255)
        {
            if (actual > max) actual = max;
            if (actual < min) actual = min;
        }

        public static double ClampLimits(double input, double min, double max)
        {
            if (input < min)
            {
                return min;
            }
            else if (input > max)
            {
                return max;
            }
            else
            {
                return input;
            }
        }

        public static Dictionary<string,Color> GetSystemColors()
        {
            if (_systemColors == null || _systemColors?.Count < 1 )
            {
                foreach (var color_prop in typeof(Colors).GetProperties())
                {
                    if (color_prop.PropertyType == typeof(Color))
                    {
                        if (!_systemColors.ContainsKey(color_prop.Name))
                        {
                            var _clr = color_prop.GetValue(null);
                            if (_clr != null)
                            {
                                _systemColors.Add(color_prop.Name, (Color)_clr);
                            }
                        }
                    }
                }
            }
            return _systemColors;
        }
        public static string ColorToHex(Color color)
        {
            return RgbToHex(color.R, color.G, color.B);
        }

        public static string RgbToHex(byte r, byte g, byte b)
        {
            string hex = r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
            return hex;
        }

        public static Color HexToColor(string hexvalue)
        {
            var _brush = new BrushConverter().ConvertFromString(hexvalue) as SolidColorBrush;
            if (_brush != null) return _brush.Color;
            return Colors.Black;
        }

        public static Color GetColorAtOffset(GradientStopCollection collection, double offset)
        {

            offset = ClampLimits(offset, 0.0, 1.0); //Offset should be on a range 0-1

            //Now, from the gradient collection, get the relevant color.
            if (collection == null) return Colors.Black;

            var ordered_stops = collection.OrderBy(p => p.Offset).ToList(); //Get the ordered stops.

            if (offset <= 0)
            {
                return ordered_stops[0].Color; //If our percent is zero or less, then get the start color.
            }

            if (offset >= 1)
            {
                return ordered_stops[ordered_stops.Count() - 1].Color; //Get the last color
            }

            //if we are stuck inbetween, then we need to find on which gradient stop does our offset falls.
            GradientStop begin_stop = ordered_stops[0];
            GradientStop end_stop = null;

            foreach (GradientStop current_stop in ordered_stops)
            {
                if (current_stop.Offset >= offset)
                {
                    //Meaning our target falls under this range.
                    end_stop = current_stop;
                    break; //Break the loop and proceed further.
                }
                //If the target offset is bigger than the current stop offset, then current offset becomes the start (like a bubble sort).
                begin_stop = current_stop;
            }

            //At this point, we should have finalized the begin and end stop. Now find the percentage with in these two stops and get the color.
            double local_percent = Math.Round((offset - begin_stop.Offset) / (end_stop.Offset - begin_stop.Offset), 3); //Percentage with in two gradients.

            //Get the relative color values. 
            byte a = (byte)(((end_stop.Color.A - begin_stop.Color.A) * local_percent) + begin_stop.Color.A);
            byte r = (byte)(((end_stop.Color.R - begin_stop.Color.R) * local_percent) + begin_stop.Color.R);
            byte g = (byte)(((end_stop.Color.G - begin_stop.Color.G) * local_percent) + begin_stop.Color.G);
            byte b = (byte)(((end_stop.Color.B - begin_stop.Color.B) * local_percent) + begin_stop.Color.B);
            return Color.FromArgb(a, r, g, b);
        }

        public static HSV ColorToHSV(Color color)
        {
            return RgbToHSV(color.A, color.R, color.G, color.B);
        }

        public static HSV RgbToHSV(byte a, byte r, byte g, byte b)
        {
            //Create a System.Drawing.Color , so that we can easily get the hue.
            var _color = dwg.Color.FromArgb(a, r, g, b);

            //between r,g,b find which is maximum and minimum. (first find between g and b and then compare with r.
            int max = Math.Max(r, Math.Max(g, b));
            int min = Math.Min(r, Math.Min(g, b));

            var hue = _color.GetHue(); //Get directly.

            int delta = max - min; //This is the total saturation 
            double saturation = (max == 0) ? 0.0 : (delta / (max * 1.0)); //if max is 0, then we don't have a chroma. All we have is white color. (because saturation 0 is at center of the circle (which is white). Saturation mapped on to 0-1 scale
            var value = max / 255.0; //Maximum value mapped on 0-1 scale.
            return new HSV(hue, saturation, value);
        }

        /// <summary>
        /// HSV To Color Converter
        /// </summary>
        /// <param name="hue">Range 0-360</param>
        /// <param name="saturation">Range 0-1</param>
        /// <param name="value">Range 0-1</param>
        /// <param name="alpha">Range 0-255</param>
        /// <returns></returns>
        public static Color HsvToColor(double hue, double saturation, double value, double alpha=255)
        {
            //imagine a hexagonal cube with 6 pieces (or sides or slices or whatever). It starts with Red (at 0 degree vertice & 360 degree), Yellow (at 60 degree ),  Lime (120), Cyan(180), Blue(240), Magneta(300) 
            int segment_number = Convert.ToInt32(Math.Floor(hue / 60)) % 6; //Get which segment the hue falls under. The gradient is split into 6 segments

            double delta = hue / 60 - Math.Floor(hue / 60);  //Get the remainder of the value.

            value = value * 255; //Convert value from 0-1 to 255. (0 is black, 1  is white).

            //MAXLEVEL ( simply the Value )
            byte max_level = (byte)Convert.ToInt32(value);
            //Take a look at the segments image. For segment 1 and 6, we have Red as maxvalue. For 2,3=> Green is maxvalue.

            //RANGE ( value * saturation)
            double range = value * saturation; //Just for understanding.

            ///MINLEVEL (Value - Range)
            byte min_level = (byte)Convert.ToInt32(value * (1- saturation)); //which is nothing but value - range. 

            //Max and Min level will give us two color values (two values of R, G, B in different circumstances).
            //However, for the third value, we need the movement in horizontal direction. So, we get the difference to locate it. 
            //If we look above in the segmentnumber calculation, we merely take the Mod value to identify the target segment.
            //By doing so, we ignore the decimal values (which returns the actual location of the third value).

            
            byte theta_decrease = (byte)Convert.ToInt32(value * (1 - delta * saturation)); //decrease
            byte theta_increase = (byte)Convert.ToInt32(value * (1 - (1 - delta) * saturation)); //increase (1-delta). we calculate from the top of the sloping line.

            
            switch(segment_number)
            {
                case 0:
                    return Color.FromArgb((byte)alpha, max_level, theta_increase, min_level);
                case 1:
                    return Color.FromArgb((byte)alpha, theta_decrease, max_level, min_level);
                case 2:
                    return Color.FromArgb((byte)alpha, min_level, max_level, theta_increase);
                case 3:
                    return Color.FromArgb((byte)alpha, min_level, theta_decrease, max_level);
                case 4:
                    return Color.FromArgb((byte)alpha, theta_increase, min_level, max_level);
                default:
                    return Color.FromArgb((byte)alpha, max_level, min_level, theta_decrease);
            }
        }
    }
}
