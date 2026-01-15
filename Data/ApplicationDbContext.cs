using Microsoft.EntityFrameworkCore;
using BookRatings.MVC.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BookRatings.MVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<User> Users => Set<User>();
        public DbSet<AuthorEntity> Authors => Set<AuthorEntity>();
        public DbSet<PublisherEntity> Publishers => Set<PublisherEntity>();
        public DbSet<Review> Reviews { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Ignore<DocumentFormat.OpenXml.MarkupCompatibilityAttributes>();
            modelBuilder.Ignore<DocumentFormat.OpenXml.OpenXmlElement>();
        
            modelBuilder.Entity<Review>()
    .HasOne(r => r.Book)
    .WithMany(b => b.Reviews)
    .HasForeignKey(r => r.BookId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // opțional: un review per user per book
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.UserId, r.BookId })
                .IsUnique();

        }
    }
}