using System.Net.Http.Json;

namespace BookRatings.MVC.Services
{
    public class PredictionService
    {
        private readonly HttpClient _httpClient;

        public PredictionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            Console.WriteLine(">>> ML API BaseAddress = " + (_httpClient.BaseAddress?.ToString() ?? "NULL"));
        }

        public async Task<float> PredictAsync(float userId, string isbn)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/Prediction/predict",
                new { userId, isbn }
            );

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"ML API error: {content}");

            var result = await response.Content
                .ReadFromJsonAsync<PredictionResponse>();

            return result?.PredictedRating ?? 0;
        }
    }

    public class PredictionResponse
    {
        public float PredictedRating { get; set; }
    }
}