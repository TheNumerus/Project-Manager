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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;
using System.Xml.Serialization;
using System.Xml;
using ProjectManager.Helpers;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Grid novaKarta = new Grid();
            PridaniKarty(out novaKarta);
            Seznam.Children.Add(novaKarta);
            //zakladni karta je neviditelna aby nesla smazat a upravit
            ZakladniKarta.Visibility = Visibility.Collapsed;
            //pokud uzivatel chce, muze aplikace nacitst data pri startu
            if (Properties.Settings.Default.LoadOnStart) {
                Promazani();
                if (!RuntimeData.Load())
                {
                    novaKarta = new Grid();
                    PridaniKarty(out novaKarta);
                    Seznam.Children.Add(novaKarta);
                }
                NahraniDoSeznamu(RuntimeData.runtimeData);
                ProjectName.Text = RuntimeData.runtimeData.nazev;
            }
        }
        //Metody pro zjednoduseni event handleru a pro mozne pouziti ve vice eventech

        public void PridaniKarty(out Grid novaKarta)
        {
            //zparsovani do stringu a nasledne nakopirovani
            var xaml = XamlWriter.Save(ZakladniKarta);
            StringReader stringReader = new StringReader(xaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            novaKarta = (Grid)XamlReader.Load(xmlReader);
            //pridani eventu k tlacitkum a labelu
            ((Button)novaKarta.Children[1]).Click += Smazat_polozku;
            ((Button)novaKarta.Children[2]).Click += Pridat_polozku;
            ((Rectangle)novaKarta.Children[3]).MouseEnter += PrejetiLabeluOn;
            ((Rectangle)novaKarta.Children[3]).MouseLeave += PrejetiLabeluOff;
            ((Rectangle)novaKarta.Children[3]).MouseDown += ZmenaBarvyLabelu;
            ((Button)novaKarta.Children[4]).Click += PresunNahoru;
            ((Button)novaKarta.Children[5]).Click += PresunDolu;
            //add event to open detail window
            novaKarta.MouseDown += OpenDetailsWindow_Event;
            //zakladni karta je neviditelna, takze jeji kopie se musi zviditelnit
            novaKarta.Visibility = Visibility.Visible;
        }
        //smaze vsechny polozky v seznamu
        public void Promazani() {
            for (int i = Seznam.Children.Count; i > 1; i--) {
                Seznam.Children.RemoveAt(i-1);
            }
        }
        //aby bylo podporovano nahrani deti, musi existovat rekurzivni funkce
        public void NahraniDoSeznamu(Data data) {
            //nacteni sebe, pokud to neni koren celeho seznamu
            if (data.pozice != 0)
            {
                PridaniDoSeznamu(data);
            }
            //nacteni deti
            for (int i = 0; i < data.Karty.Count; i++)
            {
                PridaniDoSeznamu(data.Karty[i]);
                //pokud ma dite deti tak to vypise i je
                foreach (Data d in data.Karty[i].Karty) {
                    NahraniDoSeznamu(d);
                }
            }
        }
        public void PridaniDoSeznamu(Data data) {
            Grid novaKarta = new Grid();
            PridaniKarty(out novaKarta);
            ((TextBox)novaKarta.Children[0]).Text = data.nazev;
            //nastaveni barevnych labelu
            Rectangle LabelNew = (Rectangle)(novaKarta.Children[3]);
            LabelColorNumbers.SetColorNumber(LabelNew, data.labelColor);
            CardHierarchy.SetCardLevel(novaKarta, data.pozice);
            LabelNew.Fill = new SolidColorBrush(LabelColorValues.barva[(int)data.labelColor]);
            //nastaveni marginu podle urovne karty
            Thickness marginNew = novaKarta.Margin;
            marginNew.Left = (CardHierarchy.GetCardLevel(novaKarta) - 1) * 20 + 5;
            novaKarta.Margin = marginNew;
            //pridani karty do seznamu
            Seznam.Children.Add(novaKarta);
        }
        //eventy pro tlacitka
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Promazani();
            //Nacteni();
            if (!RuntimeData.Load())
            {
                Grid novaKarta = new Grid();
                PridaniKarty(out novaKarta);
                Seznam.Children.Add(novaKarta);
            }
            NahraniDoSeznamu(RuntimeData.runtimeData);
            ProjectName.Text = RuntimeData.runtimeData.nazev;
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            RuntimeData.Generate(Seznam,ProjectName);
            RuntimeData.Save();
        }

        private void Pridat_polozku(object sender, RoutedEventArgs e)
        {
            Grid Karta = new Grid();
            PridaniKarty(out Karta);
            //zjisti poradove cislo tlacitka ktere vyslalo event
            int cisloKarty = Seznam.Children.IndexOf((Grid)((Button)sender).Parent);
            Seznam.Children.Insert(cisloKarty + 1, Karta);
        }

        private void Smazat_polozku(object sender, RoutedEventArgs e)
        {
            Seznam.Children.Remove((Grid)((Button)sender).Parent);
            //zabraneni prazdnemu listu
            if (Seznam.Children.Count == 1)
            {
                Grid Karta = new Grid();
                PridaniKarty(out Karta);
                Seznam.Children.Add(Karta);
            }
        }

        //zvetseni pri prejeti po labelu
        private void PrejetiLabeluOn(object sender, MouseEventArgs e)
        {
            ((Rectangle)sender).Height = 10;
        }
        //zmenseni pri prejeti od labelu
        private void PrejetiLabeluOff(object sender, MouseEventArgs e)
        {
            ((Rectangle)sender).Height = 4;
        }
        //cyklicka zmena barvy labelu
        private void ZmenaBarvyLabelu(object sender, MouseButtonEventArgs e)
        {
            LabelColorNumbers.LabelColorChange((Rectangle)sender,1);
        }
        //zajisteni presunu nahoru a dolu
        private void PresunNahoru(object sender, RoutedEventArgs e)
        {
            Button tlacitko = (Button)sender;
            int pozice = Seznam.Children.IndexOf((UIElement)tlacitko.Parent);
            Grid karta = (Grid)Seznam.Children[pozice];
            Thickness staryMargin = (karta).Margin;
            if (CardHierarchy.GetCardLevel(karta) > 1)
            {
                CardHierarchy.SetCardLevel(karta, CardHierarchy.GetCardLevel(karta) - 1);
                staryMargin.Left = (CardHierarchy.GetCardLevel(karta)-1) * 20 + 5;
            }
            karta.Margin = staryMargin;
        }
        private void PresunDolu(object sender, RoutedEventArgs e)
        {
            Button tlacitko = (Button)sender;
            int pozice = Seznam.Children.IndexOf((UIElement)tlacitko.Parent);
            Grid karta = (Grid)Seznam.Children[pozice];
            Grid kartaNad = (Grid)Seznam.Children[pozice-1];
            Thickness staryMargin = (karta).Margin;
            if (CardHierarchy.GetCardLevel(karta) < 4 && CardHierarchy.GetCardLevel(karta) - CardHierarchy.GetCardLevel(kartaNad) < 1)
            {
                CardHierarchy.SetCardLevel(karta, CardHierarchy.GetCardLevel(karta) + 1);
                staryMargin.Left = (CardHierarchy.GetCardLevel(karta)-1) * 20 + 5;
            }
            karta.Margin = staryMargin;
        }
        //event pro otevreni about okna
        private void OpenAboutWindow(object sender, RoutedEventArgs e) {
            AboutWindow aw = new AboutWindow();
            aw.Show();
        }
        //open settings window
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.Show();
        }
        //open details window
        private void OpenDetailsWindow_Event(object sender, MouseButtonEventArgs e)
        {
            //Ensure that the event is double-click
            if (e.ClickCount == 2)
            {
                //open window and set parameters
                DetailsWindow detWin = new DetailsWindow();
                detWin.AddCardInfo((Grid)sender);
                detWin.Show();
                //set focus to newly opened window
                detWin.Focus();
            }
        }
    }
}
