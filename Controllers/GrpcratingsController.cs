using Microsoft.AspNetCore.Mvc;
using BookRatings.gRPC;

namespace BookRatings.MVC.Controllers
{
    public class GrpcRatingsController : Controller
    {
        private readonly RatingsGrpc.RatingsGrpcClient _client;

        public GrpcRatingsController(RatingsGrpc.RatingsGrpcClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> Average(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return BadRequest("Trimite ?isbn=...");

            var reply = await _client.GetAverageRatingByIsbnAsync(
                new AverageRatingRequest { Isbn = isbn });

            return Json(new
            {
                isbn,
                averageRating = reply.AverageRating,
                ratingsCount = reply.RatingsCount
            });
        }
    }
}