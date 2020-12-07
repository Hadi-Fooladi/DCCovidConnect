using DCCovidConnect.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DCCovidConnect.Views
{
    public partial class HomePage : ContentPage
    {
        SKPath path1 = SKPath.ParseSvgPathData("M0 -2.86102e-06H375L374.833 80.1249C374.833 80.1249 373.338 168.292 197.81 290.5C196.933 290.967 196.052 291.397 195.167 291.875C48.167 371.339 0 291.875 0 291.875V-2.86102e-06Z");
        SKPath path2 = SKPath.ParseSvgPathData("M0 291.855V3.8147e-06L375 0C375 -200.113 374.833 410.32 374.833 210.985C374.833 210.985 345.049 195.026 195.371 291.745C195.303 291.782 195.235 291.818 195.167 291.855C48.167 371.319 0 291.855 0 291.855Z");
        SKPath path3 = SKPath.ParseSvgPathData("M0 291.855C0 291.855 48.167 371.319 195.167 291.855C342.167 212.391 375 290 375 290V-1H0V291.855Z");
        public HomePage()
        {
            InitializeComponent();
            BindingContext = new HomeViewModel();

            var color = baseLayout.BackgroundColor;
            baseLayout.BackgroundColor = color.WithLuminosity(color.Luminosity * 1.3);
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear();
            SKRect bounds1, bounds2, bounds3;
            path1.GetTightBounds(out bounds1);
            path2.GetTightBounds(out bounds2);
            path3.GetTightBounds(out bounds3);

            float scaleRatio = info.Width / bounds1.Width;

            canvas.Scale(scaleRatio);

            canvas.DrawPath(path3, new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Blue
            });
            canvas.DrawPath(path2, new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Red
            });
            canvas.DrawPath(path1, new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Yellow
            });
        }
    }
}
