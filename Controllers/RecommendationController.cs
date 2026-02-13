using BookRatings.MVC.Data;
using BookRatings.MVC.Services;
using BookRatings.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace BookRatings.MVC.Controllers
{
    public class RecommendationsController : Controller
    {
        private readonly PredictionService _predictionService;


        public RecommendationsController(
            PredictionService predictionService)
        {
            _predictionService = predictionService;
           
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new RecommendationVm());
        }

        [HttpPost]
        public async Task<IActionResult> Index(RecommendationVm vm)
        {
            if (vm.UserId <= 0 || string.IsNullOrWhiteSpace(vm.ISBN))
            {
                vm.Error = "Completează UserId și ISBN.";
                vm.PredictedRating = null;
                return View(vm);
            }

            try
            {
                var rating = await _predictionService.PredictAsync((float)vm.UserId, vm.ISBN);

                vm.PredictedRating = rating;
                vm.Error = null;

                // 🔽 AICI ADAUGI

               

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
}