using Nager.Country;
using System.IO;
using System.Net.Http;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using UNLowCoder.Core.Data;
using UNLowCoder.Extensions.Classes;

namespace UNLowCoder.Extensions
{
    public static class UnLocodeCountryExtensions
    {
        public static ICountryInfo? CountryInfo(this UnLocodeCountry country)
        {
            try
            {                
                ICountryProvider countryProvider = new CountryProvider();
                return countryProvider.GetCountry(country.CountryCode);
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static string Slug(this UnLocodeCountry country)
        {

            if (country.RegionInfo == null) return null;

            var englishName = country.RegionInfo.EnglishName;

            if (string.IsNullOrWhiteSpace(englishName))
                return null;

            englishName = englishName.ToLowerInvariant();
            englishName = englishName.Replace(' ', '-');

            englishName = Regex.Replace(englishName, @"[^a-z\-]", "");

            return englishName;

        }

        public static string PbfUrl(this UnLocodeCountry country) => GeoFabrik.PbfUrl(country.CountryInfo().OsmRegionName(), country.Slug());

        /// <summary>
        /// Lädt (falls nötig) die PBF-Datei herunter und baut eine RouterDb (Itinero)
        /// für dieses Land. Als einfaches Beispiel.
        /// </summary>
        /// <param name="country">Das Land</param>
        /// <param name="outputFolder">Wohin soll die Datei gespeichert werden?</param>
        /// <param name="forceDownload">true, wenn PBF bei jedem Aufruf neu geladen werden soll</param>
        /// <param name="forceRebuild">true, wenn RouterDb bei jedem Aufruf neu gebaut werden soll</param>
        public static async Task<RouterDb> BuildRouterDbAsync(
            this UnLocodeCountry country,
            string outputFolder,
            bool forceDownload = false,
            bool forceRebuild = false)
        {
            Directory.CreateDirectory(outputFolder);

            var pbfUrl = country.PbfUrl();
            if (string.IsNullOrEmpty(pbfUrl))
                throw new InvalidOperationException($"Für {country.CountryCode} konnte keine Geofabrik-URL ermittelt werden.");

            // Beispiel: DE -> "germany-latest.osm.pbf"
            // => Lokaler Pfad: "C:\...\outputFolder\DE.osm.pbf"
            var pbfLocalFile = Path.Combine(outputFolder, $"{country.CountryCode}.osm.pbf");
            var routerDbFile = Path.Combine(outputFolder, $"{country.CountryCode}.routerdb");

            // 1) PBF-Datei herunterladen (sofern nicht vorhanden oder forceDownload = true)
            if (forceDownload || !File.Exists(pbfLocalFile))
            {
                // httpClient mit Timeout (z.B. 2 Stunden)
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromHours(2) };

                //using var contentStream = await httpClient.GetStreamInChunksAsync(pbfUrl, 1024 * 1024);
                //using var contentStream = await httpClient.GetStreamInTaskChunksAsync(pbfUrl, 10);
                using var contentStream = await httpClient.GetStreamAsync(pbfUrl);
                using var fileStream = File.Create(pbfLocalFile);
                await contentStream.CopyToAsync(fileStream);
            }

            // 2) RouterDb laden oder neu bauen
            if (!forceRebuild && File.Exists(routerDbFile))
            {
                // Falls RouterDb bereits existiert und kein forceRebuild => deserialisieren
                using var stream = File.OpenRead(routerDbFile);
                return RouterDb.Deserialize(stream);
            }

            // Neu erstellen
            var routerDb = new RouterDb();
            using var pbfStream = File.OpenRead(pbfLocalFile);

            // Hier z.B. nur für Auto "vehicle = Car"
            // Du könntest das Profil anpassen (Fahrrad, Fußgänger, etc.)
            var vehicle = Vehicle.Car;

            // Itinero einlesen
            routerDb.LoadOsmData(pbfStream, vehicle);

            // Abspeichern in einer Datei, damit wir nicht jedes Mal neu bauen müssen
            using var writeStream = File.Open(routerDbFile, FileMode.Create);
            routerDb.Serialize(writeStream);

            return routerDb;
        }

        /// <summary>
        /// Beispiel-Shortcut: Baut einen Router (Itinero) aus BuildRouterDbAsync.
        /// </summary>
        public static async Task<Router> BuildRouterAsync(
            this UnLocodeCountry country,
            string outputFolder,
            bool forceDownload = false,
            bool forceRebuild = false)
        {
            var routerDb = await country.BuildRouterDbAsync(outputFolder, forceDownload, forceRebuild);
            return new Router(routerDb);
        }

    }
}
