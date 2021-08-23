namespace DCCovidConnect.Views
{
    internal class AboutPage : WebViewPage
    {
        public AboutPage()
        {
            SetHtmlBody(BODY);
        }

        public const string BODY = @"
<p>This smartphone application was developed in partnership between Children’s National Hospital, Thomas Jefferson High School for Science and Technology, and The George Washington University School of Medicine & Health Sciences.</p>";
    }
}
