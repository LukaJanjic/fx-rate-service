using System.Net;

namespace FxRateService.UnitTests.Fixtures;

/// <summary>
/// Zamenjuje mrezni sloj u testovima — vraca unapred pripremljen odgovor
/// umesto da salje pravi zahtev.
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
        _respond = respond;

    public int CallCount { get; private set; }

    public Uri? LastRequestUri { get; private set; }

    public static StubHttpMessageHandler ReturnsOk(string content) =>
        new(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content),
        });

    public static StubHttpMessageHandler ReturnsStatus(HttpStatusCode status) =>
        new(_ => new HttpResponseMessage(status));

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        LastRequestUri = request.RequestUri;

        return Task.FromResult(_respond(request));
    }

    public HttpClient CreateClient() => new(this);
}