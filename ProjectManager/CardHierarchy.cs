using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ProjectManager.Helpers
{
    public class CardHierarchy
    {
        public static readonly DependencyProperty CardLevelProperty =
            DependencyProperty.RegisterAttached("CardLevel", typeof(int), typeof(CardHierarchy), new PropertyMetadata(default(int)));

        public static void SetCardLevel(UIElement element, int value)
        {
            element.SetValue(CardLevelProperty, value);
        }

        public static int GetCardLevel(UIElement element)
        {
            return (int)element.GetValue(CardLevelProperty);
        }
    }
}
