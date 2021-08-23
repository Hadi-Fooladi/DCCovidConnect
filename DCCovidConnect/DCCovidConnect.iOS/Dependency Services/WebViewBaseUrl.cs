using Foundation;
using Xamarin.Forms;

[assembly: Dependency(typeof(DCCovidConnect.iOS.WebViewBaseUrl))]

namespace DCCovidConnect.iOS
{
    internal class WebViewBaseUrl : IWebViewBaseUrl
    {
        public string BaseUrl => NSBundle.MainBundle.BundlePath;
    }
}
