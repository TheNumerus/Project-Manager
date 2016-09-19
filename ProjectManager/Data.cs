using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectManager.Helpers;

namespace ProjectManager
{
    public class Data
    {
        public string nazev;
        public List<Data> Karty;
        public LabelColors labelColor;
        public int pozice;
        public string Description;
        public DateTime changeDate;
        public Data(string newNazev,LabelColors newLabelColor, int newPozice, string desc,DateTime date)
        {
            nazev = newNazev;
            Karty = new List<Data>();
            labelColor = newLabelColor;
            pozice = newPozice;
            Description = desc;
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
        public Data()
        {
            Karty = new List<Data>();
        }
    }
}
