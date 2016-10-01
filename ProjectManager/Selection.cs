using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectManager.Helpers
{
    class Selection
    {
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.RegisterAttached("Selected", typeof(bool), typeof(Selection), new PropertyMetadata(default(bool)));

        public static void SetSelecetion(UIElement element, bool value)
        {
            element.SetValue(SelectedProperty, value);
        }

        public static void ToggleSelecetion(UIElement element)
        {
            element.SetValue(SelectedProperty, !(bool)element.GetValue(SelectedProperty));
        }

        public static bool GetSelection(UIElement element)
        {
            return (bool)element.GetValue(SelectedProperty);
        }
    }
}
