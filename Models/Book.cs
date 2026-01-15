namespace BookRatings.MVC.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string ISBN { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int? Year { get; set; }

        public int AuthorEntityId { get; set; }
        public AuthorEntity AuthorEntity { get; set; } = null!;

        public int PublisherEntityId { get; set; }
        public PublisherEntity PublisherEntity { get; set; } = null!;

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}