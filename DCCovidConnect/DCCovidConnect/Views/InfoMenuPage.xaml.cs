using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Windows.Input;
using DCCovidConnect.Models;

namespace DCCovidConnect.Views
{
    public partial class InfoMenuPage : ContentPage
    {
        public InfoMenuPage()
        {
            InitializeComponent();
            NavigateCommand = new Command<InfoItem.InfoType>(async (section) =>
            {
                foreach (Frame elem in _infoMenu.Children.OfType<Frame>())
                {
                    (elem.Children[0] as Button).IsEnabled = false;
                }
                await Shell.Current.GoToAsync($"{nameof(InfoListPage)}?section={section}");
                foreach (Frame elem in _infoMenu.Children.OfType<Frame>())
                {
                    (elem.Children[0] as Button).IsEnabled = true;
                }
            });

            BindingContext = this;
            foreach (Frame elem in _infoMenu.Children.OfType<Frame>())
            {
                (elem.Children[0] as Button).Command = NavigateCommand;
            }
//#if DEBUG
//            pageLayout.Children.Add(new Label { Text = Constants.DatabasePath });
//#endif
        }

        public ICommand NavigateCommand { get; private set; }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Height <= 0.5625)
            {
                _thirdRow.Height = new GridLength(1, GridUnitType.Star);
                _infoMenu.WidthRequest = 3.0 / 4 * _infoMenu.Height;
            }
            else
            {
                _infoMenu.WidthRequest = 3.0 / 5 * _infoMenu.Height;
            }
            //Thickness margin = _headerBackground.Margin;
            //margin.Right = _pageLayout.Width - _header.Width;
            //_headerBackground.Margin = margin;

            Thickness padding = _header.Padding;
            padding.Left = (_pageLayout.Width - _infoMenu.WidthRequest) / 2;
            _header.Padding = padding;
            //_headerBackground.BackgroundColor = _headerBackground.BackgroundColor.WithLuminosity(0.7);
        }
    }
}