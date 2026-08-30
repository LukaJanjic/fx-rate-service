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

    [Fact]
    public void Parse_cita_tacne_vrednosti_kurseva()
    {
        var snapshot = EcbRateParser.Parse(DailyXml);

        var usd = snapshot.Rates.Single(r => r.Quote.Value == "USD");
        var idr = snapshot.Rates.Single(r => r.Quote.Value == "IDR");
        var gbp = snapshot.Rates.Single(r => r.Quote.Value == "GBP");

        Assert.Equal(1.1669m, usd.Value);
        Assert.Equal(20668.66m, idr.Value);
        Assert.Equal(0.85613m, gbp.Value);
    }

    [Fact]
    public void Parse_preskace_kurs_bez_atributa()
    {
        const string xml = """
            <gesmes:Envelope xmlns:gesmes="http://www.gesmes.org/xml/2002-08-01"
                             xmlns="http://www.ecb.int/vocabulary/2002-08-01/eurofxref">
              <Cube>
                <Cube time="2026-08-26">
                  <Cube currency="USD" rate="1.1669"/>
                  <Cube currency="XYZ"/>
                  <Cube currency="JPY" rate="185.62"/>
                </Cube>
              </Cube>
            </gesmes:Envelope>
            """;

        var snapshot = EcbRateParser.Parse(xml);

        Assert.Equal(2, snapshot.Rates.Count);
    }

    [Fact]
    public void Parse_puca_na_neispravnom_xml()
    {
        Assert.ThrowsAny<Exception>(() => EcbRateParser.Parse("ovo nije xml"));
    }

    [Fact]
    public void Parse_puca_kad_nema_datuma()
    {
        const string xml = """
            <gesmes:Envelope xmlns:gesmes="http://www.gesmes.org/xml/2002-08-01"
                             xmlns="http://www.ecb.int/vocabulary/2002-08-01/eurofxref">
              <Cube>
                <Cube currency="USD" rate="1.1669"/>
              </Cube>
            </gesmes:Envelope>
            """;

        Assert.Throws<FormatException>(() => EcbRateParser.Parse(xml));
    }
}