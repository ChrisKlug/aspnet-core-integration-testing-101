using Bazinga.AspNetCore.Authentication.Basic;
using Demo.AspNet.IntegrationTesting.Api.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Demo.AspNet.IntegrationTesting.Api.Tests;

public class ChuckQuoteTests
{
    [Fact]
    public async Task GET_returns_a_quote()
    {
        var client = SetUp(x => A.CallTo(() => x.GetQuote()).Returns("Chuck Norris can dribble a bowling ball"));

        var response = await client.GetAsync("/chuck-quote");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var quote = await response.Content.ReadAsStringAsync();
        Assert.NotNull(quote);
        Assert.Matches("Chuck Norris can dribble a bowling ball", quote);
    }

    [Fact]
    public async void GET_returns_HTTP_500_and_explanation_ChuckQuoteProvider_throws_exception()
    {
        HttpClient client = SetUp(x => A.CallTo(() => x.GetQuote()).Throws<Exception>());

        var response = await client.GetAsync("/chuck-quote");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var quote = await response.Content.ReadAsStringAsync();

        Assert.NotNull(quote);
        Assert.Matches("API was not strong enough to joke about Chuck...", quote);
    }

    [Fact]
    public async void GET_returns_HTTP_401_if_user_is_not_authenticated()
    {
        var client = SetUp(isAuthenticated: false);

        var response = await client.GetAsync("/chuck-quote");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static HttpClient SetUp(Action<IChuckQuotesProvider>? chuckQuoteConfig = null, bool isAuthenticated = true)
    {
        var application = new WebApplicationFactory<Program>()
                                        .WithWebHostBuilder(builder =>
                                        {
                                            builder.ConfigureTestServices(services =>
                                            {
                                                var chuckFake = A.Fake<IChuckQuotesProvider>();
                                                chuckQuoteConfig?.Invoke(chuckFake);

                                                services.AddSingleton(chuckFake);

                                                services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                                                    .AddBasicAuthentication(credentials =>
                                                    {
                                                        return Task.FromResult(
                                                            credentials.username == "test"
                                                            && credentials.password == "test");
                                                    });
                                            });
                                        });

        var client = application.CreateClient();

        if (isAuthenticated)
        {
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.UTF8.GetBytes("test:test"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }

        return client;
    }
}