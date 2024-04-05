using Demo.AspNet.IntegrationTesting.Api.Data;
using Demo.AspNet.IntegrationTesting.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddTransient<IChuckQuotesProvider, DefaultChuckQuotesProvider>();
builder.Services.AddDbContext<ToDosDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Sql"));
});
builder.Services.AddScoped<IToDoService, DefaultToDoService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World");

app.Run();
