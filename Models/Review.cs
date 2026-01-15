using System.ComponentModel.DataAnnotations;
namespace BookRatings.MVC.Models
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; } 

        public int BookId { get; set; }
        public Book? Book { get; set; } 

        [Range (1, 10)]
        public int RatingValue { get; set; }
    }
}