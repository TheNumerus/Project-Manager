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
                Nacteni();
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
            //zakladni karta je neviditelna, takze jeji kopie se musi zviditelnit
            novaKarta.Visibility = Visibility.Visible;
        }
        //smaze vsechny polozky v seznamu
        public void Promazani() {
            for (int i = Seznam.Children.Count; i > 1; i--) {
                Seznam.Children.RemoveAt(i-1);
            }
        }
        public void Nacteni() {
            String cesta = Application.Current.FindResource("PathToFile").ToString();
            //nacteni souboru
            FileStream databaze;
            try
            {
                databaze = File.Open(cesta, FileMode.Open);
            }
            catch (System.IO.FileNotFoundException) {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Soubor neexistuje";
                errorwindow.ErrorMessage.Text = "Chyba! Soubor neexistuje.";
                errorwindow.Show();
                Grid novaKarta = new Grid();
                PridaniKarty(out novaKarta);
                Seznam.Children.Add(novaKarta);
                return;
            }
            catch (System.UnauthorizedAccessException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Nedostatečná práva";
                errorwindow.ErrorMessage.Text = "Chyba! Program nemá potřebná práva pro načtení souboru.";
                errorwindow.Show();
                Grid novaKarta = new Grid();
                PridaniKarty(out novaKarta);
                Seznam.Children.Add(novaKarta);
                return;
            }
            StreamReader sr = new StreamReader(databaze);
            XmlSerializer xs = new XmlSerializer(typeof(Data));
            Data data = new Data();
            //osetreni vadneho souboru
            try
            {
                data = (Data)xs.Deserialize(sr);
            }
            catch(System.InvalidOperationException) {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "XML Error";
                errorwindow.ErrorMessage.Text = "Chyba! Soubor s databází je poškozen.";
                errorwindow.Show();
                Grid novaKarta = new Grid();
                PridaniKarty(out novaKarta);
                Seznam.Children.Add(novaKarta);
                sr.Close();
                databaze.Close();
                return;
            }
            NahraniDoSeznamu(data);
            sr.Close();
            databaze.Close();
            ProjectName.Text = data.nazev;
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
            MistoKartyVHierarchii.SetUrovenKarty(novaKarta, data.pozice);
            LabelNew.Fill = new SolidColorBrush(LabelColorValues.barva[(int)data.labelColor]);
            //nastaveni marginu podle urovne karty
            Thickness marginNew = novaKarta.Margin;
            marginNew.Left = (MistoKartyVHierarchii.GetUrovenKarty(novaKarta) - 1) * 20;
            novaKarta.Margin = marginNew;
            //pridani karty do seznamu
            Seznam.Children.Add(novaKarta);
        }
        public void Ulozeni() {
            String cesta = Application.Current.FindResource("PathToFile").ToString();
            FileStream databaze;
            //osetreni chybejicim pravum k zapisu souboru
            try
            {
                databaze = File.Open(cesta, FileMode.Create);
            }
            catch (System.UnauthorizedAccessException) {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Nedostatečná práva";
                errorwindow.ErrorMessage.Text = "Chyba! Program nemá potřebná práva pro uložení souboru.";
                errorwindow.Show();
                return;
            }
            StreamWriter sw = new StreamWriter(databaze);
            XmlSerializer xs = new XmlSerializer(typeof(Data));
            Data data = new Data(ProjectName.Text,LabelColors.None,0);
            //tahle promenna je kvuli tomu, aby se bylo mozno odkazat na predchozi misto ulozeni do datove struktury
            Data mistoNaUlozeni = data;
            for (int i = 0; i < Seznam.Children.Count; i++)
            {
                //zajisteni,ze karta nad existuje
                Grid kartaNad = new Grid();
                if (i != 0)
                {
                    kartaNad = (Grid)Seznam.Children[i - 1];
                }
                //pokud ne, tak se nastavi standartni misto v hierarchii
                else {
                    MistoKartyVHierarchii.SetUrovenKarty(kartaNad, 1);
                }
                Grid karta = (Grid)Seznam.Children[i];
                TextBox nazevKarty = (TextBox)karta.Children[0];
                if (karta.Visibility != Visibility.Collapsed) 
                {
                    //pokud je misto karty v hierarchii mensi nez karty nad ni, ulozi se karta do "deti" predchozi karty
                    if (MistoKartyVHierarchii.GetUrovenKarty((Grid)Seznam.Children[i - 1]) < MistoKartyVHierarchii.GetUrovenKarty((Grid)Seznam.Children[i]))
                    {
                        mistoNaUlozeni = mistoNaUlozeni.Karty[mistoNaUlozeni.Karty.Count-1];
                    }
                    //pokud je misto karty v hierarchii vetsi nez karty nad ni, ulozi se karta o uroven vys
                    else if (MistoKartyVHierarchii.GetUrovenKarty((Grid)Seznam.Children[i - 1]) > MistoKartyVHierarchii.GetUrovenKarty((Grid)Seznam.Children[i]))
                    {
                        //vyhledani mista pro ulozeni
                        mistoNaUlozeni = nalezeniMistaProUlozeni(data,mistoNaUlozeni);
                    }
                    //pokud je misto karty v hierarchii stejne jako v karte nad ni, ulozi se karta na stejne misto
                    mistoNaUlozeni.Karty.Add(new Data(nazevKarty.Text, LabelColorNumbers.GetColorNumber((Rectangle)karta.Children[3]), MistoKartyVHierarchii.GetUrovenKarty(karta)));
                }
            }
            xs.Serialize(sw, data);
            sw.Close();
            databaze.Close();
        }
        //rekurzivni hledani nadrizeneho mista pro ulozeni
        public Data nalezeniMistaProUlozeni(Data koren,Data diteHledaneho) {
            Data nalezenyPrvek = null;
            foreach (Data d in koren.Karty) {
                if (d.Equals(diteHledaneho))
                {
                    nalezenyPrvek = koren;
                    break;
                }
                else {
                    nalezenyPrvek = nalezeniMistaProUlozeni(d, diteHledaneho);
                }
            }
            return nalezenyPrvek;
        }
        //eventy pro tlacitka
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Promazani();
            Nacteni();
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            Ulozeni();
        }

        private void Pridat_polozku(object sender, RoutedEventArgs e)
        {
            Grid Karta = new Grid();
            PridaniKarty(out Karta);
            Seznam.Children.Add(Karta);
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
            LabelColors soucasneCislo = LabelColorNumbers.GetColorNumber((UIElement)sender);
            LabelColors noveCislo = (int)soucasneCislo > 3 ? 0 : soucasneCislo + 1;
            LabelColorNumbers.SetColorNumber((UIElement)sender,noveCislo);
            ((Rectangle)sender).Fill = new SolidColorBrush(LabelColorValues.barva[(int)noveCislo]);
        }
        //zajisteni presunu nahoru a dolu
        private void PresunNahoru(object sender, RoutedEventArgs e)
        {
            Button tlacitko = (Button)sender;
            int pozice = Seznam.Children.IndexOf((UIElement)tlacitko.Parent);
            Grid karta = (Grid)Seznam.Children[pozice];
            Thickness staryMargin = (karta).Margin;
            if (MistoKartyVHierarchii.GetUrovenKarty(karta) > 1)
            {
                MistoKartyVHierarchii.SetUrovenKarty(karta, MistoKartyVHierarchii.GetUrovenKarty(karta) - 1);
                staryMargin.Left = (MistoKartyVHierarchii.GetUrovenKarty(karta)-1) * 20;
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
            if (MistoKartyVHierarchii.GetUrovenKarty(karta) < 4 && MistoKartyVHierarchii.GetUrovenKarty(karta) - MistoKartyVHierarchii.GetUrovenKarty(kartaNad) < 1)
            {
                MistoKartyVHierarchii.SetUrovenKarty(karta, MistoKartyVHierarchii.GetUrovenKarty(karta) + 1);
                staryMargin.Left = (MistoKartyVHierarchii.GetUrovenKarty(karta)-1) * 20;
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
    }
}
