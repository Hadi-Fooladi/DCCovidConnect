using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace DCCovidConnect.Models
{
    public class StateObject
    {
        public string State { get; set; }
        public string StateAbbrev { get; set; }
        public SKPath Path { get; set; }
        public StateCasesItem CasesItem { get; set; }
        public Dictionary<int, CountyObject> Counties { get; set; } = new Dictionary<int, CountyObject>(); // FIPS is the key value
        public int MaxCountyCases { get; set; } = int.MinValue;
        public int MinCountyCases { get; set; } = int.MaxValue;
    }
}
