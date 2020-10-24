using DCCovidConnect.Data;
using System;
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

        protected override async void OnStart()
        {
            await Database.SaveInfoListAsync(new Models.InfoList
            {
                Title = "News",
                ItemsString = "[\"test1\",\"test2\",\"test3\"]",
            });
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
