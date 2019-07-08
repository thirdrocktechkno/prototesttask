using System;
using Newtonsoft.Json;

namespace Geolink
{
    public static class Logger
    {
        public static void SendErrorLog(this Exception ex)
        {
            Console.Write(ex);
            //Insights.Report(ex, Insights.Severity.Error);
        }
    }
}