using System;
using System.Collections.Generic;
using System.Text;

namespace DCCovidConnect.ViewModels
{
    class HomeViewModel : BaseViewModel
    {
        string region = "Virginia";
        string local = "Fairfax County";
        int regionCases = 123668;
        int localCases = 830;

        int phase = 3;
        public string Region
        {
            get => region;
            set => SetProperty(ref region, value);
        }
        public string Local
        {
            get => local;
            set => SetProperty(ref local, value);
        }
        public int RegionCases
        {
            get => regionCases;
            set => SetProperty(ref regionCases, value);
        }
        public int LocalCases
        {
            get => localCases;
            set => SetProperty(ref localCases, value);
        }

        public int Phase
        {
            get => phase;
            set => SetProperty(ref phase, value);
        }
    }
}
