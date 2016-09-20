using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectManager.Helpers;
using System.Xml.Serialization;

namespace ProjectManager
{
    public class Data
    {
        public string nazev;
        public List<Data> Karty;
        public LabelColors labelColor;
        public int pozice;
        public string description;
        public DateTime changeDate;
        //not save following variable
        [XmlIgnore]
        public int GridID;
        public Data(string newNazev,LabelColors newLabelColor, int newPozice, string desc,DateTime date)
        {
            nazev = newNazev;
            Karty = new List<Data>();
            labelColor = newLabelColor;
            pozice = newPozice;
            description = desc;
            changeDate = date;
        }
        public Data(string newNazev, LabelColors newLabelColor, int newPozice, string desc)
        {
            nazev = newNazev;
            Karty = new List<Data>();
            labelColor = newLabelColor;
            pozice = newPozice;
        }
        public Data(string newNazev, LabelColors newLabelColor, int newPozice, DateTime date)
        {
            nazev = newNazev;
            Karty = new List<Data>();
            labelColor = newLabelColor;
            pozice = newPozice;
            changeDate = date;
        }
        public Data(string newNazev, LabelColors newLabelColor, int newPozice)
        {
            nazev = newNazev;
            Karty = new List<Data>();
            labelColor = newLabelColor;
            pozice = newPozice;
        }
        public Data(string newNazev, LabelColors newLabelColor, int newPozice, string desc, DateTime date, int newID)
        {
            nazev = newNazev;
            Karty = new List<Data>();
            labelColor = newLabelColor;
            pozice = newPozice;
            description = desc;
            changeDate = date;
            GridID = newID;
        }
        public Data(string newNazev, LabelColors newLabelColor, int newPozice, string desc, int newID)
        {
            nazev = newNazev;
            Karty = new List<Data>();
            labelColor = newLabelColor;
            pozice = newPozice;
            GridID = newID;
        }
        public Data(string newNazev, LabelColors newLabelColor, int newPozice, DateTime date, int newID)
        {
            nazev = newNazev;
            Karty = new List<Data>();
            labelColor = newLabelColor;
            pozice = newPozice;
            changeDate = date;
            GridID = newID;
        }
        public Data(string newNazev, LabelColors newLabelColor, int newPozice, int newID)
        {
            nazev = newNazev;
            Karty = new List<Data>();
            labelColor = newLabelColor;
            pozice = newPozice;
            GridID = newID;
        }
        public Data()
        {
            Karty = new List<Data>();
        }
    }
}
