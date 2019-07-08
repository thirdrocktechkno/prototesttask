using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Geolink
{
    public partial class MapPage : ContentPage
    {
        public MapPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }
    }
}
