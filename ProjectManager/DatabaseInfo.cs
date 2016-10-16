using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager
{
    public class DatabaseInfo
    {
        public string name;
        public string versionDB;
        public string versionApp;
        public Data list;

        public DatabaseInfo(string newName = "New name") {
            name = newName;
            versionApp = Properties.Settings.Default.VersionNumber;
            versionDB = "2";
            list = new Data();
        }
        public DatabaseInfo() {
            versionApp = Properties.Settings.Default.VersionNumber;
            versionDB = "2";
            list = new Data();
        }
    }
}
