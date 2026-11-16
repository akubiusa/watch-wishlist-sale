using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using WatchWishlistSale.Activities;
using WatchWishlistSale.Models.Wishlist;

namespace WatchWishlistSale.Tests;

public class GetAppDetailsTests
{
    private const long AppId = 42;

    private static GetAppDetails CreateActivity(HttpStatusCode statusCode, string body = "")
    {
        StubHttpMessageHandler handler = new(statusCode, body);
        return new GetAppDetails(new StubHttpClientFactory(handler), NullLogger<GetAppDetails>.Instance);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task HttpErrorThrowsWithStatusCode(HttpStatusCode statusCode)
    {
        GetAppDetails activity = CreateActivity(statusCode);

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(() => activity.GetAppDetailsActivity(AppId));

        Assert.Equal(statusCode, exception.StatusCode);
    }

    [Fact]
    public async Task UnsuccessfulResultReturnsNull()
    {
        GetAppDetails activity = CreateActivity(HttpStatusCode.OK, """{"42":{"success":false}}""");

        AppDetails? details = await activity.GetAppDetailsActivity(AppId);

        Assert.Null(details);
    }

    [Fact]
    public async Task SuccessfulResultReturnsData()
    {
        GetAppDetails activity = CreateActivity(
            HttpStatusCode.OK,
            """{"42":{"success":true,"data":{"type":"game","name":"x","steam_appid":42,"price_overview":{"currency":"JPY","initial":100000,"final":50000,"discount_percent":50}}}}""");

        AppDetails? details = await activity.GetAppDetailsActivity(AppId);

        Assert.NotNull(details);
        Assert.Equal(42, details.SteamAppId);
        Assert.Equal(50000, details.PriceOverview?.Final);
    }
}
