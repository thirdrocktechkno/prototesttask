using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Geolink
{
    public partial class SearchContentView : ListView
    {
        public SearchContentView()
        {
            InitializeComponent();
        }

        void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            try
            {
                MapPage page = (this.Parent.Parent.Parent as MapPage);
                MapPageViewModel mapPageViewModel = (page?.BindingContext as MapPageViewModel);

                var selectedItem = (e.SelectedItem as GooglePlaceAutoCompletePrediction);
                if (page.isDestinationfocused)
                {
                    page.changeDestination(selectedItem.Description);
                }

                if (page.isOriginFocuse)
                {
                    page.changeOrigin(selectedItem.Description);
                }

                if (e.SelectedItem == null)
                    return;

                this.SelectedItem = null;
            }
            catch (Exception ex)
            {
                Logger.SendErrorLog(ex);
            }
        }
    }
}
