using DCCovidConnect.Data;
using DCCovidConnect.Models;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCCovidConnect.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        private Dictionary<string, StateObject> _states = new Dictionary<string, StateObject>();
        private float _x;
        private float _y;
        private float _scale = 4f;

        private float _xGestureStart;
        private float _yGestureStart;

        private int _minStateCases = int.MaxValue;
        private int _maxStateCases = int.MinValue;

        private StateObject _selectedState;
        private bool _isZoomed;

#if DEBUG
        private float _debugTouchX;
        private float _debugTouchY;
#endif

        private SKMatrix _canvasTranslateMatrix;
        public MapPage()
        {
            InitializeComponent();
            InitializeGestures();
            using (var stream = FileSystem.OpenAppPackageFileAsync("states.json").Result)
            {
                using (var reader = new StreamReader(stream))
                {
                    JArray state_array = JArray.Parse(reader.ReadToEnd());
                    foreach (JObject state in state_array)
                    {
                        //switch (state["state"].Value<string>())
                        //{
                        //    case "Virginia":
                        //    case "Maryland":
                        //        break;
                        //    default:
                        //        continue;
                        //}
                        _states.Add(state["state"].Value<string>(), new StateObject
                        {
                            State = state["state"].Value<string>(),
                            StateAbbrev = state["state_abbrev"].Value<string>(),
                            Path = SKPath.ParseSvgPathData(state["path"].Value<string>())
                        });
                    }
                }
            }


            foreach (StateCasesItem stateCasesItem in App.Database.GetStateCasesItemsAsync().Result)
            {
                if (!_states.ContainsKey(stateCasesItem.State)) continue;
                _states[stateCasesItem.State].CasesItem = stateCasesItem;
                if (stateCasesItem.Cases < _minStateCases)
                {
                    _minStateCases = stateCasesItem.Cases;
                }
                if (stateCasesItem.Cases > _maxStateCases)
                {
                    _maxStateCases = stateCasesItem.Cases;
                }
            }
            //using (var stream = await FileSystem.OpenAppPackageFileAsync("counties.json"))
            //{
            //    using (var reader = new StreamReader(stream))
            //    {
            //        counties_raw = JArray.Parse(await reader.ReadToEndAsync());
            //    }
            //}

            ZoomState("Virginia");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

        }

        private void InitializeGestures()
        {
            PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += PanScreen;
            PinchGestureRecognizer pinchGestureRecognizer = new PinchGestureRecognizer();
            pinchGestureRecognizer.PinchUpdated += ZoomCanvas;

            _canvasView.GestureRecognizers.Add(panGestureRecognizer);
            _canvasView.GestureRecognizers.Add(pinchGestureRecognizer);

            _canvasView.EnableTouchEvents = true;
            _canvasView.Touch += OnTouch;
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

        private CancellationTokenSource _cancellation = new CancellationTokenSource();
        private void OnTouch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    float localX = (e.Location.X - (float)_canvasView.CanvasSize.Width / 2 - _x) / _scale;
                    float localY = (e.Location.Y - (float)_canvasView.CanvasSize.Height / 2 - _y) / _scale;

#if DEBUG
                    Console.WriteLine("X: " + localX);
                    Console.WriteLine("Y: " + localY);
                    _debugTouchX = e.Location.X / _scale + _x;
                    _debugTouchY = e.Location.Y / _scale + _y;
                    _canvasView.InvalidateSurface();
#endif

                    StateObject selected = null;
                    foreach (StateObject state in _states.Values)
                    {
                        if (state.Path.Contains(localX, localY))
                        {
                            selected = state;
                        }
                    }
                    if (selected == null)
                    {
                        _selectedState = null;
                    }
                    else if(selected == _selectedState)
                    {
                        Interlocked.Exchange(ref _cancellation, new CancellationTokenSource()).Cancel();
                        _isZoomed = true;
                        ZoomState(_selectedState.State);
                    }
                    else
                    {
                        CancellationTokenSource cts = _cancellation;
                        Device.StartTimer(TimeSpan.FromSeconds(0.5), () =>
                        {
                            if (cts.IsCancellationRequested) return false;
                            _selectedState = null;
                            _isZoomed = false;
#if DEBUG
                            _canvasView.InvalidateSurface();
#endif
                            return false;
                        });
                        _selectedState = selected;
                    }
                    break;
            }
        }
        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKPaint strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White,
                StrokeWidth = 1,
            };

            _canvasTranslateMatrix = canvas.TotalMatrix;
            canvas.Scale(_scale);

            SKRect localBounds;
            canvas.GetLocalClipBounds(out localBounds);
            float transformedX = _x / canvas.TotalMatrix.ScaleX + localBounds.MidX;
            float transformedY = _y / canvas.TotalMatrix.ScaleY + localBounds.MidY;
            canvas.Translate(transformedX, transformedY);

            SKPaint fillPaint;
            foreach (StateObject state in _states.Values)
            {
                fillPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = getColorFromValue(state.CasesItem.Cases),
                };
                canvas.DrawPath(state.Path, fillPaint);
                canvas.DrawPath(state.Path, strokePaint);
            }
#if DEBUG
            canvas.Translate(-transformedX, -transformedY);
            canvas.Scale(1 / _scale * 4);
            SKPaint debugPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black,
            };
            SKFont debugFont = new SKFont
            {
                Size = 14,
            };
            string[] lines = {
                $"{nameof(_x)}: {_x}",
                $"{nameof(_y)}: {_y}",
                $"{nameof(_scale)}: {_scale}",
                $"{nameof(_debugTouchX)}: {_debugTouchX}",
                $"{nameof(_debugTouchY)}: {_debugTouchY}",
                $"{nameof(_selectedState)}: {_selectedState?.State ?? "null"}",
            };
            for (int i = 1; i <= lines.Length; i++)
                canvas.DrawText(lines[i - 1], 10, (debugFont.Size + 2) * i, debugFont, debugPaint);
#endif
        }

        SKColor getColorFromValue(int value)
        {
            Color ret = (Color)Application.Current.Resources["Primary"];
            ret = ret.WithLuminosity(1 - (float)value / _maxStateCases * 0.7);
            return ret.ToSKColor();
        }

        private bool ZoomState(string state)
        {
            SKRect targetState;
            SKPath path = _states?[state].Path;
            if (path == null || !path.GetBounds(out targetState))
                return false;
            _scale = (float)DeviceDisplay.MainDisplayInfo.Width / (Math.Max(targetState.Width, targetState.Height) * 1.1f);
            _scale = Math.Min(_scale, 7.0f);
            _x = -targetState.MidX * _scale;
            _y = -targetState.MidY * _scale;
            _canvasView.InvalidateSurface();
            return true;
        }
    }
}