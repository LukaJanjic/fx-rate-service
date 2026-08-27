using System.Reflection;

namespace FxRateService.UnitTests.Fixtures;

internal static class FixtureLoader
{
    public static string Read(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"FxRateService.UnitTests.Fixtures.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Fixture '{resourceName}' nije pronadjen. Dostupni: " +
                string.Join(", ", assembly.GetManifestResourceNames()));

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}