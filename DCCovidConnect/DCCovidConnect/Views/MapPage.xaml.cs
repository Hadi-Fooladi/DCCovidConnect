using DCCovidConnect.Models;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCCovidConnect.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private List<StateObject> _states = new List<StateObject>();
        private float _x;
        private float _y;
        private float _scale = 1f;

        private float _xGestureStart;
        private float _yGestureStart;

        private SKMatrix _canvasTranslateMatrix;
        public MapPage()
        {
            InitializeComponent();
            InitializeGestures();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            using (var stream = await FileSystem.OpenAppPackageFileAsync("states.json"))
            {
                using (var reader = new StreamReader(stream))
                {
                    JArray state_array = JArray.Parse(await reader.ReadToEndAsync());
                    foreach (JObject state in state_array)
                    {
                        _states.Add(new StateObject { 
                            State = state["state"].Value<string>(),
                            StateAbbrev = state["state_abbrev"].Value<string>(), 
                            Path = SKPath.ParseSvgPathData(state["path"].Value<string>()) 
                        });
                    }
                }
            }
            //using (var stream = await FileSystem.OpenAppPackageFileAsync("counties.json"))
            //{
            //    using (var reader = new StreamReader(stream))
            //    {
            //        counties_raw = JArray.Parse(await reader.ReadToEndAsync());
            //    }
            //}
            _canvasView.InvalidateSurface();
        }

        private void InitializeGestures()
        {
            PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += PanScreen;
            PinchGestureRecognizer pinchGestureRecognizer = new PinchGestureRecognizer();
            pinchGestureRecognizer.PinchUpdated += ZoomCanvas;

            _canvasView.GestureRecognizers.Add(panGestureRecognizer);
            _canvasView.GestureRecognizers.Add(pinchGestureRecognizer);
        }

        private void ZoomCanvas(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Running:
                    float pinchX = (float)(e.ScaleOrigin.X * Width);
                    float pinchY = (float)(e.ScaleOrigin.Y * Height);

                    float newScale = _scale * (float)e.Scale;
                    float scaleRatio = newScale / _scale;

                    float translatedX = pinchX - _canvasTranslateMatrix.TransX;
                    float translatedY = pinchY - _canvasTranslateMatrix.TransY;

                    float newX = translatedX - scaleRatio * (translatedX - _x);
                    float newY = translatedY - scaleRatio * (translatedY - _y);

                    UpdateCanvasProperties(newX, newY, newScale);
                    break;
            }
        }

        private void PanScreen(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _xGestureStart = _x;
                    _yGestureStart = _y;
                    break;
                case GestureStatus.Running:
                    UpdateCanvasProperties((float)e.TotalX + _xGestureStart, (float)e.TotalY + _yGestureStart);
                    break;
            }
        }

        private void UpdateCanvasProperties(float x, float y, float? scale = null)
        {
            _x = x;
            _y = y;
            _scale = scale ?? _scale;
            _canvasView.InvalidateSurface();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Blue,
            };

            _canvasTranslateMatrix = canvas.TotalMatrix;
            canvas.Scale(_scale);

            float scaledX = _x / canvas.TotalMatrix.ScaleX;
            float scaledY = _y / canvas.TotalMatrix.ScaleY;
            //SKMatrix transformMatrix = SKMatrix.CreateTranslation(scaledX, scaledY);
            canvas.Translate(scaledX, scaledY);
            foreach ( StateObject state in _states)
            {
                //state.Path.Transform(transformMatrix);
                canvas.DrawPath(state.Path, paint );
            }
        }
    }
}