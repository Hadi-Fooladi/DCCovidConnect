using Xamarin.Forms;

namespace DCCovidConnect.Views
{
    public partial class WebViewPage
    {
        public WebViewPage()
        {
            InitializeComponent();
        }

        public void SetHtmlBody(string body)
            => WV.Source = new HtmlWebViewSource
            {
                BaseUrl = DependencyService.Get<IWebViewBaseUrl>().BaseUrl,
                Html = $"<html><head><link rel='stylesheet' type='text/css' href='Main.css'></head><body>{body}</body></html>"
            };
    }
}
