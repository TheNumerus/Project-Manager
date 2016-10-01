using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ProjectManager.Helpers; 

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadOnStartCheckBox.IsChecked = Properties.Settings.Default.LoadOnStart;
            DarkThemeCheckBox.IsChecked = Properties.Settings.Default.DarkTheme;

        }

        private void LoadOnStart_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LoadOnStart = (bool)((CheckBox)sender).IsChecked;
            Properties.Settings.Default.Save();
        }

        private void ThemeChange_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DarkTheme = (bool)((CheckBox)sender).IsChecked;
            Properties.Settings.Default.Save();
            if (ProjectManager.Properties.Settings.Default.DarkTheme)
            {
                ((Application)Application.Current).ChangeTheme(new Uri("pack://application:,,,/Themes/DarkTheme.xaml"));
            }
            else
            {
                ((Application)Application.Current).ChangeTheme(new Uri("pack://application:,,,/Themes/LightTheme.xaml"));
            }
        }
        private void SaveColorValue(object sender, RoutedEventArgs e)
        {
            string ColorString = ColorSelector.Text;
            byte a, r, g, b;
            try
            {
                a = Convert.ToByte(ColorString.Substring(1, 2),16);
                r = Convert.ToByte(ColorString.Substring(3, 2), 16);
                g = Convert.ToByte(ColorString.Substring(5, 2), 16);
                b = Convert.ToByte(ColorString.Substring(7, 2), 16);
            }
            catch
            {
                ErrorWindow er = new ErrorWindow();
                er.Show();
                er.Title = "Incorrent color";
                er.ErrorMessage.Text = "Color format is incorrect";
                return;
            }
            Properties.Settings.Default.AccentColor = Color.FromArgb(a, r, g, b);
            Properties.Settings.Default.AccentColorShade = ColorOperarions.ShadeColor(Color.FromArgb(a, r, g, b));
            Properties.Settings.Default.Save();
        }
    }
}
