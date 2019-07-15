using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Geolink
{
    public partial class App : Application
    {
        public static INavigation Navigation { get; set; }

        public static ObservableCollection<string> LogList = new ObservableCollection<string>();
        
        public App()
        {
            try
            {
                InitializeComponent();
                if (!Application.Current.Properties.ContainsKey(Constants.LineString))
                    Application.Current.Properties[Constants.LineString] = "POLYGON((-8.59418349674972 41.1383506797128,-8.61560349288011 41.1389019841448,-8.67074812896517 41.1434744495776,-8.67905202242122 41.1449838609599,-8.67927438576518 41.1451299199957,-8.69051961393614 41.1667491884192,-8.69128901091482 41.1687515832633,-8.69129406942044 41.1687718202548,-8.6901233398917 41.1733985483824,-8.66638025932549 41.1791950462077,-8.64263275539779 41.1843186499419,-8.60475022412726 41.1859199810206,-8.60415557268555 41.1859353051989,-8.60352615847566 41.1858564905415,-8.57618203495341 41.1809534888858,-8.57596086663742 41.1808879776888,-8.56918812805673 41.1782338392238,-8.5690029395442 41.1781107344154,-8.56863743584061 41.1778608190979,-8.56838121049044 41.1776820677363,-8.56783274898752 41.1772845426276,-8.56630188826643 41.1761404097255,-8.56619263815834 41.1760550093552,-8.56610877045314 41.1759695143603,-8.55337925107515 41.1600578943756,-8.55261345505833 41.158207085713,-8.55473745611279 41.1532077014578,-8.55494508426352 41.1529458174769,-8.56368877972558 41.1463677636723,-8.57576315330342 41.1399578511665,-8.59418349674972 41.1383506797128))";

                GoogleMapsApiService.Initialize(Constants.GoogleMapsApiKey);
                MainPage = new NavigationPage(new MapPage());
                Navigation = MainPage.Navigation;
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
