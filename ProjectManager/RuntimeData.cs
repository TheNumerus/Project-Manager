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
                return false;
            }
            catch (System.UnauthorizedAccessException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Nedostatečná práva";
                errorwindow.ErrorMessage.Text = "Chyba! Program nemá potřebná práva pro načtení souboru.";
                errorwindow.Show();
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
                return false;
            }
            sr.Close();
            databaze.Close();
            return true;
        }
        /// <summary>
        /// Right now (19.09.2016), following method is severely broken, because its copied from "MainWindow" code file.
        /// Since I want to edit card description and other things in a seperate window, I must save that data somewhere else than in "MainWindow" code file.
        /// Although so far all info was stored directly in the cards themselves, description and other info cannot be stored there.
        /// Ideal candidate is the "Data" structure used already for load and save operations. But this has one major flaw. I must edit that structure at runtime.
        /// This means more events beeing added and expanded and also regenerating data structure after each hierarchical edit. <para />
        /// Update 1 (20.09.2016)
        /// So I decided to make this the hard way because it seems future-proof. Editing at runtime should be easy once I find a way to bind data object to card.
        /// No new functions yet, but now it works almost the same way.
        /// </summary>

        public static void Generate(StackPanel List, TextBox ProjectName) {
            runtimeData = new Data(ProjectName.Text, LabelColors.None, 0);
            //this variable is basicly a path reference
            Data PlaceToSave = runtimeData;
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
                        PlaceToSave = FindPlaceToSave(runtimeData, PlaceToSave);
                    }
                    //if card above has same hierarchy level, we save it in same location
                    PlaceToSave.Karty.Add(new Data(cardName.Text, LabelColorNumbers.GetColorNumber((Rectangle)card.Children[3]), CardHierarchy.GetCardLevel(card)));
                }
            }     
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
                errorwindow.Title = "Unsufficent rights";
                errorwindow.ErrorMessage.Text = "Error! Program has unsufficent rights to save database.";
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
        private static Data FindPlaceToSave(Data root, Data ChildOfElement)
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
                    foundElement = FindPlaceToSave(d, ChildOfElement);
                }
            }
            return foundElement;
        }
    }
}
