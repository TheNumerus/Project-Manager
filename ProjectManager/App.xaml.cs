using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class Application : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //set light-dark theme
            if (ProjectManager.Properties.Settings.Default.DarkTheme) {
                ChangeTheme(new Uri("pack://application:,,,/Themes/DarkTheme.xaml") );
            }
            else{
                ChangeTheme(new Uri("pack://application:,,,/Themes/LightTheme.xaml"));
            }
            MainWindow mw = new MainWindow();
            mw.Show();
        }

        public void ChangeTheme(Uri uri)
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
        }
    }
}
