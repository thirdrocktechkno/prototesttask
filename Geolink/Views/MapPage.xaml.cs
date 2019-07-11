using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace Geolink
{
    public partial class MapPage : ContentPage
    {
        public double totalPrice { get; set; }
        public double innerPrice = 0;
        private ObservableCollection<LatLong> latLongList = new ObservableCollection<LatLong>();
        public ObservableCollection<LatLong> LatLongList
        {
            get { return latLongList; }
            set
            {
                latLongList = value; OnPropertyChanged();
            }
        }

        #region Bindable properties
        public bool isDestinationfocused = false, isOriginFocuse = false;
        public static readonly BindableProperty CenterMapCommandProperty =
            BindableProperty.Create(nameof(CenterMapCommand), typeof(ICommand), typeof(MapPage), null, BindingMode.TwoWay);

        public ICommand CenterMapCommand
        {
            get { return (ICommand)GetValue(CenterMapCommandProperty); }
            set { SetValue(CenterMapCommandProperty, value); }
        }

        public static readonly BindableProperty DrawRouteCommandProperty =
            BindableProperty.Create(nameof(DrawRouteCommand), typeof(ICommand), typeof(MapPage), null, BindingMode.TwoWay);

        public ICommand DrawRouteCommand
        {
            get { return (ICommand)GetValue(DrawRouteCommandProperty); }
            set { SetValue(DrawRouteCommandProperty, value); }
        }

        public static readonly BindableProperty UpdateCommandProperty =
          BindableProperty.Create(nameof(UpdateCommand), typeof(ICommand), typeof(MapPage), null, BindingMode.TwoWay);


        public ICommand UpdateCommand
        {
            get { return (ICommand)GetValue(UpdateCommandProperty); }
            set { SetValue(UpdateCommandProperty, value); }
        }

        public static readonly BindableProperty CleanPolylineCommandProperty =
          BindableProperty.Create(nameof(CleanPolylineCommand), typeof(ICommand), typeof(MapPage), null, BindingMode.TwoWay);


        public ICommand CleanPolylineCommand
        {
            get { return (ICommand)GetValue(CleanPolylineCommandProperty); }
            set { SetValue(CleanPolylineCommandProperty, value); }
        }


        public static readonly BindableProperty GetActualLocationCommandProperty =
            BindableProperty.Create(nameof(GetActualLocationCommand), typeof(ICommand), typeof(MapPage), null, BindingMode.TwoWay);

        public ICommand GetActualLocationCommand
        {
            get { return (ICommand)GetValue(GetActualLocationCommandProperty); }
            set { SetValue(GetActualLocationCommandProperty, value); }
        }
        #endregion
        public MapPage()
        {
            try
            {
                InitializeComponent();
                BindingContext = new MapPageViewModel();
                DrawRouteCommand = new Command<List<Xamarin.Forms.GoogleMaps.Position>>(DrawRoute);
                UpdateCommand = new Command<Xamarin.Forms.GoogleMaps.Position>(Update);
                CenterMapCommand = new Command<Location>(OnCenterMap);
                CleanPolylineCommand = new Command(CleanPolyline);

                LoadPolygon(Application.Current.Properties);
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }


        async void Update(Xamarin.Forms.GoogleMaps.Position position)
        {
            if (map.Pins.Count == 1 && map.Polylines != null && map.Polylines?.Count > 1)
                return;

            var cPin = map.Pins.FirstOrDefault();

            if (cPin != null)
            {
                cPin.Position = new Position(position.Latitude, position.Longitude);
                cPin.Icon = (Device.RuntimePlatform == Device.Android) ? BitmapDescriptorFactory.FromBundle("ic_taxi.png") : BitmapDescriptorFactory.FromView(new Image() { Source = "ic_taxi.png", WidthRequest = 25, HeightRequest = 25 });

                await map.MoveCamera(CameraUpdateFactory.NewPosition(new Position(position.Latitude, position.Longitude)));
                var previousPosition = map?.Polylines?.FirstOrDefault()?.Positions?.FirstOrDefault();
                map.Polylines?.FirstOrDefault()?.Positions?.Remove(previousPosition.Value);
            }
            else
            {
                //END TRIP
                map.Polylines?.FirstOrDefault()?.Positions?.Clear();
            }

        }

        void CleanPolyline()
        {
            try
            {
                map.Pins.Clear();
                map.Polylines.Clear();
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }

        async void DrawRoute(List<Xamarin.Forms.GoogleMaps.Position> list)
        {
            try
            {
                map.Polylines.Clear();
                var polyline = new Xamarin.Forms.GoogleMaps.Polyline();
                polyline.StrokeColor = Color.Black;
                polyline.StrokeWidth = 3;

                ObservableCollection<LatLongBool> latLongBools = new ObservableCollection<LatLongBool>();

                foreach (var p in list)
                {
                    latLongBools.Add(new LatLongBool()
                    {
                        lat = p.Latitude,
                        longg = p.Longitude,
                        isInMap = isPointInPolygon(p, latLongList)
                    });
                    polyline.Positions.Add(p);
                }

                //map.Polygons.Contains()

                latLongBools = new ObservableCollection<LatLongBool>(latLongBools.Where(x => x.isInMap).ToList());

                try
                {
                    var startLat = list.First().Latitude.ToString();
                    var startLong = list.First().Longitude.ToString();
                    var EndLat = list.Last().Latitude.ToString();
                    var EndLong = list.Last().Longitude.ToString();
                    var DirectionRes = await (this.BindingContext as MapPageViewModel).googleMapsApi.GetDirections(startLat, startLong, EndLat, EndLong);
                    if (DirectionRes == null) return;
                    var totaldistance = DirectionRes.Routes.First().Legs.First().Distance;
                    if (latLongBools != null && latLongBools.Count > 0)
                    {
                        var innerDistanceRes = await (this.BindingContext as MapPageViewModel).googleMapsApi.GetDirections(latLongBools.First().lat.ToString(), latLongBools.First().longg.ToString(), latLongBools.Last().lat.ToString(), latLongBools.Last().longg.ToString());
                        if (innerDistanceRes == null) return;
                        var innerDistance = innerDistanceRes.Routes.First().Legs.First().Distance;
                        innerPrice = innerDistance.Value / 1000 * 47;//47 cents
                    }

                    totalPrice = totaldistance.Value / 1000 * 94 - innerPrice;
                    innerPrice = 0;

                    var action = App.Current.MainPage.DisplayAlert("price", totalPrice.ToString() + "cents", "ok", "cancel");

                }
                catch (Exception ex)
                {
                    Logger.SendErrorLog(ex);
                }

                map.Polylines.Add(polyline);
                map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(polyline.Positions[0].Latitude, polyline.Positions[0].Longitude), Xamarin.Forms.GoogleMaps.Distance.FromMiles(0.50f)));

                var pin = new Xamarin.Forms.GoogleMaps.Pin
                {
                    Type = PinType.SearchResult,
                    Position = new Position(polyline.Positions.First().Latitude, polyline.Positions.First().Longitude),
                    Label = "Pin",
                    Address = "Pin",
                    Tag = "CirclePoint",
                    Icon = (Device.RuntimePlatform == Device.Android) ? BitmapDescriptorFactory.FromBundle("ic_pickuplocation.png") : BitmapDescriptorFactory.FromView(new Image() { Source = "ic_pickuplocation.png", WidthRequest = 70, HeightRequest = 70 })

                };
                map.Pins.Add(pin);

                var Destpin = new Xamarin.Forms.GoogleMaps.Pin
                {
                    Type = PinType.SearchResult,
                    Position = new Position(polyline.Positions.Last().Latitude, polyline.Positions.Last().Longitude),
                    Label = "Pin",
                    Address = "Pin",
                    Tag = "CirclePoint",
                    Icon = (Device.RuntimePlatform == Device.Android) ? BitmapDescriptorFactory.FromBundle("ic_pickuplocation.png") : BitmapDescriptorFactory.FromView(new Image() { Source = "ic_pickuplocation.png", WidthRequest = 70, HeightRequest = 70 })

                };
                map.Pins.Add(Destpin);
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }

        void OnCenterMap(Location location)
        {
            map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(location.Latitude, location.Longitude), Distance.FromMiles(2)));
        }



        //Desactivate pin tap
        void Map_PinClicked(object sender, PinClickedEventArgs e)
        {
            e.Handled = true;
        }

        public void HandleSearchContentView(bool show)
        {
            //searchContentView.IsVisible = show;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            //var safeInsets = On<Xamarin.Forms.PlatformConfiguration.iOS>().SafeAreaInsets();

            //if (safeInsets.Top > 0)
            //{
            //    menuIcon.Margin = backButton.Margin = new Thickness(20, 40, 20, 0);
            //    headerSearch.BackButtonPadding = new Thickness(0, 20, 0, 0);
            //}
        }


        public void OnDestinationEntryChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == IsFocusedProperty.PropertyName)
            {
                HandleSearchContentView(destinationEntry.IsFocused);
            }
        }

        public void OnOringinEntryChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == IsFocusedProperty.PropertyName)
            {
                HandleSearchContentView(originEntry.IsFocused);
            }
        }

        public void changeDestination(string desitnation)
        {
            destinationEntry.Text = desitnation;
        }

        public void changeOrigin(string Origin)
        {
            originEntry.Text = Origin;
        }

        void Handle_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            try
            {
                isDestinationfocused = (sender as SearchBar).IsFocused;
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }

        void Handle_Focused_1(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            try
            {
                isOriginFocuse = (sender as SearchBar).IsFocused;
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }

        #region decodePolygon
        private async Task<bool> LoadPolygon(IDictionary<string, object> Properties)
        {

            try
            {
                map.Polygons.Clear();

                string linestring = Properties[Constants.LINE_STRING] as string;

                //string temp = linestring.Replace("POLYGON", "").Replace("((", "").Replace("))", "");
                string[] PolygonList = linestring.Replace("MULTIPOLYGON", "").Replace("POLYGON", "").Replace("),(", ")),((").Split(new String[] { "),(" }, StringSplitOptions.None);// temp.Split("),(");

                foreach (var polygon in PolygonList)
                {
                    string[] points = polygon.Replace("(", string.Empty).Replace(")", string.Empty).Split(',');// temp.Split(new String[] { "),(" }, StringSplitOptions.None);// temp.Split("),(");

                    Xamarin.Forms.GoogleMaps.Polygon polyy = new Xamarin.Forms.GoogleMaps.Polygon();
                    polyy.FillColor = Color.Transparent;
                    polyy.StrokeColor = Color.Transparent;

                    Xamarin.Forms.GoogleMaps.Position centerPosition = GetCenter(points);

                    LatLongList.Clear();
                    bool addnamakwa = true;

                    foreach (string point in points)
                    {
                        var coords = GetCoords(point);
                        if (coords == null)
                            continue;

                        if (coords != null)
                        {
                            LatLong cord = new LatLong();
                            cord.lat = coords[0];
                            cord.longg = coords[1];
                            if (!LatLongList.Contains(cord))
                                LatLongList.Add(cord);
                        }
                        //poly.Positions.Add(new Xamarin.Forms.GoogleMaps.Position(coords[0], coords[1]));
                    }

                    foreach (var item in LatLongList)
                    {
                        if (addnamakwa)
                        {
                            //poly.Positions.Add(new Xamarin.Forms.GoogleMaps.Position(coords[0], coords[1]));
                            polyy.Positions.Add(new Xamarin.Forms.GoogleMaps.Position(item.lat, item.longg));
                            addnamakwa = false;
                        }
                        else
                        {
                            addnamakwa = true;
                        }
                    }

                    map.Polygons.Add(polyy);

                    //OnCenterMap(new Location(centerPosition.Latitude, centerPosition.Longitude));
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
                return false;
            }


        }


        private Xamarin.Forms.GoogleMaps.Position GetCenter(string[] points)
        {
            int coord_length = points.Length;
            double[] first = null;
            var i = 0;
            while (first == null)
            {
                first = GetCoords(points[i++]);
            }
            double[] last = null;
            i = coord_length / 2;
            while (last == null)
            {
                last = GetCoords(points[i++]);
            }

            return new Xamarin.Forms.GoogleMaps.Position((first[0] + last[0]) / 2, (first[1] + last[1]) / 2);

        }


        private double[] GetCoords(string point)
        {
            string[] coordinates = point.Trim().Replace("  ", " ").Split(' ');

            if (coordinates.Length != 2)
                return null;

            double[] coords = new double[2];
            //here coordinates[1]=for long 
            if (!double.TryParse(coordinates[1], out coords[0]))
                return null;
            if (!double.TryParse(coordinates[0], out coords[1]))
                return null;

            return coords;

        }


        async void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            try
            {

                var selectedItem = (e.SelectedItem as GooglePlaceAutoCompletePrediction);

                changeOrigin(selectedItem.Description);
                (this.BindingContext as MapPageViewModel).IsOirinPlacesList = false;



                var place = await (this.BindingContext as MapPageViewModel).googleMapsApi.GetPlaceDetails(selectedItem.PlaceId);
                if (place == null) return;
                if (place != null)
                {
                    (this.BindingContext as MapPageViewModel).OriginCoordinates = new Location(place.Latitude, place.Longitude);
                }

                if (e.SelectedItem == null)
                    return;

            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }

        async void Handle_ItemSelected1(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            try
            {
                var selectedItem = (e.SelectedItem as GooglePlaceAutoCompletePrediction);
                (this.BindingContext as MapPageViewModel).IsDestingationPlacesList = false;

                changeDestination(selectedItem.Description);
                var place = await (this.BindingContext as MapPageViewModel).googleMapsApi.GetPlaceDetails(selectedItem.PlaceId);
                if (place == null) return;
                if (place != null)
                {
                    (this.BindingContext as MapPageViewModel).DestinationCoordinates = new Location(place.Latitude, place.Longitude);
                    if ((this.BindingContext as MapPageViewModel).DestinationCoordinates != null && (this.BindingContext as MapPageViewModel).OriginCoordinates != null)
                    {
                        (this.BindingContext as MapPageViewModel).LoadRouteCommand.Execute(null);
                        var distanc = (this.BindingContext as MapPageViewModel).DestinationCoordinates.CalculateDistance((this.BindingContext as MapPageViewModel).OriginCoordinates, DistanceUnits.Kilometers);
                        var dis = distance((this.BindingContext as MapPageViewModel).DestinationCoordinates.Latitude, (this.BindingContext as MapPageViewModel).DestinationCoordinates.Longitude, (this.BindingContext as MapPageViewModel).OriginCoordinates.Latitude, (this.BindingContext as MapPageViewModel).OriginCoordinates.Longitude);

                    }
                }
                if (e.SelectedItem == null)
                    return;
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }


        private bool isPointInPolygon(Xamarin.Forms.GoogleMaps.Position tap, ObservableCollection<LatLong> vertices)
        {
            int intersectCount = 0;
            for (int j = 0; j < vertices.Count - 1; j++)
            {
                if (rayCastIntersect(tap, vertices[j], vertices[j + 1]))
                {
                    intersectCount++;
                }
            }

            return ((intersectCount % 2) == 1); // odd = inside, even = outside;
        }

        private bool rayCastIntersect(Xamarin.Forms.GoogleMaps.Position tap, LatLong vertA, LatLong vertB)
        {

            double aY = vertA.lat;
            double bY = vertB.lat;
            double aX = vertA.longg;
            double bX = vertB.longg;
            double pY = tap.Latitude;
            double pX = tap.Longitude;

            if ((aY > pY && bY > pY) || (aY < pY && bY < pY)
                    || (aX < pX && bX < pX))
            {
                return false; // a and b can't both be above or below pt.y, and a or
                              // b must be east of pt.x
            }

            double m = (aY - bY) / (aX - bX); // Rise over run
            double bee = (-aX) * m + aY; // y = mx + b
            double x = (pY - bee) / m; // algebra is neat!

            return x > pX;
        }

        public double distance(double lat1, double lon1, double lat2, double lon2)
        {
            try
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1))
                                * Math.Sin(deg2rad(lat2))
                                + Math.Cos(deg2rad(lat1))
                                * Math.Cos(deg2rad(lat2))
                                * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
                dist = dist * 60 * 1.1515;
                return (dist);
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
                return 0;
            }
        }

        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private double rad2deg(double rad)
        {
            return (rad * 180.0 / Math.PI);
        }
        #endregion
    }
}
