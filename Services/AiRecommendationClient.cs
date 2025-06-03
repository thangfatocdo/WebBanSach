using System.Net.Http.Json;

public class AiRecommendationClient
{
    private readonly HttpClient _http;
    public AiRecommendationClient(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("AIClient");
    }

    // Gọi retrain
    public async Task<bool> RetrainAsync()
    => (await _http.PostAsync("retrain", null)).IsSuccessStatusCode;

    // Gọi recommend
    public async Task<List<int>> RecommendAsync(int userId, int topN = 5)
    => await _http.GetFromJsonAsync<List<int>>($"recommend?userId={userId}&topN={topN}")
       ?? new List<int>();
}
