using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMine.ColorSpaces.Conversions;
using ColorMine;
using ColorMine.ColorSpaces;

namespace ProjectManager.Helpers
{
    public static class ColorOperarions
    {
        public static Color ShadeColor(Color baseCol,float percent = 50) {
            byte r, g, b;
            r = (byte)(baseCol.R + ((255 - baseCol.R) * percent/100));
            g = (byte)(baseCol.G + ((255 - baseCol.G) * percent/100));
            b = (byte)(baseCol.B + ((255 - baseCol.B) * percent/100));
            return Color.FromArgb(255,r,g,b);
        }
        public static Color InvertHue(Color baseCol) {
            var oldColor = new Rgb { R = baseCol.R,G = baseCol.G,B = baseCol.B };
            var hsv = oldColor.To<Hsv>();
            hsv.H = (hsv.H + 180) < 360 ? hsv.H+180 : hsv.H-180;
            hsv.V = 0.8;
            hsv.S = 0.5;
            var newColor = hsv.To<Rgb>();
            return Color.FromRgb((byte)newColor.R, (byte)newColor.G, (byte)newColor.B);

        }
    }
}
