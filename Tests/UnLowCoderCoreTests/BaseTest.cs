using Argon;
using UNLowCoder.Core.Data;


namespace UnLowCoderTests;

public abstract class BaseTest : IClassFixture<TestFixture>
{
    protected string SolutionRoot => Path.Combine(AppContext.BaseDirectory, "../../../../../");
    protected string TestDataPath => Path.Combine(SolutionRoot, "UNLowCoder.Lib", "Sources");
    protected string TestPathTo(params string[] paths) => Path.Combine(TestDataPath, Path.Combine(paths));

    protected readonly TestFixture Fixture;
    protected VerifySettings VerifySettings = new VerifySettings();
    protected void Verify<T>(T item)
    {
        JsonSerializerSettings? settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        var toVerify = JsonConvert.SerializeObject(item, settings);
        Verifier.Verify(toVerify, VerifySettings);
    }
    protected BaseTest(TestFixture fixture)
    {
        Fixture = fixture;
        VerifySettings.UseDirectory(".snapshots");
        VerifySettings.ScrubInlineGuids();
    }
}

public class TestFixture : IDisposable
{
    public IReadOnlyList<UnLocodeCountry> AllCountries { get; private set; }

    public TestFixture()
    {
        //AllCountries = UnLocodes.Countries.All;
        VerifierSettings.ScrubInlineGuids();
    }

    public void Dispose()
    {
    }
}