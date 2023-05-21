using ChuckNorrisConnector.Models;
using Microsoft.Net.Http.Headers;

namespace ChuckNorrisConnector
{
  public class ChuckNorrisIOService
  {
    private readonly HttpClient _httpClient;

    public ChuckNorrisIOService(HttpClient httpClient)
    {
      _httpClient = httpClient;

      _httpClient.BaseAddress = new Uri("https://api.chucknorris.io");

      _httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "ECS-ChuckNorris-Service");
    }

    public async Task<List<string>> GetCategories() =>
      await _httpClient.GetFromJsonAsync<List<string>>("jokes/categories") ?? new();

    public async Task<Joke> GetJoke(string category = null)
    {
      var queryString = string.IsNullOrEmpty(category) ? string.Empty : $"?category={category}";

      return (await _httpClient.GetFromJsonAsync<Joke>($"jokes/random{queryString}")) ?? new();
    }
  }
}
