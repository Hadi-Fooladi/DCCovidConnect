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
            _statePicker.SelectedItem = Settings.Current.DefaultState;
            _statePicker.SelectedIndexChanged += (sender, args) =>
            {
                Settings.Current.DefaultState = _statePicker.SelectedItem.ToString();
            };
            _darkModePicker.SelectedItem = Settings.Current.DarkMode;
            _darkModePicker.SelectedIndexChanged += (sender, args) =>
            {
                Settings.Current.DarkMode = _darkModePicker.SelectedItem.ToString();
                App.SetDarkMode(Settings.Current.DarkMode.Equals("On"));
            };
        }
    }
}