using System.Text;

namespace DCCovidConnect.Views
{
    internal class AllAboutPage : WebViewPage
    {
        private static readonly string BODY;

        static AllAboutPage()
        {
            var sb = new StringBuilder();

            add("About DC Covid Connect App", AboutPage.BODY);
            add("Disclaimer", DisclaimerPage.BODY);
            add("Acknowledgments", AcknowledgmentsPage.Body);

            BODY = sb.ToString();

            void add(string header, string content)
            {
                sb.Append($"<h2>{header}</h2>");
                sb.Append(content);
            }
        }

        public AllAboutPage()
        {
            SetHtmlBody(BODY);
        }
    }
}
