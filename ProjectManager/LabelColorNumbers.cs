using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace ProjectManager.Helpers
{
    public class LabelColorNumbers
    {
        public static readonly DependencyProperty ColorNumberProperty =
            DependencyProperty.RegisterAttached("ColorNumber", typeof(LabelColors), typeof(LabelColorNumbers), new PropertyMetadata(default(LabelColors)));

        public static void SetColorNumber(UIElement element, LabelColors value)
        {
            element.SetValue(ColorNumberProperty, value);
        }

        public static LabelColors GetColorNumber(UIElement element)
        {
            return (LabelColors)element.GetValue(ColorNumberProperty);
        }
    }
    public enum LabelColors {
        None,
        Red,
        Blue,
        Green,
        Yellow };
    public static class LabelColorValues {
        public static Color[] barva = {Color.FromArgb(0,0,0,0),Colors.Firebrick,Colors.SteelBlue,Colors.YellowGreen,Colors.Gold};
    }
}
