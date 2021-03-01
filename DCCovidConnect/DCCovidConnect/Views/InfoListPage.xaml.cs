using DCCovidConnect.Models;
using DCCovidConnect.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCCovidConnect.Views
{
    [QueryProperty("Section", "section")]
    /// <summary>
    /// This page displays the sublist of a section.
    /// </summary>
    public partial class InfoListPage : ContentPage
    {
        InfoItem.InfoType section;
        ObservableCollection<InfoItem> items;
        static TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        public string Section
        {
            get => textInfo.ToTitleCase(section.ToString().Replace('_', ' ').ToLower());
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
            foreach (InfoItem item in infoItems)
            {
                item.Title = item.Title.Substring(item.Title.IndexOf('-') + 1).Trim();
            }
            Items = new ObservableCollection<InfoItem>(infoItems);
        }
    }
}