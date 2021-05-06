using DCCovidConnect.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCCovidConnect.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using DCCovidConnect.Models;

namespace DCCovidConnect.Views
{
    public partial class HomePage : ContentPage
    {
        private HomeViewModel _viewModel;
        private SKPath _mainState;

        public HomePage()
        {
            InitializeComponent();
            BindingContext = new HomeViewModel();
            _viewModel = BindingContext as HomeViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.UpdateVariables();
            _mainState = MapService.Service.States[Settings.Current.DefaultState].Path;
            ZoomPath(_mainState);
        }

        // Values for the canvas camera
        private readonly float _SCALE_MULTIPLIER = 3.0f;
        private float _x;
        private float _y;
        private float _scale = 4f;

        private SKMatrix _canvasTranslateMatrix;
        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            _canvasTranslateMatrix = canvas.TotalMatrix;
            canvas.Scale(_scale);

            SKRect localBounds;
            canvas.GetLocalClipBounds(out localBounds);
            float transformedX = _x / canvas.TotalMatrix.ScaleX + localBounds.MidX;
            float transformedY = _y / canvas.TotalMatrix.ScaleY + localBounds.MidY;
            canvas.Translate(transformedX, transformedY);

            SKPaint stateFillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = ((Color)Application.Current.Resources["Primary"]).ToSKColor()
            };

            SKPaint stateFillPaintOther = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = ((Color)Application.Current.Resources["Primary"]).WithLuminosity(0.8).ToSKColor()
            };

            SKPaint stateBorderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                Color = ((Color)Application.Current.Resources["White"]).ToSKColor(),
            };

            foreach (StateObject state in MapService.Service.States.Values)
            {
                canvas.DrawPath(state.Path, stateFillPaintOther);
                canvas.DrawPath(state.Path, stateBorderPaint);
            }
            canvas.DrawPath(_mainState, stateFillPaint);
        }

        private bool ZoomPath(SKPath path)
        {
            if (path == null || !path.GetBounds(out var targetState))
                return false;
            // finds the scale to fit the state to the screen
            _scale = (float)_canvasView.Height / Math.Max(targetState.Width, targetState.Height) * _SCALE_MULTIPLIER;
            _x = -targetState.Left * _scale;
            _y = -targetState.MidY * _scale;
            _canvasView.InvalidateSurface();
            return true;
        }
    }
}