using Xamarin.Forms;

[assembly: Dependency(typeof(DCCovidConnect.Droid.WebViewBaseUrl))]

namespace DCCovidConnect.Droid
{
    internal class WebViewBaseUrl : IWebViewBaseUrl
    {
        public string BaseUrl => "file:///android_asset/";
    }
}
