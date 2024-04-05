using Bazinga.AspNetCore.Authentication.Basic;
using Demo.AspNet.IntegrationTesting.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Data.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace Demo.AspNet.IntegrationTesting.Api.Tests;

public class ToDosTests
{
    [Fact]
    public Task GET_returns_HTTP_200()
        => Execute(async client => {
            var response = await client.GetAsync("/todos");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        });


    [Fact]
    public Task GET_returns_a_list_of_items()
        => Execute(
            addTestData: async cmd =>
            {
                cmd.CommandText = "INSERT INTO ToDos (Title, Description, Completed) VALUES" +
                                "('Present talk', 'Present talk about Integration Testing', null), " +
                                "('Relax', 'Relax and feel nice after doing talk', null);";
                await cmd.ExecuteNonQueryAsync();
            },
            execute: async client => {
                var response = await client.GetAsync("/todos");

                var content = JArray.Parse(await response.Content.ReadAsStringAsync());

                Assert.NotEmpty(content);
                Assert.Equal(2, content.Count);
                Assert.Contains(content, x => x.Value<string>("title") == "Present talk");
                Assert.Contains(content, x => x.Value<string>("title") == "Relax");
            });

    [Fact]
    public Task PATCH_updates_ToDoItem_and_returns_HTTP_200_and_the_item()
    {
        int toDoItemId = 0;

        return Execute(
                addTestData: async cmd =>
                {
                    cmd.CommandText = "INSERT INTO ToDos (Title, Description, Completed) VALUES" +
                                    "('Present talk', 'Present talk about Integration Testing', null);" +
                                    "SELECT SCOPE_IDENTITY();";
                    toDoItemId = (int)(decimal)(await cmd.ExecuteScalarAsync())!;
                },
                execute: async client =>
                {
                    var response = await client.PatchAsJsonAsync("/todos/" + toDoItemId,
                        new { Title = "New Title", Description = "New Description", IsComplete = true });

                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    var content = JObject.Parse(await response.Content.ReadAsStringAsync());

                    Assert.NotNull(content);
                    Assert.Equal("New Title", content.Value<string>("title"));
                    Assert.Equal("New Description", content.Value<string>("description"));
                    Assert.True(content.Value<bool>("isCompleted"));
                },
                validateDdb: async cmd =>
                {
                    cmd.CommandText = "SELECT * FROM ToDos WHERE ID=" + toDoItemId;
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        Assert.True(await dr.ReadAsync());
                        Assert.Equal("New Title", (string)dr["title"]);
                        Assert.Equal("New Description", (string)dr["description"]);
                        Assert.True(dr["completed"] is not DBNull);
                        Assert.False(await dr.ReadAsync());
                    }
                }
            );
    }

    private static async Task Execute(Func<HttpClient, Task> execute,
                                    Func<DbCommand, Task>? addTestData = null,
                                    Func<DbCommand, Task>? validateDdb = null,
                                    bool isAuthenticated = true)
    {
        var application = new WebApplicationFactory<Program>()
                                        .WithWebHostBuilder(builder =>
                                        {
                                            builder.UseEnvironment("IntegrationTesting");

                                            //builder.ConfigureAppConfiguration((ctx, config) =>
                                            //{
                                            //    config.AddInMemoryCollection(new Dictionary<string, string?>() {
                                            //            {
                                            //                "ConnectionStrings:Sql",
                                            //                "Server=.,1433;Database=IntegrationTestingDemo;User Id=sa;Password=MyVerySecretPassw0rd;TrustServerCertificate=True;MultipleActiveResultSets=True"
                                            //            }
                                            //    });
                                            //});

                                            builder.ConfigureTestServices(services =>
                                            {
                                                services.AddSingleton<ToDosDbContext>();

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

        using (var services = application.Services.CreateScope())
        {
            IDbContextTransaction? transaction = null;
            try
            {
                var ctx = services.ServiceProvider.GetRequiredService<ToDosDbContext>();
                transaction = ctx.Database.BeginTransaction();
                var conn = ctx.Database.GetDbConnection();
                var cmd = conn.CreateCommand();
                cmd.Transaction = transaction.GetDbTransaction();

                if (addTestData is not null)
                {
                    await addTestData(cmd);
                }

                await execute(client);

                if (validateDdb is not null)
                {
                    await validateDdb(cmd);
                }
            }
            finally
            {
                transaction?.Rollback();
            }
        }
    }
}