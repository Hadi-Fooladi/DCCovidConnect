using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DCCovidConnect.ViewModels
{
    [QueryProperty("Section", "section")]
    class InfoListViewModel : BaseViewModel
    {
        string section;
        public string Section
        {
            get => section;
            set => SetProperty(ref section, Uri.UnescapeDataString(value));
        }
    }
}
