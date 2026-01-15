using BookRatings.MVC.ViewModels; // aici trebuie să ai RecommendationVm
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BookRatings.MVC.Controllers
{
    public class RecommendationsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RecommendationsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // valori default (opțional)
            var vm = new RecommendationVm();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(RecommendationVm vm)
        {
            // validare simplă
            if (vm.UserId <= 0 || string.IsNullOrWhiteSpace(vm.ISBN))
            {
                vm.Error = "Completează UserId și ISBN.";
                vm.PredictedRating = null;
                return View(vm);
            }

            try
            {
                // clientul configurat în Program.cs: AddHttpClient("MlApi", ...)
                var client = _httpClientFactory.CreateClient("MlApi");

                // payload exact cum ai în swagger (userId + isbn)
                var payload = new
                {
                    userId = (float)vm.UserId,
                    isbn = vm.ISBN
                };

                // IMPORTANT: folosim URL relativ, că BaseAddress vine din appsettings.json
                var response = await client.PostAsJsonAsync("api/prediction/predict", payload);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<PredictionResponse>();

                vm.PredictedRating = result?.PredictedRating;
                vm.Error = null;

                return View(vm);
            }
            catch (Exception ex)
            {
                vm.Error = "Eroare la apelul ML API: " + ex.Message;
                vm.PredictedRating = null;
                return View(vm);
            }
        }
    }

    // răspunsul pe care îl întorci din ML API:
    // { "predictedRating": 9.73 }
    public class PredictionResponse
    {
        public float PredictedRating { get; set; }
    }
}