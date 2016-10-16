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
using System.Text.RegularExpressions;
using System.Net;

namespace ProjectManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //variable for drag and drop
        Point lastClick;
        Border lastClickedCard;

        public MainWindow()
        {
            InitializeComponent();
            //main card is invisible, so user can't change it
            ZakladniKarta.Visibility = Visibility.Collapsed;
            Border newCard = new Border();
            CreateCard(out newCard);
            Seznam.Children.Add(newCard);
            AddCardInfo(newCard);
            RuntimeData.Generate(Seznam,ProjectName);
            //if user wants, app can load file on start
            if (Properties.Settings.Default.LoadOnStart) {
                DeleteAllCards();
                if (!RuntimeData.LoadJSON())
                {
                    newCard = new Border();
                    CreateCard(out newCard);
                    Seznam.Children.Add(newCard);
                    RuntimeData.Generate(Seznam,ProjectName);
                    AddCardInfo((Border)Seznam.Children[1]);
                }
                else
                {
                    GenerateFromData(RuntimeData.runtimeData.list);
                    ProjectName.Text = RuntimeData.runtimeData.list.name;
                    //autofocus first card
                    AddCardInfo((Border)Seznam.Children[1]);
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
        //Methods for simplifiyng event handlers
        public void CreateCard(out Border cardContainer)
        {
            //parsing to string and generating new card
            var xaml = XamlWriter.Save(ZakladniKarta);
            StringReader stringReader = new StringReader(xaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            cardContainer = (Border)XamlReader.Load(xmlReader);
            Grid card = (Grid)cardContainer.Child;
            //adding event handlers
            ((Button)card.Children[1]).Click += DeleteCard;
            ((Button)card.Children[3]).Click += MoveLeftButton_Click;
            ((Button)card.Children[4]).Click += MoveRightButton_Click;
            ((Button)card.Children[5]).Click += MoveCardUp_Event;
            ((Button)card.Children[6]).Click += MoveCardDown_Event;
            //add event to open detail window and drag n drop
            cardContainer.PreviewMouseLeftButtonDown += ClickCardEvent;
            cardContainer.PreviewMouseLeftButtonUp += CardLMBUp_Event;
            //main card is invisible, so we must set new card to be visible
            cardContainer.Visibility = Visibility.Visible;
        }
        //deletes all cards
        public void DeleteAllCards() {
            for (int i = Seznam.Children.Count; i > 1; i--) {
                Seznam.Children.RemoveAt(i-1);
            }
        }
        //to be able to load children, we must have recursive function
        public void GenerateFromData(Data data) {
            //load self, if not root of list
            if (data.level != 0)
            {
                AddToList(data);
            }
            //load children
            for (int i = 0; i < data.cards.Count; i++)
            {
                AddToList(data.cards[i]);
                //if child had children, add them too
                foreach (Data d in data.cards[i].cards) {
                    GenerateFromData(d);
                }
            }
        }
        public void AddToList(Data data) {
            Border cardContainer = new Border();
            CreateCard(out cardContainer);
            Grid newCard = (Grid)cardContainer.Child;
            ((TextBox)newCard.Children[0]).Text = data.name;
            //set label color
            Rectangle LabelNew = (Rectangle)(newCard.Children[2]);
            LabelColorNumbers.SetColorNumber(LabelNew, data.labelColor);
            CardHierarchy.SetCardLevel(cardContainer, data.level);
            LabelNew.Fill = new SolidColorBrush(LabelColorValues.barva[(int)data.labelColor]);
            //set margin according to hierarchy level
            Thickness marginNew = cardContainer.Margin;
            marginNew.Left = (CardHierarchy.GetCardLevel(cardContainer) - 1) * 20 + 5;
            cardContainer.Margin = marginNew;
            //add card to list
            Seznam.Children.Add(cardContainer);
            //add hash of the created grid to data, so they bind together
            data.GridID = cardContainer.GetHashCode();

        }
        //methods for moving cards in hierarchy
        public void MoveCardLeft(Border card,int steps = 1) {
            for (int i = 0; i < steps; i++)
            {
                Thickness oldMargin = card.Margin;
                if (CardHierarchy.GetCardLevel(card) > 1)
                {
                    CardHierarchy.SetCardLevel(card, CardHierarchy.GetCardLevel(card) - 1);
                    oldMargin.Left = (CardHierarchy.GetCardLevel(card) - 1) * 20 + 5;
                }
                card.Margin = oldMargin;
            }
            RuntimeData.Generate(Seznam, ProjectName);
        }
        public void MoveCardRight(Border card,Border cardAbove, int steps = 1)
        {
            for (int i = 0; i < steps; i++)
            {
                Thickness oldMargin = card.Margin;
                Data data = RuntimeData.FindByID(card.GetHashCode(), RuntimeData.runtimeData.list);
                Data parent = RuntimeData.FindDataParent(RuntimeData.runtimeData.list, data);
                //if parent has a child above moved card, move can happen
                if (CardHierarchy.GetCardLevel(card) < 4 &&  parent.cards.IndexOf(data) >= 1)
                {
                    CardHierarchy.SetCardLevel(card, CardHierarchy.GetCardLevel(card) + 1);
                    oldMargin.Left = (CardHierarchy.GetCardLevel(card) - 1) * 20 + 5;
                }
                card.Margin = oldMargin;
            }
            RuntimeData.Generate(Seznam, ProjectName);
        }
        //method for moving all cards in hierarchy
        public void SortAllCards(StackPanel sp) {
            for(int i = 1;i<sp.Children.Count;i++) {
                Border card = (Border)sp.Children[i];
                Border cardAbove = (Border)sp.Children[i-1];

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
        //events for buttons
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteAllCards();
            if (!RuntimeData.LoadJSON())
            {
                Border newCard = new Border();
                CreateCard(out newCard);
                Seznam.Children.Add(newCard);
            }
            else
            {
                GenerateFromData(RuntimeData.runtimeData.list);
                ProjectName.Text = RuntimeData.runtimeData.list.name;
            }
        }

        private void WriteButton_Click(object sender, RoutedEventArgs e)
        {
            RuntimeData.Generate(Seznam,ProjectName);
            RuntimeData.SaveJSON();
        }

        private void DeleteCard(object sender, RoutedEventArgs e)
        {
            RuntimeData.RecordUndo();

            //delete multiple cards
            int cardsDeleted = 0;
            for (int i = 0; i < Seznam.Children.Count - 1; i ++)
            {
                if (Selection.GetSelection(Seznam.Children[i]) == true)
                {
                    Seznam.Children.RemoveAt(i);
                    i--;
                    cardsDeleted++;
                }
            }
            //delete self
            if (cardsDeleted == 0) {
                Seznam.Children.Remove((Border)((Grid)((Button)sender).Parent).Parent);
            }
            RuntimeData.Generate(Seznam,ProjectName);
            //avoid empty list
            if (Seznam.Children.Count == 1)
            {
                Border newCard = new Border();
                CreateCard(out newCard);
                Seznam.Children.Add(newCard);
                RuntimeData.Generate(Seznam, ProjectName);
            }
        }
        //moving up and down
        private void MoveLeftButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = Seznam.Children.IndexOf((Border)((Grid)btn.Parent).Parent);
            Border card = (Border)Seznam.Children[index];
            MoveCardLeft(card);
            SortAllCards(Seznam);
        }
        private void MoveRightButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = Seznam.Children.IndexOf((Border)((Grid)btn.Parent).Parent);
            Border card = (Border)Seznam.Children[index];
            Border cardAbove = (Border)Seznam.Children[index-1];
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
        private void ClickCardEvent(object sender, MouseButtonEventArgs e)
        {
            Border card = (Border)sender;
            lastClick = e.GetPosition(Seznam);
            lastClickedCard = card;
            //handling shift click to select card
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                Selection.ToggleSelecetion(card);
                if (!Selection.GetSelection(card))
                {
                    ((Border)sender).SetResourceReference(BackgroundProperty, "CardColor");
                }
                else
                {
                    ((Border)sender).SetResourceReference(BackgroundProperty, "ComplementColor");
                }
            }
            else {
                //reset coloration
                foreach (Border b in Seznam.Children)
                {
                    b.SetResourceReference(BackgroundProperty, "CardColor");
                    Selection.SetSelecetion(b, false);
                }
                card.SetResourceReference(BackgroundProperty, "AccentBackgroundBrush");
                AddCardInfo(card);
            }
        }
        //move card up and down
        private void MoveCardDown_Event(object sender, RoutedEventArgs e)
        {
            Border card = (Border)((Grid)((Button)sender).Parent).Parent;
            MoveCardDown(card);
        }
        public void MoveCardDown(Border card) {
            int index = Seznam.Children.IndexOf(card);
            if (index < Seznam.Children.Count && index > 0)
            {
                Seznam.Children.RemoveAt(index);
                //handle last card
                if (index != Seznam.Children.Count)
                {
                    Seznam.Children.Insert(index + 1, card);
                }
                else
                {
                    Seznam.Children.Insert(index, card);
                }
                SortAllCards(Seznam);
                RuntimeData.Generate(Seznam, ProjectName);
            }
        }
        private void MoveCardUp_Event(object sender, RoutedEventArgs e)
        {
            Border card = (Border)((Grid)((Button)sender).Parent).Parent;
            MoveCardUp(card);
        }
        public void MoveCardUp(Border card) {
            int index = Seznam.Children.IndexOf(card);
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
            openFileDialog.Filter = "Database files (*.json)|*.json|Old database files (*.xml)|*.xml|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Properties.Settings.Default.PathToFile = openFileDialog.FileName;
                DeleteAllCards();
                if (!RuntimeData.LoadJSON() || RuntimeData.runtimeData.list.cards == null)
                {
                    Border newCard = new Border();
                    CreateCard(out newCard);
                    Seznam.Children.Add(newCard);
                }
                else
                {
                    GenerateFromData(RuntimeData.runtimeData.list);
                    ProjectName.Text = RuntimeData.runtimeData.list.name;
                    Properties.Settings.Default.Save();
                }
            }
            SetWindowName();
        }
        //event for saving new file
        private void WriteAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savedia = new SaveFileDialog();
            savedia.Filter = "Database files (*.json)|*.json|All files (*.*)|*.*";
            if (savedia.ShowDialog() == true) {
                Properties.Settings.Default.PathToFile = savedia.FileName;
                Properties.Settings.Default.Save();
                RuntimeData.SaveJSON();
            }
            SetWindowName();
        }
        //sets new window name after editing label
        private void SetNewWindowName(object sender, TextChangedEventArgs e)
        {
            SetWindowName();
        }
        //events for details subwindow

        //reference to card
        public Border CardContainer;
        public Data attachedData;
        //Here we add content info from card
        public void AddCardInfo(Border srcBorder)
        {
            CardContainer = srcBorder;
            Grid Card = (Grid)CardContainer.Child;
            attachedData = RuntimeData.FindByID(CardContainer.GetHashCode(), RuntimeData.runtimeData.list);
            nameBox.Text = ((TextBox)(Card.Children[0])).Text;
            //descBox.Text = ((TextBox)(Card.Children[0])).Text;
            LabelRect.Fill = new SolidColorBrush(LabelColorValues.barva[(int)LabelColorNumbers.GetColorNumber(Card.Children[2])]);
            LabelColorNumbers.SetColorNumber(LabelRect, LabelColorNumbers.GetColorNumber(Card.Children[2]));
            //handling empty reference
            if (attachedData != null)
            {
                descBox.Text = attachedData.description;
                //handle empty date
                if (attachedData.changeDate.Year != 1)
                {
                    dateBlock.Text = attachedData.changeDate.Date.ToLongDateString();
                }
                else
                {
                    dateBlock.Visibility = Visibility.Hidden;
                }
            }
        }

        private void NameChanged_Event(object sender, TextChangedEventArgs e)
        {
            //ensure the card is set
            if (CardContainer != null)
            {
                Grid Card = (Grid)CardContainer.Child;
                ((TextBox)(Card.Children[0])).Text = nameBox.Text;
                RuntimeData.Generate(Seznam, ProjectName);
            }
        }
        //set new label color in window and update card label
        private void LabelChange_Event(object sender, MouseButtonEventArgs e)
        {
            Grid Card = (Grid)CardContainer.Child;
            LabelColorNumbers.LabelColorChange(LabelRect, 1);
            LabelColorNumbers.LabelColorChange((Rectangle)(Card.Children[2]), 1);
            attachedData.labelColor = LabelColorNumbers.GetColorNumber(LabelRect);
            RuntimeData.Generate(Seznam, ProjectName);
        }

        private void DescChanged_Event(object sender, TextChangedEventArgs e)
        {
            //ensure the card is set
            if (CardContainer != null && descBox.Text != null)
            {
                Grid Card = (Grid)CardContainer.Child;
                attachedData.description = descBox.Text;
                //parse url
                string text = descBox.Text;
                Match url = Regex.Match(text, @"((http:[/]{2})|(https:[/]{2})).+((\.jpg)|(\.png))");
                string result = url.Value;
                Match filename = Regex.Match(result, @"(([^/]+)((\.jpg)|(\.png)))$");
                if (url.Success)
                {
                    if (!Directory.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "/DownloadedImages"))
                    {
                        Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "/DownloadedImages");
                    }
                    if (!File.Exists("DownloadedImages/"+filename.Value))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFile(new Uri(result),filename.Value);
                            File.Move(filename.Value, "DownloadedImages/" + filename.Value);
                        }
                    }
                    descImage.Source = new BitmapImage(new Uri(System.AppDomain.CurrentDomain.BaseDirectory + "/DownloadedImages/" + filename.Value));
                    RuntimeData.Generate(Seznam, ProjectName);
                }
                else {
                    descImage.Source = null;
                }
            }
        }
        //resets focuses and selection
        private void ResetFocus(object sender, MouseButtonEventArgs e)
        {
            foreach (Border brd in Seznam.Children) {
                brd.SetResourceReference(BackgroundProperty, "CardColor");
                Selection.SetSelecetion((UIElement)sender, false);
            }
            //lastClickedCard = null;
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            RuntimeData.RecordUndo();
            RuntimeData.Undo();
            DeleteAllCards();
            GenerateFromData(RuntimeData.runtimeData.list);
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            RuntimeData.Redo();
            DeleteAllCards();
            GenerateFromData(RuntimeData.runtimeData.list);
        }

        private void AddButtonClicked(object sender, MouseButtonEventArgs e)
        {
            Border card = new Border();
            CreateCard(out card);
            Seznam.Children.Add(card);
            RuntimeData.Generate(Seznam, ProjectName);
            //here we assign new date to card data
            Data data = RuntimeData.FindByID(card.GetHashCode(), RuntimeData.runtimeData.list);
            data.changeDate = DateTime.Now;
            //focus new card
            AddCardInfo((Border)Seznam.Children[Seznam.Children.Count-1]);
        }

        private void CardLMBUp_Event(object sender, MouseButtonEventArgs e)
        {
            Vector change = Point.Subtract(lastClick, e.GetPosition(Seznam));
            if (change.Length > 10)
            {
                //MessageBox.Show(change.ToString());
                if (change.Y > 50) {
                    MoveCardUp(lastClickedCard);
                }
                if (change.Y < -50)
                {
                    MoveCardDown(lastClickedCard);
                }
            }
        }

        private void SettingsButton_Click(object sender, MouseButtonEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.Show();
        }
    }
}
