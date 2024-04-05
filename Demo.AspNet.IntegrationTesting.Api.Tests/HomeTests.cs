using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Demo.AspNet.IntegrationTesting.Api.Tests;

public class HomeTests
{
    [Fact]
    public async Task GET_returns_Hello_World()
    {
        var application = new WebApplicationFactory<Program>();

        var client = application.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Hello World", await response.Content.ReadAsStringAsync());
    }
}
