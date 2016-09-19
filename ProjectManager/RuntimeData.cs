using ProjectManager.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
        /// This means more events beeing added and expanded and also regenerating data structure after each hierarchical edit.
        /// </summary>
        /*
        public static void Save() {
            String cesta = Application.Current.FindResource("PathToFile").ToString();
            FileStream databaze;
            //osetreni chybejicim pravum k zapisu souboru
            try
            {
                databaze = File.Open(cesta, FileMode.Create);
            }
            catch (System.UnauthorizedAccessException)
            {
                ErrorWindow errorwindow = new ErrorWindow();
                errorwindow.Title = "Nedostatečná práva";
                errorwindow.ErrorMessage.Text = "Chyba! Program nemá potřebná práva pro uložení souboru.";
                errorwindow.Show();
                return;
            }
            StreamWriter sw = new StreamWriter(databaze);
            XmlSerializer xs = new XmlSerializer(typeof(Data));
            Data data = new Data(ProjectName.Text, LabelColors.None, 0);
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
                else
                {
                    MistoKartyVHierarchii.SetUrovenKarty(kartaNad, 1);
                }
                Grid karta = (Grid)Seznam.Children[i];
                TextBox nazevKarty = (TextBox)karta.Children[0];
                if (karta.Visibility != Visibility.Collapsed)
                {
                    //pokud je misto karty v hierarchii mensi nez karty nad ni, ulozi se karta do "deti" predchozi karty
                    if (MistoKartyVHierarchii.GetUrovenKarty((Grid)Seznam.Children[i - 1]) < MistoKartyVHierarchii.GetUrovenKarty((Grid)Seznam.Children[i]))
                    {
                        mistoNaUlozeni = mistoNaUlozeni.Karty[mistoNaUlozeni.Karty.Count - 1];
                    }
                    //pokud je misto karty v hierarchii vetsi nez karty nad ni, ulozi se karta o uroven vys
                    else if (MistoKartyVHierarchii.GetUrovenKarty((Grid)Seznam.Children[i - 1]) > MistoKartyVHierarchii.GetUrovenKarty((Grid)Seznam.Children[i]))
                    {
                        //vyhledani mista pro ulozeni
                        mistoNaUlozeni = nalezeniMistaProUlozeni(data, mistoNaUlozeni);
                    }
                    //pokud je misto karty v hierarchii stejne jako v karte nad ni, ulozi se karta na stejne misto
                    mistoNaUlozeni.Karty.Add(new Data(nazevKarty.Text, LabelColorNumbers.GetColorNumber((Rectangle)karta.Children[3]), MistoKartyVHierarchii.GetUrovenKarty(karta)));
                }
            }
            xs.Serialize(sw, data);
            sw.Close();
            databaze.Close();         
        }
        */
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
