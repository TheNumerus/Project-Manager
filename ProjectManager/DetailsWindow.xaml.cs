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
    /// Interaction logic for DetailsWindow.xaml
    /// </summary>
    public partial class DetailsWindow : Window
    {
        //reference to card
        public Grid Card;
        public Data attachedData;
        public DetailsWindow()
        {
            InitializeComponent();
        }
        //Here we add content info from card
        public void AddCardInfo(Grid SourceCard) {
            Card = SourceCard;
            attachedData = RuntimeData.FindByID(Card.GetHashCode(), RuntimeData.runtimeData);
            nameBox.Text = ((TextBox)(Card.Children[0])).Text;
            //descBox.Text = ((TextBox)(Card.Children[0])).Text;
            LabelRect.Fill = new SolidColorBrush(LabelColorValues.barva[(int)LabelColorNumbers.GetColorNumber(Card.Children[3])]);
            LabelColorNumbers.SetColorNumber(LabelRect,LabelColorNumbers.GetColorNumber(Card.Children[3]));
            descBox.Text = attachedData.description;
            //handle empty date
            if(attachedData.changeDate.Year != 1){
                dateBlock.Text = attachedData.changeDate.Date.ToLongDateString();
            }else{
                dateBlock.Visibility = Visibility.Hidden;
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
            LabelColorNumbers.LabelColorChange(LabelRect,1);
            LabelColorNumbers.LabelColorChange((Rectangle)(Card.Children[3]),1);
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
    }
}
