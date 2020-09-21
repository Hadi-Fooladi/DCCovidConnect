using DCCovidConnect.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DCCovidConnect.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            BindingContext = new HomeViewModel();

            var color = baseLayout.BackgroundColor;
            baseLayout.BackgroundColor = color.WithLuminosity(color.Luminosity * 1.3);
        }
    }
}
