namespace BookRatings.MVC.ViewModels
{
    public class RecommendationVm
    {
        public float UserId { get; set; }
        public string ISBN { get; set; } = string.Empty;
       

        public float? PredictedRating { get; set; }
        public string? Error { get; set; }
    }
}