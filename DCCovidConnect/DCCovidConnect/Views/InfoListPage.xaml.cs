using DCCovidConnect.Models;
using DCCovidConnect.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCCovidConnect.Views
{
    [QueryProperty("Section", "section")]
    public partial class InfoListPage : ContentPage
    {
        string section;
        ObservableCollection<string> items;
        public string Section
        {
            get => section;
            set
            {
                section = Uri.UnescapeDataString(value);
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> Items
        {
            get => items; 
            set
            {
                items = value;
                OnPropertyChanged();
            }
        }

        public InfoListPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            InfoList infoList = await App.Database.GetInfoListAsync(section);
            if (infoList == null)
                Items = new ObservableCollection<string>(new string[] { section, "hello" });
            else
                Items = JsonConvert.DeserializeObject<ObservableCollection<string>>(infoList.ItemsString);
        }
    }
}