using FxRateService.Infrastructure.Providers.Ecb;
using FxRateService.UnitTests.Fixtures;

namespace FxRateService.UnitTests.Providers;

public class EcbRateParserTests
{
    private static string DailyXml => FixtureLoader.Read("ecb-daily-2026-08-26.xml");

    [Fact]
    public void Parse_cita_datum_objave()
    {
        var snapshot = EcbRateParser.Parse(DailyXml);

        Assert.Equal(new DateOnly(2026, 8, 26), snapshot.AsOf);
    }

    [Fact]
    public void Parse_cita_sve_kurseve()
    {
        var snapshot = EcbRateParser.Parse(DailyXml);

        Assert.Equal(29, snapshot.Rates.Count);
    }

    [Fact]
    public void Parse_postavlja_EUR_kao_baznu_valutu()
    {
        var snapshot = EcbRateParser.Parse(DailyXml);

        Assert.All(snapshot.Rates, rate => Assert.Equal("EUR", rate.Base.Value));
    }
}