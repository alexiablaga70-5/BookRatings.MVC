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
    public class PublisherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublisherController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Publisher
        public async Task<IActionResult> Index()
        {
            return View(await _context.Publishers.ToListAsync());
        }

        // GET: Publisher/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisherEntity = await _context.Publishers
                .FirstOrDefaultAsync(m => m.PublisherEntityId == id);
            if (publisherEntity == null)
            {
                return NotFound();
            }

            return View(publisherEntity);
        }

        // GET: Publisher/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Publisher/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PublisherEntityId,Name")] PublisherEntity publisherEntity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(publisherEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(publisherEntity);
        }

        // GET: Publisher/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisherEntity = await _context.Publishers.FindAsync(id);
            if (publisherEntity == null)
            {
                return NotFound();
            }
            return View(publisherEntity);
        }

        // POST: Publisher/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PublisherEntityId,Name")] PublisherEntity publisherEntity)
        {
            if (id != publisherEntity.PublisherEntityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publisherEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublisherEntityExists(publisherEntity.PublisherEntityId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(publisherEntity);
        }

        // GET: Publisher/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisherEntity = await _context.Publishers
                .FirstOrDefaultAsync(m => m.PublisherEntityId == id);
            if (publisherEntity == null)
            {
                return NotFound();
            }

            return View(publisherEntity);
        }

        // POST: Publisher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publisherEntity = await _context.Publishers.FindAsync(id);
            if (publisherEntity != null)
            {
                _context.Publishers.Remove(publisherEntity);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PublisherEntityExists(int id)
        {
            return _context.Publishers.Any(e => e.PublisherEntityId == id);
        }
    }
}
