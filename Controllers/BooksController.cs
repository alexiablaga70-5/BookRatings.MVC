using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookRatings.MVC.Data;
using BookRatings.MVC.Models;

namespace BookRatings.MVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Books
        // GET: Books
        public async Task<IActionResult> Index(
            string? searchString,
            int? authorId,
            int? publisherId,
            string? sortOrder)
        {
            // pentru sort links în view
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentAuthor"] = authorId;
            ViewData["CurrentPublisher"] = publisherId;

            ViewData["TitleSort"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["YearSort"] = sortOrder == "year" ? "year_desc" : "year";

            // dropdowns pentru filtre
            ViewData["AuthorEntityId"] = new SelectList(
                await _context.Authors.OrderBy(a => a.Name).ToListAsync(),
                "AuthorEntityId",
                "Name",
                authorId);

            ViewData["PublisherEntityId"] = new SelectList(
                await _context.Publishers.OrderBy(p => p.Name).ToListAsync(),
                "PublisherEntityId",
                "Name",
                publisherId);

            // query
            var query = _context.Books
                .Include(b => b.AuthorEntity)
                .Include(b => b.PublisherEntity)
                .AsQueryable();

            // SEARCH (Title)
            if (!string.IsNullOrWhiteSpace(searchString))
                query = query.Where(b => b.Title.Contains(searchString));

            // FILTER (Author / Publisher)
            if (authorId.HasValue)
                query = query.Where(b => b.AuthorEntityId == authorId.Value);

            if (publisherId.HasValue)
                query = query.Where(b => b.PublisherEntityId == publisherId.Value);

            // SORT
            query = sortOrder switch
            {
                "title_desc" => query.OrderByDescending(b => b.Title),
                "year" => query.OrderBy(b => b.Year),
                "year_desc" => query.OrderByDescending(b => b.Year),
                _ => query.OrderBy(b => b.Title),
            };

            var books = await query.ToListAsync();
            return View(books);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.ISBN == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["AuthorEntityId"] = new SelectList(_context.Authors, "AuthorEntityId", "Name");
            ViewData["PublisherEntityId"] = new SelectList(_context.Publishers, "PublisherEntityId", "Name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ISBN,Title,Year,AuthorEntityId,PublisherEntityId")] Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorEntityId"] = new SelectList(_context.Authors, "AuthorEntityId", "Name", book.AuthorEntityId);
            ViewData["PublisherEntityId"] = new SelectList(_context.Publishers, "PublisherEntityId", "Name", book.PublisherEntityId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["AuthorEntityId"] = new SelectList(_context.Authors, "AuthorEntityId", "Name", book.AuthorEntityId);
            ViewData["PublisherEntityId"] = new SelectList(_context.Publishers, "PublisherEntityId", "Name", book.PublisherEntityId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
      
        public async Task<IActionResult> Edit(string id, [Bind("ISBN,Title,Year,AuthorEntityId,PublisherEntityId")] Book book)
        {
            if (id != book.ISBN) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AuthorEntityId"] = new SelectList(_context.Authors, "AuthorEntityId", "Name", book.AuthorEntityId);
            ViewData["PublisherEntityId"] = new SelectList(_context.Publishers, "PublisherEntityId", "Name", book.PublisherEntityId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.ISBN == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(string id)
        {
            return _context.Books.Any(e => e.ISBN == id);
        }
    }
}
