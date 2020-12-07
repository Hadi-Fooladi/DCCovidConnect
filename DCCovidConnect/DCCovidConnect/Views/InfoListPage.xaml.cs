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
        InfoItem.InfoType section;
        ObservableCollection<string> items;
        public string Section
        {
            get => section.ToString();
            set
            {
                section = (InfoItem.InfoType)Enum.Parse(typeof(InfoItem.InfoType), Uri.UnescapeDataString(value), true);
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
        async void GoToDetailPage(object sender, EventArgs args)
        {
            await Shell.Current.GoToAsync($"{nameof(InfoDetailPage)}?title={(args as TappedEventArgs).Parameter}");
        }
        public InfoListPage()
        {
            InitializeComponent();
            BindingContext = this;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            List<InfoItem> infoItems = await App.Database.GetInfoItemsAsync(section);
            if (infoItems == null)
                Items = new ObservableCollection<string>(new string[] { section.ToString(), "hello" });
            else
                Items = new ObservableCollection<string>(infoItems.Select(i => i.Title));
        }
    }
}