using System.Globalization;
using System.Xml.Linq;
using FxRateService.Core.Domain;

namespace FxRateService.Infrastructure.Providers.Ecb;

/// <summary>
/// Parsira ECB eurofxref-daily.xml u RateSnapshot.
/// Format: gnezdjeni Cube elementi — omotac, datum, pa po jedan po valuti.
/// </summary>
public static class EcbRateParser
{
    private static readonly XNamespace Ns = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

    private static readonly CurrencyCode Euro = CurrencyCode.Parse("EUR");

    public static RateSnapshot Parse(string xml)
    {
        var document = XDocument.Parse(xml);

        var dayCube = document.Descendants(Ns + "Cube")
            .FirstOrDefault(c => c.Attribute("time") is not null)
            ?? throw new FormatException("ECB feed nema Cube element sa 'time' atributom.");

        var asOf = DateOnly.ParseExact(
            dayCube.Attribute("time")!.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        var rates = new List<ExchangeRate>();

        foreach (var cube in dayCube.Elements(Ns + "Cube"))
        {
            var currency = cube.Attribute("currency")?.Value;
            var rate = cube.Attribute("rate")?.Value;

            if (currency is null || rate is null)
            {
                continue;
            }

            if (!decimal.TryParse(rate, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                continue;
            }

            rates.Add(ExchangeRate.Of(Euro, CurrencyCode.Parse(currency), value));
        }

        return RateSnapshot.Of(asOf, rates);
    }
}