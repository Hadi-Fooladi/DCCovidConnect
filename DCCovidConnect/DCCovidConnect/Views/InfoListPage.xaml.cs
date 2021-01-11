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
        ObservableCollection<InfoItem> items;
        public string Section
        {
            get => section.ToString();
            set
            {
                section = (InfoItem.InfoType)Enum.Parse(typeof(InfoItem.InfoType), Uri.UnescapeDataString(value), true);
                OnPropertyChanged();
            }
        }
        public ObservableCollection<InfoItem> Items
        {
            get => items;
            set
            {
                items = value;
                OnPropertyChanged();
            }
        }

        private bool _userTapped;
        async void GoToDetailPage(object sender, EventArgs args)
        {
            if (_userTapped) 
                return;
            _userTapped = true;
            await Shell.Current.GoToAsync($"{nameof(InfoDetailPage)}?id={(args as TappedEventArgs).Parameter}");
            _userTapped = false;
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
            Items = new ObservableCollection<InfoItem>(infoItems);
        }
    }
}