using Itinero;
using System.IO;
using System.Linq;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;

namespace UNLowCoder.Extensions.Classes;

public static class GeoFabrik
{
    private const string URL = "https://download.geofabrik.de/{0}-latest.osm.pbf";
    public static string PbfUrl(string continent, string country) => string.Format(URL, $"{continent}/{country}");
    public static string PbfUrl(string continent) => string.Format(URL, $"{continent}");

    internal static async Task CreateRouteDb(string dir)
    {
        var regions = CountryInfoExtensions.AllAvailableOsmRegions().Select(GeoFabrik.PbfUrl).ToArray();
        var tasks = new List<Task>();
        string outputFolder = "ROUTER";
        Directory.CreateDirectory(outputFolder);
        foreach (var pbfUrl in regions)
        {
            var pbfLocalFile = Path.Combine(outputFolder, pbfUrl.Split('/').Last());
            var routerDbFile = pbfLocalFile + ".db";
            var pbf = pbfLocalFile;
            var db = routerDbFile;
            if (!File.Exists(pbf))
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromHours(2) };
                using var contentStream = await httpClient.GetStreamAsync(pbfUrl);
                using var fileStream = File.Create(pbf);
                await contentStream.CopyToAsync(fileStream);
            }

            if (!File.Exists(db))
            {
                try
                {
                    var routerDb = new RouterDb();
                    using var pbfStream = File.OpenRead(pbf);

                    // Itinero einlesen
                    routerDb.LoadOsmData(pbfStream, Vehicle.Car);

                    // Abspeichern in einer Datei, damit wir nicht jedes Mal neu bauen müssen
                    using var writeStream = File.Open(db, FileMode.Create);
                    routerDb.Serialize(writeStream);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error while building router for {pbfUrl} {e.Message}");
                }
            }
            tasks.Add(Task.Run(async () =>
            {
                //var pbf = pbfLocalFile;
                //var db = routerDbFile;
                //if(!File.Exists(pbf))
                //{
                //    using var httpClient = new HttpClient { Timeout = TimeSpan.FromHours(2) };
                //    using var contentStream = await httpClient.GetStreamAsync(pbfUrl);
                //    using var fileStream = File.Create(pbf);
                //    await contentStream.CopyToAsync(fileStream);
                //}

                //if (!File.Exists(db))
                //{
                //    try
                //    {
                //        var routerDb = new RouterDb();
                //        using var pbfStream = File.OpenRead(pbf);

                //        // Itinero einlesen
                //        routerDb.LoadOsmData(pbfStream, Vehicle.Car);

                //        // Abspeichern in einer Datei, damit wir nicht jedes Mal neu bauen müssen
                //        using var writeStream = File.Open(db, FileMode.Create);
                //        routerDb.Serialize(writeStream);
                //    }
                //    catch (Exception e)
                //    {
                //        Console.WriteLine($"Error while building router for {pbfUrl} {e.Message}");
                //    }
                //}


            }));

        }

        await Task.WhenAll(tasks);
    }
}