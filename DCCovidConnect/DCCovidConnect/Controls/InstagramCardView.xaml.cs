using DCCovidConnect.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCCovidConnect.Controls
{
    public partial class InstagramCardView : ContentView
    {
        public static readonly BindableProperty InstagramItemSourceProperty = BindableProperty.Create(
                                                                                propertyName: nameof(InstagramItemSource),
                                                                                returnType: typeof(InstagramItem),
                                                                                declaringType: typeof(InstagramCardView),
                                                                                defaultValue: default(InstagramItem),
                                                                                defaultBindingMode: BindingMode.OneWay,
                                                                                propertyChanged: OnInstagramItemPropertyChanged);

        public InstagramItem InstagramItemSource
        {
            get => (InstagramItem)GetValue(InstagramItemSourceProperty);
            set => SetValue(InstagramItemSourceProperty, value);
        }

        private static void OnInstagramItemPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            InstagramCardView control = (InstagramCardView)bindable;
            InstagramItem item = (InstagramItem)newValue;
            control.FullName.Text = item.FullName;
            control.ProfileImage.Source = item.ProfileImage;
            control.Images.ItemsSource = item.Images;
            control.Text.Text = item.Text;
            control.LikesCount.Text = item.LikesCount.ToString();
            control.CommentsCount.Text = item.CommentsCount.ToString();
        }

        public InstagramCardView()
        {
            InitializeComponent();
        }
    }
}