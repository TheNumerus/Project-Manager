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
using Microsoft.Win32;

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
                else
                {
                    NahraniDoSeznamu(RuntimeData.runtimeData);
                    ProjectName.Text = RuntimeData.runtimeData.nazev;
                }
            }
            SetWindowName();
        }
        //method for setting new window name
        public void SetWindowName() {
            string label = String.Empty;
            label+="Project Manager - " + ProjectName.Text;
            this.Title = label;
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
            ((Button)novaKarta.Children[6]).Click += MoveCardUp;
            ((Button)novaKarta.Children[7]).Click += MoveCardDown;
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
            //add hash of the created grid to data, so they bind together
            data.GridID = novaKarta.GetHashCode();

        }
        //methods for moving cards in hierarchy
        public void MoveCardLeft(Grid card,int steps = 1) {
            for (int i = 0; i < steps; i++)
            {
                Thickness staryMargin = card.Margin;
                if (CardHierarchy.GetCardLevel(card) > 1)
                {
                    CardHierarchy.SetCardLevel(card, CardHierarchy.GetCardLevel(card) - 1);
                    staryMargin.Left = (CardHierarchy.GetCardLevel(card) - 1) * 20 + 5;
                }
                card.Margin = staryMargin;
            }
            RuntimeData.Generate(Seznam, ProjectName);
        }
        public void MoveCardRight(Grid card,Grid cardAbove, int steps = 1)
        {
            for (int i = 0; i < steps; i++)
            {
                Thickness staryMargin = card.Margin;
                Data data = RuntimeData.FindByID(card.GetHashCode(), RuntimeData.runtimeData);
                Data parent = RuntimeData.FindDataParent(RuntimeData.runtimeData, data);
                //if parent has a child above moved card, move can happen
                if (CardHierarchy.GetCardLevel(card) < 4 &&  parent.Karty.IndexOf(data) >= 1)
                {
                    CardHierarchy.SetCardLevel(card, CardHierarchy.GetCardLevel(card) + 1);
                    staryMargin.Left = (CardHierarchy.GetCardLevel(card) - 1) * 20 + 5;
                }
                card.Margin = staryMargin;
            }
            RuntimeData.Generate(Seznam, ProjectName);
        }
        //method for moving all cards in hierarchy
        public void SortAllCards(StackPanel sp) {
            for(int i = 1;i<sp.Children.Count;i++) {
                Grid card = (Grid)sp.Children[i];
                Grid cardAbove = (Grid)sp.Children[i-1];

                //set first card allways to first place
                if (i == 1)
                {
                    MoveCardLeft(card, 4);
                    //move all cards with invalid hierarchy to level above
                } else if(CardHierarchy.GetCardLevel(card) - CardHierarchy.GetCardLevel(cardAbove) > 1)
                {
                    MoveCardLeft(card);
                }
            }
        }
        //eventy pro tlacitka
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Promazani();
            if (!RuntimeData.Load())
            {
                Grid novaKarta = new Grid();
                PridaniKarty(out novaKarta);
                Seznam.Children.Add(novaKarta);
            }
            else
            {
                NahraniDoSeznamu(RuntimeData.runtimeData);
                ProjectName.Text = RuntimeData.runtimeData.nazev;
            }
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            //RuntimeData.Generate(Seznam,ProjectName);
            RuntimeData.Save();
        }

        private void Pridat_polozku(object sender, RoutedEventArgs e)
        {
            Grid Karta = new Grid();
            PridaniKarty(out Karta);
            //get card number which sent the event, so we can add new card just under it
            int cisloKarty = Seznam.Children.IndexOf((Grid)((Button)sender).Parent);
            Seznam.Children.Insert(cisloKarty + 1, Karta);
            RuntimeData.Generate(Seznam, ProjectName);
        }

        private void Smazat_polozku(object sender, RoutedEventArgs e)
        {
            Seznam.Children.Remove((Grid)((Button)sender).Parent);
            //avoid empty list
            if (Seznam.Children.Count == 1)
            {
                Grid Karta = new Grid();
                PridaniKarty(out Karta);
                Seznam.Children.Add(Karta);
                RuntimeData.Generate(Seznam, ProjectName);
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
            RuntimeData.Generate(Seznam, ProjectName);
        }
        //zajisteni presunu nahoru a dolu
        private void PresunNahoru(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = Seznam.Children.IndexOf((UIElement)btn.Parent);
            Grid card = (Grid)Seznam.Children[index];
            MoveCardLeft(card);
            SortAllCards(Seznam);
        }
        private void PresunDolu(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = Seznam.Children.IndexOf((UIElement)btn.Parent);
            Grid card = (Grid)Seznam.Children[index];
            Grid cardAbove = (Grid)Seznam.Children[index-1];
            MoveCardRight(card, cardAbove);
            SortAllCards(Seznam);
        }
        //open about window
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
        //move card up and down
        private void MoveCardDown(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = Seznam.Children.IndexOf((UIElement)btn.Parent);
            Grid card = (Grid)Seznam.Children[index];
            Seznam.Children.RemoveAt(index);
            //handle last card
            if (index != Seznam.Children.Count)
            {
                Seznam.Children.Insert(index + 1, card);
            }
            else {
                Seznam.Children.Insert(index, card);
            }
            SortAllCards(Seznam);
            RuntimeData.Generate(Seznam, ProjectName);
        }

        private void MoveCardUp(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = Seznam.Children.IndexOf((UIElement)btn.Parent);
            Grid card = (Grid)Seznam.Children[index];
            //handle first card
            if (index != 1)
            {
                Seznam.Children.RemoveAt(index);
                Seznam.Children.Insert(index - 1, card);
            }
            SortAllCards(Seznam);
            RuntimeData.Generate(Seznam, ProjectName);
        }
        //event for opening new file
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Database files (*.xml)|*.xml|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Properties.Settings.Default.PathToFile = openFileDialog.FileName;
                Promazani();
                if (!RuntimeData.Load())
                {
                    Grid novaKarta = new Grid();
                    PridaniKarty(out novaKarta);
                    Seznam.Children.Add(novaKarta);
                }
                else
                {
                    NahraniDoSeznamu(RuntimeData.runtimeData);
                    ProjectName.Text = RuntimeData.runtimeData.nazev;
                }
            }
            SetWindowName();
        }
        //event for saving new file
        private void WriteAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savedia = new SaveFileDialog();
            savedia.Filter = "Database files (*.xml)|*.xml|All files (*.*)|*.*";
            if (savedia.ShowDialog() == true) {
                Properties.Settings.Default.PathToFile = savedia.FileName;
                RuntimeData.Save();
            }
            SetWindowName();
        }
        //sets new window name after editing label
        private void SetNewWindowName(object sender, TextChangedEventArgs e)
        {
            SetWindowName();
        }
    }
}
