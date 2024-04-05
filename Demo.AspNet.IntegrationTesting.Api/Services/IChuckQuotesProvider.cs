namespace Demo.AspNet.IntegrationTesting.Api.Services;

public interface IChuckQuotesProvider
{
    Task<string> GetQuote();
}

public class DefaultChuckQuotesProvider(HttpClient client) : IChuckQuotesProvider
{
    public async Task<string> GetQuote()
    {
        return (await client.GetFromJsonAsync<QuoteData>("https://api.chucknorris.io/jokes/random")).Value;
    }

    private record struct QuoteData(string Value);
}
