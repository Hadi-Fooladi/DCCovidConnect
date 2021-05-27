using DCCovidConnect.Data;
using DCCovidConnect.Services;
using DCCovidConnect.Themes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: ExportFont("Font Awesome 5 Free-Regular-400.otf", Alias = "FontAwesomeRegular")]
[assembly: ExportFont("Font Awesome 5 Free-Solid-900.otf", Alias = "FontAwesomeSolid")]


namespace DCCovidConnect
{
    public partial class App : Application
    {
        static InfoDatabase database;
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            ToggleTheme();
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
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public static void ToggleTheme()
        {
            ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();

                switch (Settings.Current.DarkMode)
                {
                    case "On":
                        mergedDictionaries.Add(new DarkTheme());
                        break;
                    case "Off":
                        mergedDictionaries.Add(new LightTheme());
                        break;
                    default:
                        OSAppTheme currentTheme = Application.Current.RequestedTheme;
                        if (currentTheme.ToString().Equals("Dark"))
                        {
                            mergedDictionaries.Add(new DarkTheme());
                        } else
                        {
                            mergedDictionaries.Add(new LightTheme());
                        }
                        break;
                }
            }
        }
    }
}
