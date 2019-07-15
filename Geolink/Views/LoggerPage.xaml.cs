using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Geolink
{
    public partial class LoggerPage : ContentPage
    {
        public LoggerPage(ObservableCollection<string> LogList)
        {
            try
            {
                InitializeComponent();
                LogListview.ItemsSource = LogList;
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }

        void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            try
            {
                LogListview.SelectedItem = null;
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }
    }
}
