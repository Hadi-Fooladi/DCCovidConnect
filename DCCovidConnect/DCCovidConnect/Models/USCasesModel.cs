using System;
using System.Collections.Generic;
using System.Text;

namespace DCCovidConnect.Models
{
    public class USCasesModel
    {
        public DateTime Date { get; set; }
        public int Cases { get; set; }
        public int Deaths { get; set; }
        public int ConfirmedCases { get; set; }
        public int ConfirmedDeaths { get; set; }
        public int ProbableCases { get; set; }
        public int ProbableDeaths { get; set; }
    }
}
