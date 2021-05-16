using DCCovidConnect.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DCCovidConnect.Views
{
    [QueryProperty("ID", "id")]
    /// <summary>
    /// This page displays info based on the id that is passed in the query property.
    /// </summary>
    public partial class InfoDetailPage : ContentPage
    {
        int _id;

        public string ID
        {
            get => _id.ToString();
            set
            {
                _id = int.Parse(Uri.UnescapeDataString(value));
                OnPropertyChanged();
            }
        }

        private enum Property
        {
            NONE,
            TYPE,
            HREF,
            TEXT,
            CHILDREN,
            SRC
        }

        private enum Type
        {
            NONE,
            TITLE,
            ITEM,
            A,
            P,
            BOLD,
            UL,
            OL,
            LI,
            TEXT,
            COLOR,
            IMG,
            BR,
            H1,
            H2,
            H3,
            H4,
            H5,
            H6,
            SPAN,
            FIGURE,
            S,
            SUP,
            TABLE,
            TBODY,
            TD,
            TR,
            IFRAME
        }

        private readonly StackLayout _parentView;

        public InfoDetailPage()
        {
            InitializeComponent();
            _parentView = new StackLayout
            {
                Margin = new Thickness(20, 5),
            };
            Content = new ScrollView
            {
                Content = _parentView
            };
        }

        private bool _isLoaded = false;

        protected override async void OnAppearing()
        {
            if (_isLoaded) return;
            _isLoaded = true;
            base.OnAppearing();
            // Constructs the page from the JSON object
            InfoDatabase.DataTasks[_id]?.Wait();
            string json = App.Database.GetInfoItemAsync(_id).Result.Content;
            JArray objects = JArray.Parse(json);
            foreach (JObject token in objects ?? Enumerable.Empty<JToken>())
            {
                await Task.Run(() =>
                {
                    var tcs = new TaskCompletionSource<bool>();
                    MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            View parsed = await Parse(token);
                            if (parsed != null)
                                _parentView.Children.Add(parsed);
                            tcs.SetResult(false);
                        }
                    );
                    return tcs.Task;
                });
            }
        }

        /// <summary>
        /// This method parses the nest of objects.
        /// </summary>
        /// <param name="token">The node to parse.</param>
        /// <returns>Returns a view of all the nested elements.</returns>
        private async Task<View> Parse(JObject token)
        {
            Type type = GetType(token);
            JArray children = token[Property.CHILDREN.ToString()]?.Value<JArray>();
            string href = token[Property.HREF.ToString()]?.Value<string>();
            string text = token[Property.TEXT.ToString()]?.Value<string>();
            View ret = null;
            // Creates a layout based on the type
            switch (type)
            {
                case Type.OL:
                    break;
                case Type.UL:
                    ret = new StackLayout
                    {
                        Padding = new Thickness(32, 0, 0, 0)
                    };
                    break;
                case Type.LI:
                    if (children?.Where(obj => GetType(obj) == Type.UL).Count() == 0)
                    {
                        ret = new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                        };
                        Label label = new Label
                        {
                            Style = (Style)Resources["LIStyle"],
                            FormattedText = new FormattedString(),
                        };
                        foreach (Span span in ParseText(token))
                        {
                            span.FontSize = label.FontSize;
                            label.FormattedText.Spans.Add(span);
                        }
                        label.Style = (Style)Resources["LIStyle"];
                        ((Layout<View>)ret).Children.Add(ToBulleted(label));
                    }
                    else
                    {
                        ret = new StackLayout
                        {
                            Orientation = StackOrientation.Vertical,
                        };
                        Label label = null;
                        foreach (JObject obj in children ?? Enumerable.Empty<JToken>())
                        {
                            if (GetType(obj) != Type.UL)
                            {
                                if (label == null)
                                {
                                    label = new Label
                                    {
                                        Style = (Style)Resources["LIStyle"],
                                        FormattedText = new FormattedString(),
                                    };
                                }

                                foreach (Span span in ParseText(obj))
                                {
                                    span.FontSize = label.FontSize;
                                    label.FormattedText.Spans.Add(span);
                                }
                            }
                            else
                            {
                                if (label != null)
                                    ((Layout<View>)ret).Children.Add(ToBulleted(label));
                                label = null;
                                ((Layout<View>)ret).Children.Add(await Parse(obj));
                            }
                        }

                        if (label != null)
                            (ret as Layout<View>).Children.Add(label);
                    }

                    return ret;
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
                    foreach (JObject obj in children ?? Enumerable.Empty<JToken>()
                    ) // finds first Title children, should be first element regardless
                    {
                        if (GetType(obj) == Type.TITLE)
                        {
                            Label label = ParseLabel(obj);
                            label.Style = (Style)Resources["EXPANDERStyle"];
                            Frame header = new Frame
                            {
                                Content = label,
                                BackgroundColor = (Color)App.Current.Resources["ElementBackgroundColor"],
                                CornerRadius = 5,
                                HasShadow = false,
                            };
                            (ret as Expander).Header = header;
                            obj.Remove();
                            break;
                        }
                    }

                    break;
                case Type.NONE:
                    break;
                default:
                    // If its not a layout parse the element
                    ret = ParseView(token);
                    break;
            }

            Layout<View> layout = ret as Layout<View> ?? (ret as Expander)?.Content as Layout<View>;
            // If its a layout, parse the children elements
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

        /// <summary>
        /// Returns a View based on the input type.
        /// </summary>
        /// <param name="token">Input element.</param>
        /// <returns>The outputted view.</returns>
        private View ParseView(JObject token)
        {
            Type type = GetType(token);
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
                    // If it's a label, parse the individual text properties.
                    ret = ParseLabel(token);
                    break;
            }

            return ret;
        }

        /// <summary>
        /// This method returns a label based on the inputted object.
        /// </summary>
        /// <param name="token">Input object.</param>
        /// <returns>Returns a label.</returns>
        private Label ParseLabel(JObject token)
        {
            Type type = GetType(token);
            JArray children = token[Property.CHILDREN.ToString()]?.Value<JArray>();
            Label ret = new Label();
            ret.FormattedText = new FormattedString();
            switch (type)
            {
                case Type.P:
                    ret.Style = (Style)Resources["PStyle"];
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
            }

            // Iterates through the individual spans and applies styling based on them.
            foreach (JObject obj in children ?? Enumerable.Empty<JToken>())
            {
                foreach (Span span in ParseText(obj))
                {
                    span.FontSize = ret.FontSize;
                    ret.FormattedText.Spans.Add(span);
                }
            }

            return ret;
        }

        /// <summary>
        /// This method returns a list of spans based on the inputted object.
        /// </summary>
        /// <param name="token">Input object with children of spans.</param>
        /// <returns>Returns the outputted list of spans.</returns>
        private List<Span> ParseText(JObject token)
        {
            Type type = GetType(token);
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
                    ret.AddRange(ParseText(obj));
                }
            }

            App.Current.Resources.TryGetValue("AccentColor", out var accent);
            switch (type)
            {
                case Type.COLOR:
                    foreach (Span span in ret)
                    {
                        span.TextColor = (Color)accent;
                    }

                    break;
                // Adds a hyperlink
                case Type.A:
                    foreach (Span span in ret)
                    {
                        string href = token[Property.HREF.ToString()]?.Value<string>();
                        span.GestureRecognizers.Add(new TapGestureRecognizer
                        { Command = new Command(async () => await Launcher.OpenAsync(href)) });
                        span.TextColor = (Color)accent;
                        span.TextDecorations = TextDecorations.Underline;
                    }

                    break;
                case Type.BOLD:
                    foreach (Span span in ret)
                    {
                        span.FontAttributes = FontAttributes.Bold;
                    }

                    break;
            }

            return ret;
        }

        /// <summary>
        /// This method returns the type of the inputted token.
        /// </summary>
        /// <param name="token">The inputted token.</param>
        /// <returns>Returns the type of the token.</returns>
        private Type GetType(JToken token)
        {
            return (Type)Enum.Parse(typeof(Type), token[Property.TYPE.ToString()].Value<string>(), true);
        }

        private Layout<View> ToBulleted(Label label)
        {
            App.Current.Resources.TryGetValue("AccentColor", out var accent);
            return new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    new Label
                    {
                        Text = "\u2022",
                        TextColor = (Color) accent,
                        FontSize = label.FontSize
                    },
                    label
                }
            };
        }
    }
}