using System.Net;
using Microsoft.Extensions.Options;
using TTX.AdminBot.Options;

namespace TTX.AdminBot.Services;

public class TtxApiClient(IOptions<TtxOptions> _options, IHttpClientFactory _httpClientFactory)
{
    public async Task<(bool, string)> CreateCreator(string slug, string ticker)
    {
        HttpClient client = GetClient();
        HttpResponseMessage response = await client.PostAsync($"/creators?username={slug}&ticker={ticker}", null);
        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return (false, responseContent);

            }

            return (false, response.ReasonPhrase ?? responseContent);
        }

        return (true, responseContent);
    }

    private HttpClient GetClient()
    {
        HttpClient client = _httpClientFactory.CreateClient("ttx");
        client.BaseAddress = _options.Value.BaseUrl;
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.Value.Token}");

        return client;
    }
}
