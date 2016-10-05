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

        byte a, r, g, b;
        public SettingsWindow()
        {
            InitializeComponent();
            LoadOnStartCheckBox.IsChecked = Properties.Settings.Default.LoadOnStart;
            DarkThemeCheckBox.IsChecked = Properties.Settings.Default.DarkTheme;
            FlipColorCheckBox.IsChecked = Properties.Settings.Default.FlipColor;
            Red.Value = Properties.Settings.Default.AccentColor.R;
            Green.Value = Properties.Settings.Default.AccentColor.G;
            Blue.Value = Properties.Settings.Default.AccentColor.B;
        }

        private void LoadOnStart_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LoadOnStart = (bool)((CheckBox)sender).IsChecked;
            Properties.Settings.Default.Save();
        }

        private void FlipColorChange_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.FlipColor = (bool)((CheckBox)sender).IsChecked;
            if (Properties.Settings.Default.FlipColor)
            {
                Properties.Settings.Default.ComplementCol = ColorOperarions.InvertHue(Color.FromArgb(255, r, g, b));
            }
            else {
                Properties.Settings.Default.ComplementCol = Properties.Settings.Default.AccentColor;
            }
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
            if (Properties.Settings.Default.FlipColor)
            {
                Properties.Settings.Default.ComplementCol = ColorOperarions.InvertHue(Color.FromArgb(a, r, g, b));
            }
            else {
                Properties.Settings.Default.ComplementCol = Properties.Settings.Default.AccentColor;
            }
    Properties.Settings.Default.Save();
        }

        private void UpdateColorValue(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Red == null || Green == null || Blue == null)
            {
                r = Red != null ? (byte)Red.Value : (byte)0;
                g = Green != null ? (byte)Green.Value : (byte)0;
                b = Blue != null ? (byte)Blue.Value : (byte)0;
                
            }
            else {
                r = Red != null ? (byte)Red.Value : (byte)0;
                g = Green != null ? (byte)Green.Value : (byte)0;
                b = Blue != null ? (byte)Blue.Value : (byte)0;
                Properties.Settings.Default.AccentColor = Color.FromArgb(255, r, g, b);
                Properties.Settings.Default.AccentColorShade = ColorOperarions.ShadeColor(Properties.Settings.Default.AccentColor);
                if (Properties.Settings.Default.FlipColor)
                {
                    Properties.Settings.Default.ComplementCol = ColorOperarions.InvertHue(Color.FromArgb(255, r, g, b));
                }
                else {
                    Properties.Settings.Default.ComplementCol = Properties.Settings.Default.AccentColor;
                }
                Properties.Settings.Default.Save();
            }
        }
    }
}
