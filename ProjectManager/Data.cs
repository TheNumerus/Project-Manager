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
        private static ID;
        public string nazev;
        public List<Data> Karty;
        public LabelColors labelColor;
        public int pozice;
        public string description;
        public DateTime changeDate;
        //not save following variable
        [XmlIgnore]
        public int GridID;
        public Data(string novyNazev = "", LabelColors novyLabelColor = LabelColors.None, int novaPozice = 0, string desc = "", DateTime date = DateTime.Now, int newID = Data.ID)
        {
            nazev = novyNazev;
            Karty = new List<Data>();
            labelColor = novyLabelColor;
            pozice = novaPozice;
            description = desc;
            changeDate = date;
            
            /* Každý nový objekt dostane nové ID. Nevím, jestli to je potřeba, radši to zkontroluj a přepiš. */
            
            GridID = newID;
            ++(Data.ID);
        }
        /* Tohle už je nejspíš zbytečné, ale jestli to budeš potřebovat, odstraň tento comment.
        public Data()
        {
            Karty = new List<Data>();
        }
        */
    }
}
