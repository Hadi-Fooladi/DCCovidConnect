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
    /// <summary>
    /// This page contains the map that displays data based on COVID19 cases in the United States
    /// </summary>
    public partial class MapPage : ContentPage
    {
        private bool _isLoaded = false;

        private Dictionary<string, StateObject> _states = new Dictionary<string, StateObject>();

        // Values for the canvas camera
        private float _x;
        private float _y;
        private float _scale = 4f;

        // Used for panning gesture
        private float _xGestureStart;
        private float _yGestureStart;

        private int _minStateCases = int.MaxValue;
        private int _maxStateCases = 0;

        private StateObject _selectedState;
        private StateObject _highlightedState;
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
            
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_isLoaded) return;
            _isLoaded = true;

            // loads in all the data from the path json files
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


            if (!App.Database.UpdateCovidStatsTask.IsCompleted)
            {
                _loadingIndicator.IsVisible = true;
                await App.Database.UpdateCovidStatsTask;
                _loadingIndicator.IsVisible = false;
            }
            _canvasView.IsEnabled = true;

            // loads in all the data from the database into the dictionary
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
                    if (!_isZoomed)
                    {
                        _selectedState = null;
                    }
                    _xGestureStart = _x;
                    _yGestureStart = _y;
                    break;
                case GestureStatus.Running:
                    UpdateCanvasProperties((float)e.TotalX + _xGestureStart, (float)e.TotalY + _yGestureStart);
                    break;
            }
        }

        /// <summary>
        /// This method updates canvas properties and invalidates the paint surface.
        /// </summary>
        /// <param name="x">New x value.</param>
        /// <param name="y">New y value.</param>
        /// <param name="scale">New scale ratio.</param>
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
                    // If zoomed, check if touch intercepts any counties in the selected state
                    // else look at the states
                    if (_isZoomed)
                    {
                        if (!_selectedState.Path.Contains(localX, localY))
                        {
                            if (_selectedCounty != null)
                            {
                                _selectedCounty = null;
                                _infoPopup.IsVisible = false;
                            }
                            else
                            {
                                _selectedState = null;
                                _highlightedState = null;
                                _infoPopup.IsVisible = false;
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
                            _isZoomed = false;
                            _selectedState = null;
                            _highlightedState = null;
                            _infoPopup.IsVisible = false;
                        }
                        else if (selected == _selectedState)
                        {
                            // Select the state and cancel the timer to clear the selected state.
                            Interlocked.Exchange(ref _cancellation, new CancellationTokenSource()).Cancel();
                            _isZoomed = true;
                            ZoomState(_selectedState.State);
                        }
                        else
                        {
                            CancellationTokenSource cts = _cancellation;
                            // Clears the selected state after a certain about of time.
                            Device.StartTimer(TimeSpan.FromSeconds(0.5), () =>
                            {
                                if (cts.IsCancellationRequested) return false;
                                _selectedState = null;
                                _isZoomed = false;
                                return false;
                            });
                            _selectedState = selected;
                            _highlightedState = selected;
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
                StrokeWidth = 1,
                StrokeCap = SKStrokeCap.Round
            };

            // Paint the counties if zoomed, else states
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
                if (_highlightedState != null)
                {
                    canvas.DrawPath(_highlightedState.Path, selectedPaint);
                }
            }
            
            // Undo transformations to paint absolute items
            canvas.Translate(-transformedX, -transformedY);
            canvas.Scale(1 / _scale * _uiScale);

            // Code to draw the cases legend
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
            // Draws all of the variables on the canvas
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

        /// <summary>
        /// This method gets the color based on the max cases in the states.
        /// </summary>
        /// <param name="value">Number of cases</param>
        /// <returns>Returns the color.</returns>
        SKColor getColorFromValue(int value)
        {
            return getColorFromValue(value, _maxStateCases);
        }

        /// <summary>
        /// This method gets the color based on the value.
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="max">Max value</param>
        /// <param name="minLum">Minimum luminosity of the color</param>
        /// <returns>Returns the color.</returns>
        SKColor getColorFromValue(int value, int max, float minLum = 0.2f)
        {
            Color ret = (Color)Application.Current.Resources["Primary"];
            ret = ret.WithLuminosity(1 - ((float)value / max * (0.7 - minLum) + minLum));
            return ret.ToSKColor();
        }

        /// <summary>
        /// This method zooms and centers on the inputted state.
        /// </summary>
        /// <param name="state">State to focus on.</param>
        /// <returns>Returns whenever it is successfully able to fit the state.</returns>
        private bool ZoomState(string state)
        {
            SKRect targetState;
            SKPath path = _states?[state].Path;
            if (path == null || !path.GetBounds(out targetState))
                return false;
            // finds the scale to fit the state to the screen
            _scale = (float)DeviceDisplay.MainDisplayInfo.Width / (Math.Max(targetState.Width, targetState.Height) * 1.1f);
            // a maximum scale for the smaller states
            _scale = Math.Min(_scale, 7.0f);
            _x = -targetState.MidX * _scale;
            _y = -targetState.MidY * _scale;
            _canvasView.InvalidateSurface();
            return true;
        }

        /// <summary>
        /// This method updates the info on the popup.
        /// </summary>
        private void updatePopUpInfo()
        {
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