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
        private bool _isLoaded = false;

        private Dictionary<string, StateObject> _states = new Dictionary<string, StateObject>();
        private float _x;
        private float _y;
        private float _scale = 4f;

        private float _xGestureStart;
        private float _yGestureStart;

        private int _minStateCases = int.MaxValue;
        private int _maxStateCases = int.MinValue;

        private StateObject _selectedState;
        private CountyObject _selectedCounty;
        private bool _isZoomed;

        private int _uiScale = 2;

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

            using (var stream = FileSystem.OpenAppPackageFileAsync("counties.json").Result)
            {
                using (var reader = new StreamReader(stream))
                {
                    JArray county_array = JArray.Parse(reader.ReadToEnd());
                    int i = 0;
                    foreach (JObject county in county_array)
                    {
                        int fips = county["fips"].Type == JTokenType.Null ? --i : county["fips"].Value<int>();
                        _states[county["state"].Value<string>()].Counties.Add(fips, new CountyObject
                        {
                            State = county["state"].Value<string>(),
                            StateAbbrev = county["state_abbrev"].Value<string>(),
                            County = county["county"].Value<string>(),
                            FIPS = fips,
                            Path = SKPath.ParseSvgPathData(county["path"].Value<string>())
                        });
                    }
                }
            }
            ZoomState("Virginia");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_isLoaded) return;
            _isLoaded = true;

            foreach (StateCasesItem stateCasesItem in App.Database.GetStateCasesItemsAsync().Result)
            {
                if (!_states.ContainsKey(stateCasesItem.State)) continue;
                _states[stateCasesItem.State].CasesItem = stateCasesItem;
                _minStateCases = Math.Min(_minStateCases, stateCasesItem.Cases);
                _maxStateCases = Math.Max(_maxStateCases, stateCasesItem.Cases);
                _canvasView.InvalidateSurface();
            }
            foreach (CountyCasesItem countyCasesItem in App.Database.GetCountyCasesItemsAsync().Result)
            {
                if (!_states.ContainsKey(countyCasesItem.State)) continue;
                StateObject state = _states[countyCasesItem.State];
                Console.WriteLine(state.State + " " + countyCasesItem.County + " " + countyCasesItem.FIPS + " " + state.Counties.ContainsKey(countyCasesItem.FIPS));
                if (!state.Counties.ContainsKey(countyCasesItem.FIPS)) continue;
                state.Counties[countyCasesItem.FIPS].CasesItem = countyCasesItem;
                state.MinCountyCases = Math.Min(state.MinCountyCases, countyCasesItem.Cases);
                state.MaxCountyCases = Math.Max(state.MaxCountyCases, countyCasesItem.Cases);
            }
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
                    _selectedState = null;
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
                    if (_isZoomed)
                    {
                        if (!_selectedState.Path.Contains(localX, localY))
                        {
                            if (_selectedCounty != null)
                                _selectedCounty = null;
                            else
                            {
                                _selectedState = null;
                                _isZoomed = false;
                            }
                        }
                        else
                        {
                            foreach (CountyObject county in _selectedState.Counties.Values)
                            {
                                if (county.Path.Contains(localX, localY))
                                {
                                    _selectedCounty = county;
                                    updatePopUpInfo();
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        StateObject selected = null;
                        foreach (StateObject state in _states.Values)
                        {
                            if (state.Path.Contains(localX, localY))
                            {
                                selected = state;
                                break;
                            }
                        }
                        if (selected == null)
                        {
                            _selectedState = null;
                            _isZoomed = false;
                        }
                        else if (selected == _selectedState)
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
                                return false;
                            });
                            _selectedState = selected;
                            updatePopUpInfo();
                        }
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

            SKPaint stateStrokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White,
                StrokeWidth = 1,
            };

            SKPaint countyStrokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White,
                StrokeWidth = 0.25f,
            };

            _canvasTranslateMatrix = canvas.TotalMatrix;
            canvas.Scale(_scale);

            SKRect localBounds;
            canvas.GetLocalClipBounds(out localBounds);
            float transformedX = _x / canvas.TotalMatrix.ScaleX + localBounds.MidX;
            float transformedY = _y / canvas.TotalMatrix.ScaleY + localBounds.MidY;
            canvas.Translate(transformedX, transformedY);

            SKPaint fillPaint;
            SKPaint selectedPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 4
            };

            if (_isZoomed)
            {
                foreach (CountyObject county in _selectedState.Counties.Values)
                {
                    fillPaint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = getColorFromValue(county.CasesItem?.Cases ?? 0, _selectedState.MaxCountyCases),
                    };
                    canvas.DrawPath(county.Path, fillPaint);
                    canvas.DrawPath(county.Path, countyStrokePaint);
                }
                if (_selectedCounty != null)
                {
                    canvas.DrawPath(_selectedCounty.Path, selectedPaint);
                }
            }
            else
            {
                foreach (StateObject state in _states.Values)
                {
                    fillPaint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = getColorFromValue(state.CasesItem?.Cases ?? 0),
                    };
                    canvas.DrawPath(state.Path, fillPaint);
                    canvas.DrawPath(state.Path, stateStrokePaint);
                }
                if (_selectedCounty != null)
                {
                    canvas.DrawPath(_selectedState.Path, selectedPaint);
                }
            }
            

            canvas.Translate(-transformedX, -transformedY);
            canvas.Scale(1 / _scale * _uiScale);

            int maxCases = _isZoomed ? _selectedState.MaxCountyCases : _maxStateCases;
            int height = 400;
            int width = 10;
            float x = (info.Width / (_uiScale) - 30);
            float y = (info.Height / (2 * _uiScale) - height / 2);
            short segments = 10;
            short gap = 2;
            int segmentHeight = (height - gap * (segments - 1)) / segments;
            for (short i = 0; i < segments; i++)
            {
                SKPaint segPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = getColorFromValue((int)(maxCases * (1 - (float)i / (segments - 1))), maxCases)
                };
                SKRect seg = new SKRect(x, y + i * (segmentHeight + gap), x + width, y + i * (segmentHeight + gap) + segmentHeight);
                canvas.DrawRect(seg, segPaint);
            }

            SKPaint casesAmountPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black,
                TextSize = 16,
                TextAlign = SKTextAlign.Right,
                IsAntialias = true
            };

            canvas.DrawText(maxCases.ToString(), new SKPoint(x + width, y - casesAmountPaint.TextSize / 2), casesAmountPaint);
            canvas.DrawText(0.ToString(), new SKPoint(x + width, y + height + casesAmountPaint.TextSize), casesAmountPaint);



#if DEBUG
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
                $"{nameof(_selectedCounty)}: {_selectedCounty?.County ?? "null"}",
            };
            for (int i = 1; i <= lines.Length; i++)
                canvas.DrawText(lines[i - 1], 10, (debugFont.Size + 2) * i, debugFont, debugPaint);
#endif
        }

        SKColor getColorFromValue(int value)
        {
            return getColorFromValue(value, _maxStateCases);
        }

        SKColor getColorFromValue(int value, int max, float minLum = 0.2f)
        {
            Color ret = (Color)Application.Current.Resources["Primary"];
            ret = ret.WithLuminosity(1 - ((float)value / max * (0.7 - minLum) + minLum));
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

        private void updatePopUpInfo()
        {
            if (_selectedState == null)
            {
                _infoPopup.IsVisible = false;
                return;
            }

            _infoPopup.IsVisible = true;
            if (_selectedCounty != null)
            {
                _infoPopupHeaderDetail.IsVisible = true;
                _infoPopupHeaderDetail.Text = _selectedCounty.State;
                _infoPopupHeader.Text = _selectedCounty.County;
                _infoPopupCases.Text = _selectedCounty.CasesItem.Cases.ToString();
                _infoPopupDeaths.Text = _selectedCounty.CasesItem.Deaths.ToString();
            } 
            else
            {
                _infoPopupHeaderDetail.IsVisible = false;
                _infoPopupHeader.Text = _selectedState.State;
                _infoPopupCases.Text = _selectedState.CasesItem.Cases.ToString();
                _infoPopupDeaths.Text = _selectedState.CasesItem.Deaths.ToString();
            }
        }
    }
}