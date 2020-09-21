using DCCovidConnect.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCCovidConnect.Views
{
    public partial class InfoListPage : ContentPage
    {
        public InfoListPage()
        {
            InitializeComponent();
            BindingContext = new InfoListViewModel();
        }
    }
}