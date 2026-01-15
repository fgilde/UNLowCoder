using System.IO.Compression;
using System.Text;
using UNLowCoder;
using UNLowCoder.Core;
using UNLowCoder.Core.Data;

namespace UnLowCoderTests;

public class CoreTests(TestFixture fixture) : BaseTest(fixture)
{

    [Fact]
    public void CoreParseCheck()
    {
        string zip = TestPathTo("loc241csv.zip");
        string zip2 = TestPathTo("loc242csv.zip");

        var res = UnLocodeParser.ParseZipArchive(zip, ParseMode.OnlyNewest);
        var res2 = UnLocodeParser.ParseZipArchive(zip2, ParseMode.OnlyNewest);

        Assert.Equal(249, res.Count);
        Assert.Equal(res.Count, res2.Count);

        var a1 = res.SelectMany(country => country.Locations).Where(l => l.Function.HasFlag(LocationFunction.RailTerminal) && l.Function.HasFlag(LocationFunction.Airport)).ToList();
        Assert.Equal(896, a1.Count);

        var a2 = res2.SelectMany(country => country.Locations).Where(l => l.Function.HasFlag(LocationFunction.RailTerminal) && l.Function.HasFlag(LocationFunction.Airport)).ToList();
        Assert.Equal(905, a2.Count);

    }

    [Fact]
    public void Test_Enrich_Location()
    {
        string zip = TestPathTo("loc241csv.zip");
        var res = UnLocodeParser.ParseZipArchive(zip, ParseMode.OnlyNewest);

        UnLocodeLocation[] all = res.SelectMany(c => c.Locations).ToArray();
        UnLocodeLocation[] defaultWithoutLocations = all.Where(l => l?.Coordinates == null).ToArray();


        string zip2 = "C:\\dev\\privat\\github\\UNLowCoder\\UNLowCoder.SourceGen\\code-list-improved.zip";
        var improved = UnLocodeParser.ParseZipArchive(zip2, ParseMode.OnlyNewest);
        var res2 = UnLocodeEnrichment.EnrichCoordinates(res, improved);
        
        UnLocodeLocation[] allLocations = res2.SelectMany(c => c.Locations).ToArray();
        UnLocodeLocation[] withoutLocations = allLocations.Where(l => l?.Coordinates == null).ToArray();

    }
}