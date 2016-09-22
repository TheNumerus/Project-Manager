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
        public Data(string novyNazev = "", LabelColors novyLabelColor = LabelColors.None, int novaPozice = 0, string desc = "", DateTime date = new DateTime(),int newID = 0)
        {
            nazev = novyNazev;
            Karty = new List<Data>();
            labelColor = novyLabelColor;
            pozice = novaPozice;
            description = desc;
            changeDate = date;
            GridID = newID;
        }
        //this construstor is here, so we can serialize this object
        public Data()
        {

        }
    }
}
