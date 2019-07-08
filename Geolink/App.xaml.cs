using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Geolink
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();
                MainPage = new MapPage();
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
