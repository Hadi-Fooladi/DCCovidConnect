﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DCCovidConnect.Models
{
    public class CountyCasesItem
    {
        public DateTime Date { get; set; }
        public String County { get; set; }
        public String State { get; set; }
        public int FIPS { get; set; }
        public int Cases { get; set; }
        public int Deaths { get; set; }
        public int ConfirmedCases { get; set; }
        public int ConfirmedDeaths { get; set; }
        public int ProbableCases { get; set; }
        public int ProbableDeaths { get; set; }
    }
}
