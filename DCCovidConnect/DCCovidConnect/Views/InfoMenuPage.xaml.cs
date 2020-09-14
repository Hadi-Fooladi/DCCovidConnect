using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace DCCovidConnect.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InfoMenuPage : ContentPage
    {
        public InfoMenuPage()
        {
            InitializeComponent();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Height <= 0.5625)
            {
                ThirdRow.Height = new GridLength(20, GridUnitType.Star);
                PageLayout.WidthRequest = 3.0 / 4 * InfoMenu.Height;
            }
            else
            {
                PageLayout.WidthRequest = 3.0 / 5 * InfoMenu.Height;
            }
        }
    }
}