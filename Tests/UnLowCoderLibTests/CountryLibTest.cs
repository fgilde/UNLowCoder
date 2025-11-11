using Nager.Country;
using System.Globalization;
using UNLowCoder.Extensions;
using UNLowCoder.Lib;

namespace UnLowCoderTests
{
    public class CountryTest : BaseTest
    {
        public CountryTest(TestFixture fixture) : base(fixture)
        { }

        [Fact]
        public void Can_Resolve_By_Name()
        {
            var germany = UnLocodes.Countries.Get("DE");
            Assert.Equal("GERMANY", germany.CountryName);
        }

        [Fact]
        public void Can_Resolve_By_CultureInfo()
        {
            var cultureInfo = new CultureInfo("de-DE");
            var germany = UnLocodes.Countries.Get(cultureInfo);
            Verify(germany);
        }

        [Fact]
        public void Can_Resolve_By_RegionInfo()
        {
            var regionInfo = new RegionInfo("DE");
            var germany = UnLocodes.Countries.Get(regionInfo);
            Assert.Equal("GERMANY", germany.CountryName);
        }

        [Fact]
        public void Can_Filter_By_SubRegion()
        {
            var northAndEastEuropeCountries = Fixture.AllCountries.Filter(SubRegion.EasternEurope, SubRegion.NorthernEurope).ToList();
            Assert.Equal(28, northAndEastEuropeCountries.Count);
        }

        [Fact]
        public void Can_Filter_By_Alpha2Codes()
        {
            var germanyAndGb = Fixture.AllCountries.Filter(Alpha2Code.DE, Alpha2Code.GB).ToList();
            Assert.Equal(2, germanyAndGb.Count);
        }

        [Fact]
        public void Can_Create_CountryInfo()
        {
            var germanyCI = UnLocodes.Countries.DE.CountryInfo();
            var borderCountries = Fixture.AllCountries.Filter(germanyCI.BorderCountries).ToList();
        }
    }
}
