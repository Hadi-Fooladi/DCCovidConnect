using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace DCCovidConnect.Models
{
    public class InfoList
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Title { get; set; }
        public string ItemsString { get; set; }
        public bool Leaf { get; set; }
    }
}
