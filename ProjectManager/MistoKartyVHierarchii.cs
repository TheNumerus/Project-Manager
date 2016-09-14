using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ProjectManager.Helpers
{
    public class MistoKartyVHierarchii
    {
        public static readonly DependencyProperty UrovenKartyProperty =
            DependencyProperty.RegisterAttached("UrovenKarty", typeof(int), typeof(MistoKartyVHierarchii), new PropertyMetadata(default(int)));

        public static void SetUrovenKarty(UIElement element, int value)
        {
            element.SetValue(UrovenKartyProperty, value);
        }

        public static int GetUrovenKarty(UIElement element)
        {
            return (int)element.GetValue(UrovenKartyProperty);
        }
    }
}
