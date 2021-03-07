using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Haley.Models
{ 
    public class ImageInfo
    {
        public BitmapSource source { get; set; }
        public int pixel_width { get; set; }
        public int pixel_height { get; set; }
        public double dpiX { get; set; }
        public double dpiY { get; set; }
        public int length { get; set; }
        public int stride { get; set; }
        public PixelFormat format { get; set; }
        public BitmapMetadata metadata { get; set; }
        public ImageInfo() { }
    }
}
