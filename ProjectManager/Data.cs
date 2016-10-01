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
        public string name;
        public List<Data> cards;
        public LabelColors labelColor;
        public int level;
        public string description;
        public DateTime changeDate;
        //not save following variable
        [XmlIgnore]
        public int GridID;
        public Data(string newName = "", LabelColors newLabelColor = LabelColors.None, int newLevel = 0, string desc = "", DateTime date = new DateTime(),int newID = 0)
        {
            name = newName;
            cards = new List<Data>();
            labelColor = newLabelColor;
            level = newLevel;
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
