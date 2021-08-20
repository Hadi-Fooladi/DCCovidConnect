using Xamarin.Forms;

namespace DCCovidConnect
{
    internal class DefaultFontImageSource : FontImageSource
    {
        private static readonly double SIZE = Device.GetNamedSize(NamedSize.Large, typeof(Label));

        public DefaultFontImageSource()
        {
            Size = SIZE;
            FontFamily = "FontAwesomeSolid";
        }
    }

    internal class InfoFontImageSource : DefaultFontImageSource
    {
        public InfoFontImageSource()
        {
            SetDynamicResource(ColorProperty, "AccentColor");
        }
    }
}
