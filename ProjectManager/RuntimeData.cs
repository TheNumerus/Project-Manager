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
using Newtonsoft.Json;

namespace ProjectManager
{
    public static class RuntimeData
    {
        //variable for loading data at runtime
        public static DatabaseInfo runtimeData = new DatabaseInfo();
        //variable for undo and redo
        public static List<Data> undoData = new List<Data>();
        public static int step = 0;

        //obsolete method for loading data
        public static bool LoadXML()
        {
            runtimeData = new DatabaseInfo();
            runtimeData.list = new Data();
            String path = Properties.Settings.Default.PathToFile;
            //load file
            FileStream databaze;
            try
            {
                databaze = File.Open(path, FileMode.Open);
            }
            catch (System.IO.FileNotFoundException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "File doesn't exist.";
                errorwindow.ErrorMessage.Text = "Error! File does not exist.";
                errorwindow.Show();
                runtimeData = new DatabaseInfo();
                runtimeData.list = new Data();
                return false;
            }
            catch (System.UnauthorizedAccessException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Insufficient rights";
                errorwindow.ErrorMessage.Text = "Error! App has insufficient rights to open this file. Please launch this app as admin.";
                errorwindow.Show();
                runtimeData = new DatabaseInfo();
                runtimeData.list = new Data();
                return false;
            }
            StreamReader sr = new StreamReader(databaze);
            XmlSerializer xs = new XmlSerializer(typeof(DatabaseInfo));
            //handle corrupted file
            try
            {
                runtimeData = (DatabaseInfo)xs.Deserialize(sr);
            }
            catch (System.InvalidOperationException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "XML Error";
                errorwindow.ErrorMessage.Text = "Error! This database file is corrupted.";
                errorwindow.Show();
                sr.Close();
                databaze.Close();
                runtimeData = new DatabaseInfo();
                runtimeData.list = new Data();
                return false;
            }
            catch (System.InvalidCastException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Database Error";
                errorwindow.ErrorMessage.Text = "Error! This database file is obsolete.";
                errorwindow.Show();
                sr.Close();
                databaze.Close();
                runtimeData = new DatabaseInfo();
                runtimeData.list = new Data();
                return false;
            }
            //handle empty file
            if (runtimeData == null)
            {
                runtimeData = new DatabaseInfo();
                runtimeData.list = new Data();
            }
            RecordUndo();
            sr.Close();
            databaze.Close();
            return true;
        }

        public static bool LoadJSON()
        {
            runtimeData = new DatabaseInfo();
            runtimeData.list = new Data();
            String path = Properties.Settings.Default.PathToFile;
            //load file
            String json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (System.IO.FileNotFoundException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "File doesn't exist.";
                errorwindow.ErrorMessage.Text = "Error! File does not exist.";
                errorwindow.Show();
                runtimeData = new DatabaseInfo();
                runtimeData.list = new Data();
                return false;
            }
            catch (System.UnauthorizedAccessException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Insufficient rights";
                errorwindow.ErrorMessage.Text = "Error! App has insufficient rights to open this file. Please launch this app as admin.";
                errorwindow.Show();
                runtimeData = new DatabaseInfo();
                runtimeData.list = new Data();
                return false;
            }
            try
            {
                runtimeData = JsonConvert.DeserializeObject<DatabaseInfo>(json);
            }
            catch(JsonReaderException) {
                if (LoadXML()) {
                    ErrorWindow errorwindow = new ErrorWindow();
                    errorwindow.Title = "JSON Error";
                    errorwindow.ErrorMessage.Text = "This database is being converted to newer format.";
                    errorwindow.Show();
                }
                else {
                    ErrorWindow errorWindow = new ErrorWindow();
                    errorWindow.Title = "JSON Error";
                    errorWindow.ErrorMessage.Text = "Error! This database file is corrupted.";
                    errorWindow.Show();
                    runtimeData = new DatabaseInfo();
                    runtimeData.list = new Data();
                    return false;
                }
                //handle empty file
                if (runtimeData == null)
                {
                    runtimeData = new DatabaseInfo();
                    runtimeData.list = new Data();
                }
                RecordUndo();
            }
            runtimeData.versionDB = "2";
            return true;
        }

        //method that generates data structure
        public static void Generate(StackPanel List, TextBox ProjectName) {
            //handle empty runtime data
            if (runtimeData == null) {
                runtimeData = new DatabaseInfo();
                runtimeData.list = new Data();
            }
            Data runtimeDataNew = new Data(ProjectName.Text, LabelColors.None, 0);
            //this variable is basicly a path reference
            Data PlaceToSave = runtimeDataNew;
            for (int i = 0; i < List.Children.Count; i++)
            {
                //this code doesn't work with only one card, so we handle it there
                Border cardAbove = new Border();
                if (i != 0)
                {
                    cardAbove = (Border)List.Children[i - 1];
                }
                //there start code for generating data structure
                else
                {
                    CardHierarchy.SetCardLevel(cardAbove, 1);
                }
                Border card = (Border)List.Children[i];
                TextBox cardName = (TextBox)((Grid)card.Child).Children[0];
                if (card.Visibility != Visibility.Collapsed)
                {
                    //if card above has bigger hierarchy level, we save that data there
                    if (CardHierarchy.GetCardLevel(cardAbove) < CardHierarchy.GetCardLevel(card))
                    {
                        PlaceToSave = PlaceToSave.cards[PlaceToSave.cards.Count - 1];
                    }
                    //if card above has lower hierarchy level, we save that card into parent of card, with the same hierarchy
                    else if (CardHierarchy.GetCardLevel(cardAbove) > CardHierarchy.GetCardLevel(card))
                    {
                        //but for that, we need to search all lists to find a parent
                        PlaceToSave = FindDataParent(runtimeDataNew, FindDataByHierarchy(runtimeDataNew, CardHierarchy.GetCardLevel(card)));
                    }
                    //if card above has same hierarchy level, we save it in same location
                    Data DataToAdd = new Data(cardName.Text, LabelColorNumbers.GetColorNumber((Rectangle)((Grid)card.Child).Children[2]), CardHierarchy.GetCardLevel(card), newID: card.GetHashCode());
                    PlaceToSave.cards.Add(DataToAdd);
                    //if there was a data object with description, we add that description back
                    if (FindByID(card.GetHashCode(), runtimeData.list) != null)
                    {
                        DataToAdd.description = FindByID(card.GetHashCode(), runtimeData.list).description;
                        DataToAdd.changeDate = FindByID(card.GetHashCode(), runtimeData.list).changeDate;
                    }
                    else {
                        DataToAdd.description = "Edit description...";
                        DataToAdd.changeDate = DateTime.Now;
                    }
                }
            }
            //here we replace old data with new data
            runtimeData.list = runtimeDataNew;
        }
        public static void SaveJSON()
        {
            String path = Properties.Settings.Default.PathToFile;
            string json;
            json = JsonConvert.SerializeObject(runtimeData);
            File.WriteAllText(path, json);
        }

        //recursive searching in data structure by element in list
        public static Data FindDataParent(Data root, Data ChildOfElement)
        {
            Data foundElement = null;
            foreach (Data d in root.cards)
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
        //recursive searching in data structure for last element with specific hierarchy number
        public static Data FindDataByHierarchy(Data root,int hierarchyNumber)
        {
            Data foundElement = null;
            foreach (Data d in root.cards)
            {
                if (d.level == hierarchyNumber)
                {
                    foundElement = d;
                }
                else
                {
                    foundElement = FindDataByHierarchy(d,hierarchyNumber);
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
                //handle no childs
                if (root.cards != null) {
                    foreach (Data d in root.cards)
                    {
                        foundElement = FindByID(ID, d);
                        if (foundElement != null) {
                            break;
                        }
                    }
                }
            }
            return foundElement;
        }
        //methods for undo and redo
        public static void RecordUndo() {
            step++;
            Data copy = runtimeData.list.Copy();
            if (step >= undoData.Count - 1)
            {
                undoData.Add(copy);
            }
            else
            {
                undoData.Insert(step, copy);
            }
        }
        public static void Undo() {
            if (step > 0)
            {
                step--;
                runtimeData.list = undoData[step];
            }

        }
        public static void ResetUndo() {
            undoData = new List<Data>() {runtimeData.list.Copy()};
            step = 0;
        }
        public static void Redo() {
            //handle bigger index out of range
            if (step+1 <= undoData.Count-1) {
                step++;
                runtimeData.list = undoData[step];
            }
        }
    }
}
