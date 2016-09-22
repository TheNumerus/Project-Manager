using ProjectManager.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;

namespace ProjectManager
{
    public static class RuntimeData
    {
        //variable for loading data at runtime
        public static Data runtimeData;

        //load data to variable
        public static bool Load()
        {
            String cesta = Application.Current.FindResource("PathToFile").ToString();
            //load file
            FileStream databaze;
            try
            {
                databaze = File.Open(cesta, FileMode.Open);
            }
            catch (System.IO.FileNotFoundException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Soubor neexistuje";
                errorwindow.ErrorMessage.Text = "Chyba! Soubor neexistuje.";
                errorwindow.Show();
                runtimeData = new Data();
                return false;
            }
            catch (System.UnauthorizedAccessException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Nedostatečná práva";
                errorwindow.ErrorMessage.Text = "Chyba! Program nemá potřebná práva pro načtení souboru.";
                errorwindow.Show();
                runtimeData = new Data();
                return false;
            }
            StreamReader sr = new StreamReader(databaze);
            XmlSerializer xs = new XmlSerializer(typeof(Data));
            //handle corrupted file
            try
            {
                runtimeData = (Data)xs.Deserialize(sr);
            }
            catch (System.InvalidOperationException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "XML Error";
                errorwindow.ErrorMessage.Text = "Chyba! Soubor s databází je poškozen.";
                errorwindow.Show();
                sr.Close();
                databaze.Close();
                runtimeData = new Data();
                return false;
            }
            sr.Close();
            databaze.Close();
            return true;
        }
        //method that generates data structure
        public static void Generate(StackPanel List, TextBox ProjectName) {
            Data runtimeDataNew = new Data(ProjectName.Text, LabelColors.None, 0);
            //this variable is basicly a path reference
            Data PlaceToSave = runtimeDataNew;
            for (int i = 0; i < List.Children.Count; i++)
            {
                //this code doesn't work with only one card, so we handle it there
                Grid cardAbove = new Grid();
                if (i != 0)
                {
                    cardAbove = (Grid)List.Children[i - 1];
                }
                //there start code for generating data structure
                else
                {
                    CardHierarchy.SetCardLevel(cardAbove, 1);
                }
                Grid card = (Grid)List.Children[i];
                TextBox cardName = (TextBox)card.Children[0];
                if (card.Visibility != Visibility.Collapsed)
                {
                    //if card above has bigger hierarchy level, we save that data there
                    if (CardHierarchy.GetCardLevel((Grid)List.Children[i - 1]) < CardHierarchy.GetCardLevel((Grid)List.Children[i]))
                    {
                        PlaceToSave = PlaceToSave.Karty[PlaceToSave.Karty.Count - 1];
                    }
                    //if card above has lower hierarchy level, we save that data into parent of this card
                    else if (CardHierarchy.GetCardLevel((Grid)List.Children[i - 1]) > CardHierarchy.GetCardLevel((Grid)List.Children[i]))
                    {
                        //but for that, we need to search all lists to find a parent
                        PlaceToSave = FindDataParent(runtimeDataNew, PlaceToSave);
                    }
                    //if card above has same hierarchy level, we save it in same location
                    Data DataToAdd = new Data(cardName.Text, LabelColorNumbers.GetColorNumber((Rectangle)card.Children[3]), CardHierarchy.GetCardLevel(card), newID: card.GetHashCode());
                    PlaceToSave.Karty.Add(DataToAdd);
                    //if there was a data object with description, we add that description back
                    if (FindByID(card.GetHashCode(), runtimeData) != null) {
                        DataToAdd.description = FindByID(card.GetHashCode(), runtimeData).description;
                    }
                }
            }
            //here we replace old data with new data
            runtimeData = runtimeDataNew;
        }
        public static void Save() {
            String path = Application.Current.FindResource("PathToFile").ToString();
            FileStream database;
            //handle missing rigths to save file
            try
            {
                database = File.Open(path, FileMode.Create);
            }
            catch (System.UnauthorizedAccessException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Insufficent rights";
                errorwindow.ErrorMessage.Text = "Error! Program has insufficent rights to save database.";
                errorwindow.Show();
                return;
            }
            StreamWriter sw = new StreamWriter(database);
            XmlSerializer xs = new XmlSerializer(typeof(Data));
            xs.Serialize(sw, runtimeData);
            sw.Close();
            database.Close();
        }
        //recursive searching in data structure by element in list
        public static Data FindDataParent(Data root, Data ChildOfElement)
        {
            Data foundElement = null;
            foreach (Data d in root.Karty)
            {
                if (d.Equals(ChildOfElement))
                {
                    foundElement = root;
                    break;
                }
                else
                {
                    foundElement = FindDataParent(d, ChildOfElement);
                    if (foundElement != null)
                    {
                        break;
                    }
                }
            }
            return foundElement;
        }
        //find data by id
        public static Data FindByID(int ID,Data root) {
            Data foundElement = null;
            if (ID == root.GridID)
            {
                foundElement = root;
            }
            else
            {
                foreach (Data d in root.Karty)
                {
                    foundElement = FindByID(ID, d);
                    if (foundElement != null) {
                        break;
                    }
                }
            }
            return foundElement;
        }
    }
}
