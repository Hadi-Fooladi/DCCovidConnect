using DCCovidConnect.Data;
using DCCovidConnect.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCCovidConnect
{
    public partial class App : Application
    {
        static InfoDatabase database;
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        public static InfoDatabase Database
        {
            get
            {
                if (database == null)
                {
                    database = new InfoDatabase();
                }
                return database;
            }
        }

        protected async override void OnStart()
        {
            await Task.Run(() => MainThread.BeginInvokeOnMainThread(async () =>
            {
                await App.Database.UpdateDatabase();
            }));
            SetDarkMode(Settings.Current.DarkMode.Equals("On"));
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public static void SetDarkMode(bool enabled)
        {
            if (enabled)
            {
                App.Current.Resources["Black"] = Color.FromHex("#FFFFFF");
                App.Current.Resources["White"] = Color.FromHex("#000000");
            }
            else
            {
                App.Current.Resources["White"] = Color.FromHex("#FFFFFF");
                App.Current.Resources["Black"] = Color.FromHex("#000000");
            }
        }
    }
}
