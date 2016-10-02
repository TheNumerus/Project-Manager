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
            Grid newCard = new Grid();
            CreateCard(out newCard);
            Seznam.Children.Add(newCard);
            //main card is invisible, so user can't change it
            ZakladniKarta.Visibility = Visibility.Collapsed;
            //if user wants, app can load file on start
            if (Properties.Settings.Default.LoadOnStart) {
                DeleteAllCards();
                if (!RuntimeData.Load())
                {
                    newCard = new Grid();
                    CreateCard(out newCard);
                    Seznam.Children.Add(newCard);
                    RuntimeData.Generate(Seznam,ProjectName);
                    AddCardInfo((Grid)Seznam.Children[1]);
                }
                else
                {
                    GenerateFromData(RuntimeData.runtimeData.list);
                    ProjectName.Text = RuntimeData.runtimeData.list.name;
                    //autofocus first card
                    AddCardInfo((Grid)Seznam.Children[1]);
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
        public void CreateCard(out Grid newCard)
        {
            //parsing to string and generating new card
            var xaml = XamlWriter.Save(ZakladniKarta);
            StringReader stringReader = new StringReader(xaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            newCard = (Grid)XamlReader.Load(xmlReader);
            //adding event handlers
            ((Button)newCard.Children[1]).Click += DeleteCard;
            ((Button)newCard.Children[2]).Click += AddNewCard;
            ((Button)newCard.Children[4]).Click += MoveLeftButton_Click;
            ((Button)newCard.Children[5]).Click += MoveRightButton_Click;
            ((Button)newCard.Children[6]).Click += MoveCardUp;
            ((Button)newCard.Children[7]).Click += MoveCardDown;
            //add event to open detail window
            newCard.MouseDown += ClickCardEvent;
            //main card is invisible, so we must set new card to be visible
            newCard.Visibility = Visibility.Visible;
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
            Grid newCard = new Grid();
            CreateCard(out newCard);
            ((TextBox)newCard.Children[0]).Text = data.name;
            //set label color
            Rectangle LabelNew = (Rectangle)(newCard.Children[3]);
            LabelColorNumbers.SetColorNumber(LabelNew, data.labelColor);
            CardHierarchy.SetCardLevel(newCard, data.level);
            LabelNew.Fill = new SolidColorBrush(LabelColorValues.barva[(int)data.labelColor]);
            //set margin according to hierarchy level
            Thickness marginNew = newCard.Margin;
            marginNew.Left = (CardHierarchy.GetCardLevel(newCard) - 1) * 20 + 5;
            newCard.Margin = marginNew;
            //add card to list
            Seznam.Children.Add(newCard);
            //add hash of the created grid to data, so they bind together
            data.GridID = newCard.GetHashCode();

        }
        //methods for moving cards in hierarchy
        public void MoveCardLeft(Grid card,int steps = 1) {
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
        public void MoveCardRight(Grid card,Grid cardAbove, int steps = 1)
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
        //events for buttons
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteAllCards();
            if (!RuntimeData.Load())
            {
                Grid newCard = new Grid();
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
            //RuntimeData.Generate(Seznam,ProjectName);
            RuntimeData.Save();
        }

        private void AddNewCard(object sender, RoutedEventArgs e)
        {
            Grid card = new Grid();
            CreateCard(out card);
            //get card number which sent the event, so we can add new card just under it
            int cardIndex = Seznam.Children.IndexOf((Grid)((Button)sender).Parent);
            Seznam.Children.Insert(cardIndex + 1, card);
            RuntimeData.Generate(Seznam, ProjectName);
            //here we assign new date to card data
            Data data = RuntimeData.FindByID(card.GetHashCode(), RuntimeData.runtimeData.list);
            data.changeDate = DateTime.Now;
            //focus new card
            AddCardInfo((Grid)Seznam.Children[cardIndex + 1]);
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
                Seznam.Children.Remove((Grid)((Button)sender).Parent);
            }
            RuntimeData.Generate(Seznam,ProjectName);
            //avoid empty list
            if (Seznam.Children.Count == 1)
            {
                Grid newCard = new Grid();
                CreateCard(out newCard);
                Seznam.Children.Add(newCard);
                RuntimeData.Generate(Seznam, ProjectName);
            }
        }
        //moving up and down
        private void MoveLeftButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = Seznam.Children.IndexOf((UIElement)btn.Parent);
            Grid card = (Grid)Seznam.Children[index];
            MoveCardLeft(card);
            SortAllCards(Seznam);
        }
        private void MoveRightButton_Click(object sender, RoutedEventArgs e)
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
        private void ClickCardEvent(object sender, MouseButtonEventArgs e)
        {
            AddCardInfo((Grid)sender);
            //handling shift click to select card
            if (Keyboard.IsKeyDown(Key.LeftShift)) {
                Selection.ToggleSelecetion((Grid)sender);
                if (!Selection.GetSelection((Grid)sender))
                {
                    ((Grid)sender).SetResourceReference(BackgroundProperty, "CardColor");
                }
                else {
                    ((Grid)sender).SetResourceReference(BackgroundProperty, "AccentBackgroundBrush");
                }
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
                DeleteAllCards();
                if (!RuntimeData.Load())
                {
                    Grid novaKarta = new Grid();
                    CreateCard(out novaKarta);
                    Seznam.Children.Add(novaKarta);
                }
                else
                {
                    GenerateFromData(RuntimeData.runtimeData.list);
                    ProjectName.Text = RuntimeData.runtimeData.list.name;
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
        //events for details subwindow

        //reference to card
        public Grid Card;
        public Data attachedData;
        //Here we add content info from card
        public void AddCardInfo(Grid SourceCard)
        {
            Card = SourceCard;
            attachedData = RuntimeData.FindByID(Card.GetHashCode(), RuntimeData.runtimeData.list);
            nameBox.Text = ((TextBox)(Card.Children[0])).Text;
            //descBox.Text = ((TextBox)(Card.Children[0])).Text;
            LabelRect.Fill = new SolidColorBrush(LabelColorValues.barva[(int)LabelColorNumbers.GetColorNumber(Card.Children[3])]);
            LabelColorNumbers.SetColorNumber(LabelRect, LabelColorNumbers.GetColorNumber(Card.Children[3]));
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
            if (Card != null)
            {
                ((TextBox)(Card.Children[0])).Text = nameBox.Text;
            }
        }
        //set new label color in window and update card label
        private void LabelChange_Event(object sender, MouseButtonEventArgs e)
        {
            LabelColorNumbers.LabelColorChange(LabelRect, 1);
            LabelColorNumbers.LabelColorChange((Rectangle)(Card.Children[3]), 1);
            attachedData.labelColor = LabelColorNumbers.GetColorNumber(LabelRect);
        }

        private void DescChanged_Event(object sender, TextChangedEventArgs e)
        {
            //ensure the card is set
            if (Card != null && descBox.Text != null)
            {
                attachedData.description = descBox.Text;
            }
        }
        //resets focuses and selection
        private void ResetFocus(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).Focus();
            foreach (Grid card in Seznam.Children) {
                Selection.SetSelecetion((UIElement)sender, false);
            }
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
    }
}
