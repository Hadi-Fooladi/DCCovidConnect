using DCCovidConnect.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCCovidConnect
{
    public partial class AppShell : Shell
    {
        public ICommand GoToSettings => new Command(async () => await NavigateToSettings());
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
            BindingContext = this;
        }

        void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(InfoListPage), typeof(InfoListPage));
            Routing.RegisterRoute(nameof(InfoDetailPage), typeof(InfoDetailPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }

        async Task NavigateToSettings()
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
            Shell.Current.FlyoutIsPresented = false;
        }
    }
}