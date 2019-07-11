using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Plugin.Geolocator;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace Geolink
{
    public class MapPageViewModel : BaseViewModel
    {
        public IGoogleMapsApiService googleMapsApi = new GoogleMapsApiService();

        public PageStatusEnum PageStatusEnum { get; set; }

        public ICommand DrawRouteCommand { get; set; }
        public ICommand GetPlacesCommand { get; set; }
        public ICommand GetDestinataionPlacesCommand { get; set; }
        public ICommand GetUserLocationCommand { get; set; }
        public ICommand ChangePageStatusCommand { get; set; }
        public ICommand GetLocationNameCommand { get; set; }
        public ICommand CenterMapCommand { get; set; }
        public ICommand GetPlaceDetailCommand { get; set; }
        public ICommand LoadRouteCommand { get; set; }
        public ICommand CleanPolylineCommand { get; set; }
        public ICommand ChooseLocationCommand { get; set; }

        //public ObservableCollection<GooglePlaceAutoCompletePrediction> Places { get; set; }
        public ObservableCollection<GooglePlaceAutoCompletePrediction> RecentPlaces { get; set; }
        public GooglePlaceAutoCompletePrediction RecentPlace1 { get; set; }
        public GooglePlaceAutoCompletePrediction RecentPlace2 { get; set; }
        public ObservableCollection<PriceOption> PriceOptions { get; set; }
        public PriceOption PriceOptionSelected { get; set; }

        //public string PickupLocation { get; set; }

        public Location OriginCoordinates { get; set; }
        public Location DestinationCoordinates { get; set; }
        //IsOirinPlacesList
        //IsDestingationPlacesList


        bool isInitialLoading = true;
        public bool IsInitialLoading
        {
            get
            {
                return isInitialLoading;
            }
            set
            {
                isInitialLoading = value;
                OnPropertyChanged();
            }
        }

        bool isOirinPlacesList;
        public bool IsOirinPlacesList
        {
            get
            {
                return isOirinPlacesList;
            }
            set
            {
                isOirinPlacesList = value;
                OnPropertyChanged();
            }
        }


        bool isDestingationPlacesList;
        public bool IsDestingationPlacesList
        {
            get
            {
                return isDestingationPlacesList;
            }
            set
            {
                isDestingationPlacesList = value;
                OnPropertyChanged();
            }
        }

        string pickupLocation;
        public string PickupLocation
        {
            get
            {
                return pickupLocation;
            }
            set
            {
                pickupLocation = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(pickupLocation) && places == null)
                {
                    //if (pickupLocation.Length % 3 != 0)
                    //    return;
                    GetPlacesCommand.Execute(pickupLocation);
                    places = null;
                }
                else
                {
                    places = null;
                    CleanPolylineCommand.Execute(null);
                    IsOirinPlacesList = false;
                }
            }
        }

        string _destinationLocation;
        public string DestinationLocation
        {
            get
            {
                return _destinationLocation;
            }
            set
            {
                _destinationLocation = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(_destinationLocation) && places == null)
                {
                    //if (_destinationLocation.Length % 3 != 0)
                    //    return;
                    GetDestinataionPlacesCommand.Execute(_destinationLocation);
                    places = null;
                }
                else
                {
                    places = null;
                    CleanPolylineCommand.Execute(null);
                    IsDestingationPlacesList = false;
                }
            }
        }

        GooglePlaceAutoCompletePrediction _placeSelected;
        public GooglePlaceAutoCompletePrediction PlaceSelected
        {
            get
            {
                return _placeSelected;
            }
            set
            {
                _placeSelected = value;
                if (_placeSelected != null)
                {
                    GetPlaceDetailCommand.Execute(_placeSelected);
                }
            }
        }

        ObservableCollection<GooglePlaceAutoCompletePrediction> places;
        public ObservableCollection<GooglePlaceAutoCompletePrediction> Places
        {
            get
            {
                return places;
            }
            set
            {
                places = value;
                OnPropertyChanged();
            }
        }

        public MapPageViewModel()
        {
            LoadRouteCommand = new Command(async () => await LoadRoute());
            GetPlaceDetailCommand = new Command<GooglePlaceAutoCompletePrediction>(async (param) => await GetPlacesDetail(param));
            GetPlacesCommand = new Command<string>(async (param) => await GetPlacesByName(param));
            GetDestinataionPlacesCommand = new Command<string>(async (param) => await GetDestinationPlacesByName(param));
            GetUserLocationCommand = new Command(async () => await GetActualUserLocation());
            GetLocationNameCommand = new Command<Position>(async (param) => await GetLocationName(param));
            ChangePageStatusCommand = new Command<PageStatusEnum>((param) =>
              {
                  PageStatusEnum = param;

                  if (PageStatusEnum == PageStatusEnum.Default)
                  {
                      CleanPolylineCommand.Execute(null);
                      GetUserLocationCommand.Execute(null);
                      DestinationLocation = string.Empty;
                  }
                  else if (PageStatusEnum == PageStatusEnum.Searching)
                  {
                      Places = new ObservableCollection<GooglePlaceAutoCompletePrediction>(RecentPlaces);
                  }
              });

            ChooseLocationCommand = new Command<Position>((param) =>
            {
                if (PageStatusEnum == PageStatusEnum.Searching)
                {
                    //GetLocationNameCommand.Execute(param);
                }
            });

            //FillRecentPlacesList();
            //FillPriceOptions();
            GetUserLocationCommand.Execute(null);
        }

        async Task GetActualUserLocation()
        {
            try
            {
                await Task.Yield();
                var locator = CrossGeolocator.Current;
                //if (locator.IsGeolocationAvailable)
                //{
                //    var action = App.Current.MainPage.DisplayAlert("price", "Please enable location", "ok", "cancel");

                //    return;
                //}
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5000));
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    OriginCoordinates = location;
                    CenterMapCommand.Execute(location);
                    GetLocationNameCommand.Execute(new Position(location.Latitude, location.Longitude));
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync("Error", "Unable to get actual location", "Ok");
            }
        }

        public async Task GetDestinationName(Position position)
        {
            try
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(position.Latitude, position.Longitude);
                DestinationLocation = placemarks?.FirstOrDefault()?.FeatureName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        //Get place 
        public async Task GetLocationName(Position position)
        {
            try
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(position.Latitude, position.Longitude);
                PickupLocation = placemarks?.FirstOrDefault()?.FeatureName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public async Task GetPlacesByName(string placeText)
        {
            //if (!isInitialLoading)
            //    return;

            var places = await googleMapsApi.GetPlaces(placeText);
            if (places == null) return;
            var placeResult = places.AutoCompletePlaces;
            if (placeResult != null && placeResult.Count > 0)
            {
                Places = new ObservableCollection<GooglePlaceAutoCompletePrediction>(placeResult);
                //if (!isInitialLoading)
                //{
                //OriginCoordinates = Places?.FirstOrDefault()?.StructuredFormatting.MainText;
                IsOirinPlacesList = true;
                //}
                //else
                //    isInitialLoading = false;

            }
        }


        public async Task GetDestinationPlacesByName(string placeText)
        {
            var places = await googleMapsApi.GetPlaces(placeText);
            if (places == null) return;
            var placeResult = places.AutoCompletePlaces;
            if (placeResult != null && placeResult.Count > 0)
            {
                Places = new ObservableCollection<GooglePlaceAutoCompletePrediction>(placeResult);
                IsDestingationPlacesList = true;
            }
        }

        public async Task GetPlacesDetail(GooglePlaceAutoCompletePrediction placeA)
        {
            var place = await googleMapsApi.GetPlaceDetails(placeA.PlaceId);
            if (places == null) return;
            if (place != null)
            {
                DestinationCoordinates = new Location(place.Latitude, place.Longitude);
                //LoadRouteCommand.Execute(null);
                //RecentPlaces.Add(placeA);
            }
        }

        public async Task LoadRoute()
        {
            if (OriginCoordinates == null)
                return;

            if (PickupLocation == null || _destinationLocation == null)
                return;
            //if (!IsDestingationPlacesList)

            ChangePageStatusCommand.Execute(PageStatusEnum.ShowingRoute);

            var googleDirection = await googleMapsApi.GetDirections($"{OriginCoordinates.Latitude}", $"{OriginCoordinates.Longitude}", $"{DestinationCoordinates.Latitude}", $"{DestinationCoordinates.Longitude}");
            if (googleDirection == null) return;

            if (googleDirection.Routes != null && googleDirection.Routes.Count > 0)
            {


                var positions = (Enumerable.ToList(PolylineHelper.Decode(googleDirection.Routes.First().OverviewPolyline.Points)));

                DrawRouteCommand.Execute(positions);
            }
            else
            {
                ChangePageStatusCommand.Execute(PageStatusEnum.Default);
                await UserDialogs.Instance.AlertAsync(":(", "No route found", "Ok");

            }
        }

        void FillRecentPlacesList()
        {
            RecentPlaces = new ObservableCollection<GooglePlaceAutoCompletePrediction>()
            {
                {new GooglePlaceAutoCompletePrediction(){ PlaceId="ChIJq0wAE_CJr44RtWSsTkp4ZEM", StructuredFormatting=new StructuredFormatting(){ MainText="Random Place", SecondaryText="Paseo de los locutores #32" } } },
                {new GooglePlaceAutoCompletePrediction(){ PlaceId="ChIJq0wAE_CJr44RtWSsTkp4ZEM", StructuredFormatting=new StructuredFormatting(){ MainText="Green Tower", SecondaryText="Ensanche Naco #4343, Green 232" } } },
                {new GooglePlaceAutoCompletePrediction(){ PlaceId="ChIJm02ImNyJr44RNs73uor8pFU", StructuredFormatting=new StructuredFormatting(){ MainText="Tienda Aurora", SecondaryText="Rafael Augusto Sanchez" } } },
            };

            RecentPlace1 = RecentPlaces[0];
            RecentPlace2 = RecentPlaces[1];

        }

        void FillPriceOptions()
        {
            PriceOptions = new ObservableCollection<PriceOption>()
            {
                {new PriceOption(){ Tag="xUBERX", Category="Economy", CategoryDescription="Affortable, everyday rides", PriceDetails=new System.Collections.Generic.List<PriceDetail>(){
                    { new PriceDetail(){ Type="xUber X", Price=332, ArrivalETA="12:00pm", Icon="https://d1ic4altzx8ueg.cloudfront.net/finder-au/wp-uploads/2019/01/uber-melbourne-new-1.jpg" } },
                  { new PriceDetail(){ Type="xUber Black", Price=150, ArrivalETA="12:40pm", Icon="https://d1ic4altzx8ueg.cloudfront.net/finder-au/wp-uploads/2019/01/uber-melbourne-new-2.jpg" } }}
                 } },
                {new PriceOption(){Tag="xUBERXL", Category="Extra Seats", CategoryDescription="Affortable rides for group up to 6" ,  PriceDetails=new System.Collections.Generic.List<PriceDetail>(){
                    { new PriceDetail(){ Type="xUber XL", Price=332, ArrivalETA="12:00pm", Icon="https://d1ic4altzx8ueg.cloudfront.net/finder-au/wp-uploads/2019/01/uber-melbourne-new-2.jpg" } }
                  } } }
            };
            PriceOptionSelected = PriceOptions.First();

        }
    }
}
