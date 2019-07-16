using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;

namespace Geolink
{
    public class GoogleMapsApiService : IGoogleMapsApiService
    {
        static string _googleMapsKey;

        private const string ApiBaseAddress = "https://maps.googleapis.com/maps/";
        private HttpClient CreateClient()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseAddress)
            };

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }
        public static void Initialize(string googleMapsKey)
        {
            _googleMapsKey = googleMapsKey;
        }

        public async Task<GoogleDirection> GetDirections(string originLatitude, string originLongitude, string destinationLatitude, string destinationLongitude)
        {
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {

                var action = App.Current.MainPage.DisplayAlert("price", "Please check your network", "ok", "cancel");
                return null;
            }

            originLatitude = originLatitude.Replace(",", ".");
            originLongitude = originLongitude.Replace(",", ".");
            destinationLatitude = destinationLatitude.Replace(",", ".");
            destinationLongitude = destinationLongitude.Replace(",", ".");

            GoogleDirection googleDirection = new GoogleDirection();

            using (var httpClient = CreateClient())
            {
                string str = "GetDirections Request : " + ApiBaseAddress + $"api/directions/json?region=pt-PT&origin={originLatitude},{originLongitude}&destination={destinationLatitude},{destinationLongitude}&key={_googleMapsKey}";
                App.LogList.Add(str.Trim());

                var response = await httpClient.GetAsync($"api/directions/json?region=pt-PT&origin={originLatitude},{originLongitude}&destination={destinationLatitude},{destinationLongitude}&key={_googleMapsKey}").ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    App.LogList.Add("GetDirections Response : " + json.Trim());

                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        googleDirection = await Task.Run(() =>
                           JsonConvert.DeserializeObject<GoogleDirection>(json)
                        ).ConfigureAwait(false);

                    }
                }
                else
                    App.LogList.Add("GetDirections Response : False");

            }

            return googleDirection;
        }

        public async Task<GooglePlaceAutoCompleteResult> GetPlaces(string text)
        {
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {
                var action = App.Current.MainPage.DisplayAlert("price", "Please check your network", "ok", "cancel");
                return null;
            }

            GooglePlaceAutoCompleteResult results = null;

            using (var httpClient = CreateClient())
            {
                string str = "GetPlaces Request : " + ApiBaseAddress + $"api/place/autocomplete/json?input={Uri.EscapeUriString(text)}&key={_googleMapsKey}";
                App.LogList.Add(str.Trim());

                var response = await httpClient.GetAsync($"api/place/autocomplete/json?input={Uri.EscapeUriString(text)}&key={_googleMapsKey}").ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    App.LogList.Add("GetPlaces Response : " + json.Trim());

                    if (!string.IsNullOrWhiteSpace(json) && json != "ERROR")
                    {
                        results = await Task.Run(() =>
                           JsonConvert.DeserializeObject<GooglePlaceAutoCompleteResult>(json)
                        ).ConfigureAwait(false);

                    }
                }
                else
                    App.LogList.Add("GetPlaces Response : False");
            }

            return results;
        }

        public async Task<GooglePlace> GetPlaceDetails(string placeId)
        {

            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {
                var action = App.Current.MainPage.DisplayAlert("price", "Please check your network", "ok", "cancel");
                return null;
            }


            GooglePlace result = null;
            using (var httpClient = CreateClient())
            {
                string str = "GetPlaceDetails Request : " + ApiBaseAddress + $"api/place/details/json?placeid={Uri.EscapeUriString(placeId)}&key={_googleMapsKey}";
                App.LogList.Add(str.Trim());

                var response = await httpClient.GetAsync($"api/place/details/json?placeid={Uri.EscapeUriString(placeId)}&key={_googleMapsKey}").ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    App.LogList.Add("GetPlaceDetails Response : " + json.Trim());

                    if (!string.IsNullOrWhiteSpace(json) && json != "ERROR")
                    {
                        result = new GooglePlace(JObject.Parse(json));
                    }
                }
                else
                    App.LogList.Add("GetPlaceDetails Response : False");
            }

            return result;
        }
    }
}
