using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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

            PageLayout.WidthRequest = 3.0 / 5 * InfoMenu.Height;
        }
    }
}