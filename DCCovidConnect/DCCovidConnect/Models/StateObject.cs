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
    }
}
