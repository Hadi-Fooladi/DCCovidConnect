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
    [QueryProperty("ID", "id")]
    public partial class InfoDetailPage : ContentPage
    {
        int id;
        public string ID
        {
            get => id.ToString();
            set
            {
                id = Int32.Parse(Uri.UnescapeDataString(value));
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
            InitializeComponent();
            parentView = new StackLayout
            {
                Margin = new Thickness(10, 5)
            };
            Content = new ScrollView
            {
                Content = parentView
            };
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            InfoDatabase.DataTasks[id]?.Wait();
            string JSON = App.Database.GetInfoItemAsync(id).Result.Content;
            JArray objects = JArray.Parse(JSON);
            foreach (JObject token in objects ?? Enumerable.Empty<JToken>())
            {
                await Task.Run(() =>
                {
                    var tcs = new TaskCompletionSource<bool>();
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        View parsed = await Parse(token);
                        if (parsed != null)
                            parentView.Children.Add(parsed);
                        tcs.SetResult(false);
                    }
                    );
                    return tcs.Task;
                });
            }
        }
        private async Task<View> Parse(JObject token)
        {
            Type type = getType(token);
            JArray children = token[Property.CHILDREN.ToString()]?.Value<JArray>();
            string href = token[Property.HREF.ToString()]?.Value<string>();
            string text = token[Property.TEXT.ToString()]?.Value<string>();
            View ret = null;
            switch (type)
            {
                case Type.OL:
                    break;
                case Type.UL:
                    ret = new StackLayout();
                    break;

                case Type.TABLE:
                    break;
                case Type.TBODY:
                    break;
                case Type.TD:
                    break;
                case Type.TR:
                    break;
                case Type.ITEM:
                    ret = new Expander { Content = new StackLayout() };
                    foreach (JObject obj in children ?? Enumerable.Empty<JToken>()) // finds first Title children, should be first element regardless
                    {
                        if (getType(obj) == Type.TITLE)
                        {
                            Label header = parseLabel(obj);
                            (ret as Expander).Header = header;
                            obj.Remove();
                            break;
                        }
                    }
                    break;
                case Type.NONE:
                    break;
                default:
                    ret = ParseView(token);
                    break;
            }
            Layout<View> layout = ret as Layout<View> ?? (ret as Expander)?.Content as Layout<View>;
            if (layout != null)
                foreach (JObject obj in children ?? Enumerable.Empty<JToken>())
                {
                    await Task.Run(async () =>
                    {
                        var tcs = new TaskCompletionSource<bool>();
                        View parsed = await Parse(obj);
                        if (parsed != null)
                            layout.Children.Add(parsed);
                        tcs.SetResult(false);
                        return tcs.Task;
                    });
                }
            return ret;
        }
        private View ParseView(JObject token)
        {
            Type type = getType(token);
            JArray children = token[Property.CHILDREN.ToString()]?.Value<JArray>();
            string src = token[Property.SRC.ToString()]?.Value<string>();
            View ret = null;
            switch (type)
            {
                case Type.IMG:
                    ret = new Image
                    {
                        Source = ImageSource.FromUri(new Uri(src))
                    };
                    break;
                case Type.BR:
                    ret = new Label();
                    break;
                case Type.IFRAME:
                    ret = new WebView
                    {
                        Source = src,
                        HeightRequest = Application.Current.MainPage.Width * 9 / 16,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        IsVisible = true
                    };
                    break;
                case Type.FIGURE:
                    break;
                default:
                    ret = parseLabel(token);
                    break;
            }
            return ret;
        }

        private Label parseLabel(JObject token)
        {
            Type type = getType(token);
            JArray children = token[Property.CHILDREN.ToString()]?.Value<JArray>();
            Label ret = new Label();
            ret.FormattedText = new FormattedString();
            switch (type)
            {
                case Type.P:
                    ret.Style = (Style)Resources["PStyle"];
                    break;
                case Type.LI:
                    ret.Style = (Style)Resources["LIStyle"];
                    ret.FormattedText.Spans.Add(new Span { Text = "\u2022 ", TextColor = (Color)Application.Current.Resources["Secondary"] });
                    break;
                // All in 1 case for the multiple HTML headers
                case Type header when header.ToString().Contains('H') && header.ToString().Length == 2:
                    bool success = Int32.TryParse(header.ToString().Substring(1), out int headerSize);
                    if (success)
                    {
                        ret.FontSize = headerSize * 12;
                    }
                    break;
                case Type.S:
                    break;
                case Type.SUP:
                    break;
                case Type.SPAN:
                    break;
                case Type.TITLE:
                    ret.Style = (Style)Resources["TITLEStyle"];
                    break;
                default:
                    return null;
            }
            foreach (JObject obj in children ?? Enumerable.Empty<JToken>())
            {
                foreach (Span span in parseText(obj))
                {
                    ret.FormattedText.Spans.Add(span);
                }
            }
            return ret;
        }
        private List<Span> parseText(JObject token)
        {
            Type type = getType(token);
            JArray children = token[Property.CHILDREN.ToString()]?.Value<JArray>();
            List<Span> ret = new List<Span>();
            if (type == Type.TEXT)
            {
                string text = token[Property.TEXT.ToString()]?.Value<string>();
                return new List<Span> { new Span { Text = text } };
            }
            else // Keep on parsing until text is found
            {
                foreach (JObject obj in children ?? Enumerable.Empty<JToken>())
                {
                    ret.AddRange(parseText(obj));
                }
            }
            switch (type)
            {
                case Type.COLOR:
                    foreach (Span span in ret)
                    {
                        span.TextColor = (Color)Application.Current.Resources["Secondary"];
                    }
                    break;
                case Type.A:
                    foreach (Span span in ret)
                    {
                        string href = token[Property.HREF.ToString()]?.Value<string>();
                        span.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(async () => await Launcher.OpenAsync(href)) });
                        span.TextColor = (Color)Application.Current.Resources["Secondary"];
                        span.TextDecorations = TextDecorations.Underline;
                    }
                    break;
                case Type.BOLD:
                    foreach (Span span in ret)
                    {
                        span.FontSize = 20;
                        span.FontAttributes = FontAttributes.Bold;
                    }
                    break;
            }
            return ret;
        }

        private Type getType(JToken token)
        {
            return (Type)Enum.Parse(typeof(Type), token[Property.TYPE.ToString()].Value<string>(), true);
        }
    }
}