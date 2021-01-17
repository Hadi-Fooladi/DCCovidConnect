using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace DCCovidConnect.Models
{
    public class InfoItem
    {
        public enum InfoType { NONE, AGE_SPECIFIC, COMMUNITY, FAQ, GENERAL_INFORMATION, GETTING_INVOLVED, PREGNANCY, NEWS, RESEARCH, SELF_CARE, SERVICES }
        [PrimaryKey]
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public InfoType Type { get; set; }
    }
}
