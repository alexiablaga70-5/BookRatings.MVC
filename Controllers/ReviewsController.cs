using BookRatings.MVC.Data;
using BookRatings.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookRatings.MVC.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var list = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .ToListAsync();

            return View(list);
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // GET: Reviews/Create
        public IActionResult Create()
        {
            ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title");
            return View();
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,RatingValue")] Review review)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", review.BookId);
                return View(review);
            }

            // Fără login: punem user default (primul user)
            var defaultUserId = await _context.Users.Select(u => u.UserId).FirstOrDefaultAsync();
            if (defaultUserId == 0)
            {
                // dacă nu există useri, creează unul
                _context.Users.Add(new User());
                await _context.SaveChangesAsync();
                defaultUserId = await _context.Users.Select(u => u.UserId).FirstAsync();
            }

            review.UserId = defaultUserId;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == id);
            if (review == null) return NotFound();

            ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", review.BookId);
            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,BookId,RatingValue")] Review review)
        {
            if (id != review.ReviewId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", review.BookId);
                return View(review);
            }

            // păstrează UserId existent
            var existing = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == id);
            if (existing == null) return NotFound();

            existing.BookId = review.BookId;
            existing.RatingValue = review.RatingValue;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.ReviewId == id);

            if (review == null) return NotFound();

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}