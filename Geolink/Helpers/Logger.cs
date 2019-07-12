using System;
using Acr.UserDialogs;
using Newtonsoft.Json;

namespace Geolink
{
    public static class Logger
    {
        public static void SendErrorLog(this Exception ex)
        {
            UserDialogs.Instance.AlertAsync("Error", JsonConvert.SerializeObject(ex), "Ok");
            Console.Write(ex);
            //Insights.Report(ex, Insights.Severity.Error);
        }
    }
}