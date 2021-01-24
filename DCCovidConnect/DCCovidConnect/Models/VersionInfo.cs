using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DCCovidConnect.Models
{
    public class VersionInfo
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string US_CASES_SHA { get; set; }
        public string STATE_CASES_SHA { get; set; }
        public string COUNTY_CASES_SHA { get; set; }
    }
}
