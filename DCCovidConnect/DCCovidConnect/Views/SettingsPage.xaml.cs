using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCovidConnect.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCCovidConnect.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            _statePicker.ItemsSource = MapService.Service.States.Keys.ToList();
            _statePicker.SelectedItem = Settings.DefaultState;
            _statePicker.SelectedIndexChanged += (sender, args) =>
            {
                Settings.DefaultState = _statePicker.SelectedItem.ToString();
            };

            _darkModePicker.SelectedItem = Settings.DarkMode;
            _darkModePicker.SelectedIndexChanged += (sender, args) =>
            {
                Settings.DarkMode = _darkModePicker.SelectedItem.ToString();
            };
        }
    }
}