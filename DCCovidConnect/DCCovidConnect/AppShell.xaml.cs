using DCCovidConnect.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCCovidConnect
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(InfoListPage), typeof(InfoListPage));
            Routing.RegisterRoute(nameof(InfoDetailPage), typeof(InfoDetailPage));
        }
    }
}