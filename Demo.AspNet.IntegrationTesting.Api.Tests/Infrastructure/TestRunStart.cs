using Demo.AspNet.IntegrationTesting.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("Demo.AspNet.IntegrationTesting.Api.Tests.Infrastructure.TestRunStart", "Demo.AspNet.IntegrationTesting.Api.Tests")]

namespace Demo.AspNet.IntegrationTesting.Api.Tests.Infrastructure;

public class TestRunStart : XunitTestFramework
{
    public TestRunStart(IMessageSink messageSink) : base(messageSink)
    {
        var config = new ConfigurationManager()
                    .AddJsonFile("appSettings.IntegrationTesting.json")
                    .Build();

        var options = new DbContextOptionsBuilder<ToDosDbContext>()
                        .UseSqlServer(config.GetConnectionString("Sql"));

        var dbContext = new ToDosDbContext(options.Options);

        dbContext.Database.Migrate();
    }
}
