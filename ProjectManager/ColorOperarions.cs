using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
