using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Windows.Input;

namespace DCCovidConnect.Views
{
    public partial class InfoMenuPage : ContentPage
    {
        public InfoMenuPage()
        {
            InitializeComponent();

            NavigateCommand = new Command<String>(async (section) =>
            {
                await Shell.Current.GoToAsync($"{nameof(InfoListPage)}?section={section}");
            });

            BindingContext = this;
            foreach (Button elem in infoMenu.Children.OfType<Button>())
            {
                elem.Command = NavigateCommand;
            }
        }

        public ICommand NavigateCommand { get; private set; }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Height <= 0.5625)
            {
                thirdRow.Height = new GridLength(1, GridUnitType.Star);
                pageLayout.WidthRequest = 3.0 / 4 * infoMenu.Height;
            }
            else
            {
                pageLayout.WidthRequest = 3.0 / 5 * infoMenu.Height;
            }
        }
    }
}