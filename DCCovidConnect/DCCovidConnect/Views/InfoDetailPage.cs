using DCCovidConnect.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DCCovidConnect.Views
{
    [QueryProperty("Title", "title")]
    public class InfoDetailPage : ContentPage
    {
        string title;
        public string Title
        {
            get => title;
            set
            {
                title = Uri.UnescapeDataString(value);
                OnPropertyChanged();
            }
        }
        enum Property
        {
            NONE, TYPE, HREF, TEXT, CHILDREN, SRC
        }
        enum Type
        {
            NONE, TITLE, ITEM, A, P, BOLD, UL, OL, LI, TEXT, COLOR, IMG, BR, H1, H2, H3, H4, H5, H6, SPAN, FIGURE, S, SUP, TABLE, TBODY, TD, TR, IFRAME
        }
        StackLayout parentView;
        public InfoDetailPage()
        {
            parentView = new StackLayout
            {
                Margin = new Thickness(10, 5)
            };
            Content = new ScrollView
            {
                Content = parentView
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            InfoDatabase.DataTasks[Title].Wait();
            string Test = App.Database.GetInfoItemAsync(title).Result.Content;
            Console.WriteLine(Test);
            JArray objects = JArray.Parse(Test);
            objects.Children<JObject>().ToList().ForEach(t => Task.Run(() => Parse(t, parentView, null, null, 10, null)));
        }
        private void Parse(JObject token, Layout<View> parent, FormattedString currentText, Span currentSpan, int currentFontSize, Expander currentExpander)
        {
            Type type = (Type)Enum.Parse(typeof(Type), token[Property.TYPE.ToString()].Value<string>(), true);
            JArray children = token[Property.CHILDREN.ToString()]?.Value<JArray>();
            string href = token[Property.HREF.ToString()]?.Value<string>();
            string text = token[Property.TEXT.ToString()]?.Value<string>();
            string src = token[Property.SRC.ToString()]?.Value<string>();
            Layout<View> currentLayout = parent;
            switch (type)
            {
                case Type.TEXT:
                    if (currentSpan == null)
                        currentSpan = new Span();
                    currentSpan.Text = text;
                    currentSpan.FontSize = currentFontSize;
                    currentText.Spans.Add(currentSpan);
                    currentSpan = null;
                    break;
                case Type.A:
                    if (currentSpan == null)
                        currentSpan = new Span();
                    currentSpan = new Span { FontSize = currentFontSize, Text = currentSpan.Text };
                    currentSpan.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Launcher.OpenAsync(href)) });
                    currentSpan.TextColor = (Color)Application.Current.Resources["Secondary"];
                    currentSpan.TextDecorations = TextDecorations.Underline;
                    break;
                case Type.P:
                    currentText = new FormattedString();
                    currentFontSize = 20;
                    currentLayout.Children.Add(new Label { FormattedText = currentText, FontSize = currentFontSize });
                    break;
                case Type.BOLD:
                    if (currentSpan == null)
                        currentSpan = new Span();
                    currentSpan.FontAttributes = FontAttributes.Bold;
                    break;
                case Type.SPAN:
                    break;
                case Type.S:
                    break;
                case Type.SUP:
                    break;
                case Type.H1:
                    break;
                case Type.H2:
                    break;
                case Type.H3:
                    break;
                case Type.H4:
                    break;
                case Type.H5:
                    break;
                case Type.H6:
                    break;
                case Type.OL:
                    break;
                case Type.UL:
                    currentLayout = new StackLayout();
                    parent.Children.Add(currentLayout);
                    break;
                case Type.LI:
                    currentText = new FormattedString();
                    currentFontSize = 18;
                    currentLayout.Children.Add(new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children = {
                            new Label { Text = "•", FontSize = currentFontSize, TextColor = (Color) Application.Current.Resources["Secondary"]},
                            new Label { FormattedText = currentText, FontSize = currentFontSize }
                        }
                    });
                    break;
                case Type.IMG:
                    currentLayout.Children.Add(new Image
                    {
                        Source = ImageSource.FromUri(new Uri(src))
                    });
                    break;
                case Type.BR:
                    currentLayout.Children.Add(new Label());
                    break;
                case Type.IFRAME:
                    currentLayout.Children.Add(new WebView
                    {
                        Source = src,
                        HeightRequest = Application.Current.MainPage.Width * 9 / 16,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        IsVisible = true
                    });
                    break;
                case Type.TABLE:
                    break;
                case Type.TBODY:
                    break;
                case Type.TD:
                    break;
                case Type.TR:
                    break;
                case Type.TITLE:
                    currentText = new FormattedString();
                    currentFontSize = 24;
                    currentExpander.Header = new Label { FormattedText = currentText, FontSize = currentFontSize, TextColor = (Color)Application.Current.Resources["Black"] };
                    break;
                case Type.COLOR:
                    if (currentSpan == null)
                        currentSpan = new Span();
                    currentSpan.TextColor = (Color)Application.Current.Resources["Secondary"];
                    break;
                case Type.FIGURE:
                    break;
                case Type.ITEM:
                    currentExpander = new Expander();
                    currentLayout.Children.Add(currentExpander);
                    currentLayout = new StackLayout();
                    currentExpander.Content = currentLayout;
                    break;
                case Type.NONE:
                    break;
            }
            children?.Children<JObject>().ToList().ForEach(t => Parse(t, currentLayout, currentText, currentSpan, currentFontSize, currentExpander));
        }
    }
}