using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Windows.Shapes;

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
        //this method is stored here, so it can be used in mutiple windows
        public static void LabelColorChange(Rectangle rect,int shift) {
            LabelColors currentCol = LabelColorNumbers.GetColorNumber(rect);
            LabelColors newCol = (int)currentCol > 5 ? 0 : currentCol + shift;
            LabelColorNumbers.SetColorNumber(rect, newCol);
            rect.Fill = new SolidColorBrush(LabelColorValues.barva[(int)newCol]);
        }
    }
    public enum LabelColors {
        None,
        Red,
        Blue,
        Green,
        Yellow,
        Violet,
        Grey
    };
    public static class LabelColorValues {
        public static Color[] barva = {Colors.White,Colors.Firebrick,Colors.SteelBlue,Colors.YellowGreen,Colors.Gold,Colors.Indigo,Colors.Gray};
    }
}
