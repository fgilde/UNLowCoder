using System.IO.Compression;
using System.Text;
using UNLowCoder;
using UNLowCoder.Core; 
using UNLowCoder.Core.Data;      

public class UnLocodeParserTests
{
    // --- Helpers -------------------------------------------------------------

    private static MemoryStream CreateZip(params (string name, string content, Encoding enc)[] entries)
    {
        var ms = new MemoryStream();
        using (var za = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content, enc) in entries)
            {
                var e = za.CreateEntry(name);
                using var s = e.Open();
                var bytes = (enc ?? Encoding.UTF8).GetBytes(content);
                s.Write(bytes, 0, bytes.Length);
            }
        }
        ms.Position = 0;
        return ms;
    }

    private static List<UnLocodeCountry> ParseZip(MemoryStream zip, ParseMode mode = ParseMode.AllEntries)
        => UnLocodeParser.ParseZipStream(zip, mode);

    // Kombinierte CodeList.csv (24-1) – Reihenfolge: Status (col6), Function (col7)
    private const string HeaderlessCombinedRow =
          //  0   1    2      3           4         5     6      7         8     9    10             11
          //chg, Ctry, Loc,   Name,       NameWo,   Sub,  Status,Function, Date, IATA,Coords,        Remark
          "=,  DE,   XXX,   Berlin Hbf, Berlin,   BE,   AI,    12-4-6-,  2412, BER, 5230N 01324E, Test";

    // Part-Dateien (24-1 & 24-2) – Reihenfolge: Function (col6), Status (col7)
    private const string HeaderlessPartRow =
          //  0   1    2      3           4         5     6        7     8     9    10             11
          //chg, Ctry, Loc,   Name,       NameWo,   Sub,  Function,Status,Date, IATA,Coords,        Remark
          "=,  DE,   XXX,   Berlin Hbf, Berlin,   BE,   12-4-6-, AI,   2412, BER, 5230N 01324E, Test";

    // Function mit Border 'B' als 8. Zeichen – soll ignoriert werden
    private const string FunctionWithBorder =
          "=,  DE,   XYA,   Rail+Air,    RailAir,  BE,   12-4-6-B, AI,  2412, ,    5230N 01324E,  Border";

    // Ältere & neuere Duplikate + Deletion
    private const string OlderRow =
          "=,  DE,   DUP,   Old,         Old,      BE,   12-----,  AI,  2406, ,    ,              Old";
    private const string NewerRow =
          "=,  DE,   DUP,   New,         New,      BE,   12-----,  AI,  2412, ,    ,              New";
    private const string DeletedNewest =
          "X,  DE,   DEL,   Gone,        Gone,     BE,   12-----,  AI,  2412, ,    ,              Deleted";

    // Country-Name-Zeile (.Germany)
    private const string CountryNameLine =
          // change empty, country code DE, location code empty, name prefix "." = Ländername
          ",   DE,  ,      .Germany,     ,         ,     ,        ,     ,     ,    ,              ";

    private const string SubdivisionRows =
          "DE,BE,Berlin,State\nDE,BY,Bayern,State";

    // --- Tests ---------------------------------------------------------------

    [Fact]
    public void Parses_Function_From_Combined_And_Parts_Correctly()
    {
        // Combined + Part: identischer Standort DE XXX – Combined hat Status/Function,
        // Part hat Function/Status (vertauscht).
        using var zip = CreateZip(
            ("CodeList.csv", HeaderlessCombinedRow, null),
            ("CodeListPart1.csv", HeaderlessPartRow, null));

        var countries = ParseZip(zip);
        var loc = countries.SelectMany(c => c.Locations).Single(l => l.LocationCode == "XXX");

        // Rail (pos 2) & Airport (pos 4) -> beide Flags gesetzt
        Assert.True(loc.Function.HasFlag(LocationFunction.RailTerminal));
        Assert.True(loc.Function.HasFlag(LocationFunction.Airport));
    }

    [Fact]
    public void Skips_Parts_When_Combined_Exists_To_Avoid_Double_Count()
    {
        using var zip = CreateZip(
            ("CodeList.csv", HeaderlessCombinedRow, null),
            ("CodeListPart1.csv", HeaderlessPartRow, null));

        // Parser sollte die Part-Datei überspringen, weil Combined vorhanden ist
        var countries = ParseZip(zip);
        var all = countries.SelectMany(c => c.Locations).Where(l => l.CountryCode == "DE" && l.LocationCode == "XXX").ToList();

        Assert.Single(all); // keine Doppelung
    }

    [Fact]
    public void Uses_OnlyNewest_And_Drops_MarkedForDeletion()
    {
        using var zip = CreateZip(
            ("CodeListPart1.csv", OlderRow, null),
            ("CodeListPart2.csv", NewerRow, null),
            ("CodeListPart3.csv", DeletedNewest, null));

        // OnlyNewest: für DUP bleibt nur NewerRow; DEL ist X-marked => raus
        var countries = ParseZip(zip, ParseMode.OnlyNewest);
        var all = countries.SelectMany(c => c.Locations).ToList();

        // DUP bleibt, DEL fällt raus
        Assert.Contains(all, l => l.LocationCode == "DUP" && l.Name.Contains("New"));
        Assert.DoesNotContain(all, l => l.LocationCode == "DEL");
    }

    [Fact]
    public void Function_Ignores_Border_Flag_And_Sets_Bits()
    {
        using var zip = CreateZip(("CodeListPart1.csv", FunctionWithBorder, null));

        var loc = ParseZip(zip).SelectMany(c => c.Locations).Single();

        Assert.True(loc.Function.HasFlag(LocationFunction.Seaport));
        Assert.True(loc.Function.HasFlag(LocationFunction.RailTerminal));
        Assert.True(loc.Function.HasFlag(LocationFunction.Airport));
        Assert.True(loc.Function.HasFlag(LocationFunction.ICD));
        // 8. Zeichen 'B' soll keinen zusätzlichen Bit setzen
        Assert.False(loc.Function.HasFlag(LocationFunction.FixedTransport)); // in unserem String nicht gesetzt
    }

    [Fact]
    public void Parses_Coordinates_And_Date()
    {
        using var zip = CreateZip(("CodeListPart1.csv", HeaderlessPartRow, null));

        var loc = ParseZip(zip).SelectMany(c => c.Locations).Single();

        // Date 2412 => 2024-12-01
        Assert.Equal(new DateTime(2024, 12, 1), loc.LastUpdateDate!.Value.Date);

        // "5230N 01324E" => ~52.5, 13.4
        Assert.NotNull(loc.Coordinates);
        Assert.InRange(loc.Coordinates!.Latitude, 52.49, 52.51);
        Assert.InRange(loc.Coordinates!.Longitude, 13.39, 13.41);
    }

    [Fact]
    public void Parses_Country_Name_From_Dot_Prefix()
    {
        using var zip = CreateZip(
            ("CodeListPart1.csv", CountryNameLine, null),
            ("CodeListPart2.csv", HeaderlessPartRow, null));

        var de = ParseZip(zip).Single(c => c.CountryCode == "DE");

        Assert.Equal("Germany", de.CountryName);
    }

    [Fact]
    public void Parses_Subdivisions_And_Attaches_To_Country()
    {
        using var zip = CreateZip(
            ("Subdivisions.csv", SubdivisionRows, null),
            ("CodeListPart1.csv", HeaderlessPartRow, null));

        var de = ParseZip(zip).Single(c => c.CountryCode == "DE");
        Assert.Contains(de.Subdivisions, s => s.SubdivisionCode == "BE" && s.Name.Contains("Berlin"));
        Assert.Contains(de.Locations, l => l.SubdivisionCode == "BE");
    }

    [Fact]
    public void Detects_Latin1_Encoding_For_Umlauts()
    {
        // Name enthält 'München' in ISO-8859-1 (Latin-1)
        var latin1 = Encoding.GetEncoding("ISO-8859-1");
        var row = "=,  DE,   MUC,   München,      Muenchen, BY,   1-----B, AI,  2410, MUC,  4821N 01147E,  Umlaut";
        using var zip = CreateZip(("CodeListPart1.csv", row, latin1));

        var muc = ParseZip(zip).SelectMany(c => c.Locations).Single(l => l.LocationCode == "MUC");
        Assert.Contains("München", muc.Name); // korrekt erkannt
    }

    [Fact]
    public void Rail_And_Airport_Filter_Matches_Expected()
    {
        // Zwei Standorte: einer Rail+Airport, einer nur Rail
        var both = "=,DE,RAA,RailAir,RailAir,BE,12-4-6-,AI,2412,,,";
        var rail = "=,DE,RAL,RailOnly,RailOnly,BE,2------,AI,2412,,,";
        using var zip = CreateZip(("CodeListPart1.csv", both + "\n" + rail, null));

        var res = ParseZip(zip, ParseMode.OnlyNewest);
        var hits = res.SelectMany(c => c.Locations)
                      .Where(l => l.Function.HasFlag(LocationFunction.RailTerminal)
                               && l.Function.HasFlag(LocationFunction.Airport))
                      .Select(l => l.LocationCode)
                      .ToList();

        Assert.Single(hits);
        Assert.Contains("RAA", hits);
    }
}
