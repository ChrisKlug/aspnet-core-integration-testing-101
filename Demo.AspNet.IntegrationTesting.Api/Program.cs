using Demo.AspNet.IntegrationTesting.Api.Data;
using Demo.AspNet.IntegrationTesting.Api.Models;
using Demo.AspNet.IntegrationTesting.Api.Services;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddTransient<IChuckQuotesProvider, DefaultChuckQuotesProvider>();
builder.Services.AddDbContext<ToDosDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Sql"));
});
builder.Services.AddScoped<IToDoService, DefaultToDoService>();


builder.Services.AddAuthentication(BearerTokenDefaults.AuthenticationScheme).AddBearerToken();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World");

app.MapGet("/chuck-quote", async Task<IResult> (IChuckQuotesProvider provider) =>
{
    try
    {
        return TypedResults.Ok(await provider.GetQuote());
    }
    catch
    {
        return TypedResults.Content("API was not strong enough to joke about Chuck...", statusCode: 500);
    }
}).RequireAuthorization();


app.MapGet("/todos", async Task<IResult> (IToDoService svc, [FromQuery] bool includeCompleted = true) =>
{
    try
    {
        return TypedResults.Ok(await svc.GetToDos(includeCompleted));
    }
    catch
    {
        return TypedResults.Content("Oops something went wrong...", statusCode: 500);
    }
}).RequireAuthorization();

app.MapPatch("/todos/{id}", async Task<IResult> (IToDoService svc, int id, [FromBody] ToDoItemPatch patch) =>
{
    try
    {
        var item = await svc.UpdateToDo(id, patch.Title, patch.Description, patch.IsComplete);

        return (item == null) ? TypedResults.NotFound() : TypedResults.Ok(item);
    }
    catch (Exception ex)
    {
        return TypedResults.Content(ex.GetBaseException().Message, statusCode: 500);
    }
}).RequireAuthorization();
    
app.Run();

public partial class Program { }
