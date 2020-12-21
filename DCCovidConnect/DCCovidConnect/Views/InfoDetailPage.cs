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

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await InfoDatabase.DataTasks[Title];
     
                string Test = (await App.Database.GetInfoItemAsync(title)).Content;
                Console.WriteLine(Test);
                JArray objects = JArray.Parse(Test);
                objects.Children<JObject>().ToList().ForEach(async t => await Parse(t, parentView));
            //string Test = @"[{ ""TYPE"": ""UL"", ""CHILDREN"": [ { ""TYPE"": ""LI"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""A person can be infected with the virus"" }, { ""TYPE"": ""COLOR"", ""CHILDREN"": [ { ""TYPE"": ""BOLD"", ""CHILDREN"": [{ ""TYPE"": ""TEXT"", ""TEXT"": ""1-14 days"" }] } ] }, { ""TYPE"": ""TEXT"", ""TEXT"": ""before showing symptoms."" } ] } ] }, { ""TYPE"": ""UL"", ""CHILDREN"": [ { ""TYPE"": ""LI"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""The most common initial symptoms are"" }, { ""TYPE"": ""BOLD"", ""CHILDREN"": [ { ""TYPE"": ""COLOR"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""fever, fatigue, and dry cough"" } ] } ] }, { ""TYPE"": ""TEXT"", ""TEXT"": "". However, anorexia (decreased appetite), nasal congestion, runny nose, diarrhea (present in children more than adults), myalgias (muscle aches), dyspnea (shortness of breath), sputum production, and anosmia (decreased or inability to smell one or more smells) can also be present.&nbsp;"" } ] } ] }, { ""TYPE"": ""UL"", ""CHILDREN"": [ { ""TYPE"": ""LI"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""Symptoms start out mild and worsen gradually. Symptoms can progress from mild to requiring hospital admission in a span of 5-10 days.&nbsp;"" } ] } ] }, { ""TYPE"": ""UL"", ""CHILDREN"": [ { ""TYPE"": ""LI"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""Young individuals can be infected with the virus, but remain asymptomatic or have mild symptoms, making them a vector that can spread the infection to those around them.&nbsp;"" } ] } ] }, { ""TYPE"": ""UL"", ""CHILDREN"": [ { ""TYPE"": ""LI"", ""CHILDREN"": [ { ""TYPE"": ""COLOR"", ""CHILDREN"": [ { ""TYPE"": ""BOLD"", ""CHILDREN"": [{ ""TYPE"": ""TEXT"", ""TEXT"": ""1 in 6"" }] } ] }, { ""TYPE"": ""TEXT"", ""TEXT"": ""people who have COVID-19 develop severe symptoms and require hospitalization."" } ] } ] }, { ""TYPE"": ""UL"", ""CHILDREN"": [ { ""TYPE"": ""LI"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""The populations that are most vulnerable are"" }, { ""TYPE"": ""BOLD"", ""CHILDREN"": [ { ""TYPE"": ""COLOR"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""middle-age and older adults, immunocompromised"" } ] } ] }, { ""TYPE"": ""TEXT"", ""TEXT"": ""individuals, and those with pre-existing medical conditions including, but not limited to"" }, { ""TYPE"": ""BOLD"", ""CHILDREN"": [ { ""TYPE"": ""COLOR"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""chronic lung disease, hypertension, diabetes, heart disease, and cancer"" } ] } ] }, { ""TYPE"": ""TEXT"", ""TEXT"": "". These populations are also at the highest risk of presenting with more advanced symptoms and progressing to more severe disease."" } ] } ] }, { ""TYPE"": ""UL"", ""CHILDREN"": [ { ""TYPE"": ""LI"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""According to the WHO, recovery time appears to be around"" }, { ""TYPE"": ""COLOR"", ""CHILDREN"": [ { ""TYPE"": ""BOLD"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""two weeks for mild infections"" } ] } ] }, { ""TYPE"": ""TEXT"", ""TEXT"": ""and"" }, { ""TYPE"": ""COLOR"", ""CHILDREN"": [ { ""TYPE"": ""BOLD"", ""CHILDREN"": [ { ""TYPE"": ""TEXT"", ""TEXT"": ""three to six weeks for severe disease"" } ] } ] } ] } ] }]";

        }

        FormattedString currentText;
        Span currentSpan;
        int currentFontSize = 10;
        Expander currentExpander;
        private async Task Parse(JObject token, Layout<View> parent)
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
                    currentSpan.TextColor = (Color) Application.Current.Resources["Secondary"];
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
            children?.Children<JObject>().ToList().ForEach(t => Parse(t, currentLayout));
        }
    }
}