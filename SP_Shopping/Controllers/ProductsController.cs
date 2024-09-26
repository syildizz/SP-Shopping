using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Data;
using SP_Shopping.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var pdtoList = await _context.Products.Include(p => p.Category)
                .Select(pr => new ProductDetailsDtoMapper(_context).MapTo(pr))
                .ToListAsync();
            return View(pdtoList);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            ProductDetailsDto pdto = new ProductDetailsDtoMapper(_context).MapTo(product);

            return View(pdto);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            IEnumerable<Category> categories = _context.Categories.ToList();
            var pdto = new ProductCreateDtoMapper(_context).MapTo(new Product());
            return View(pdto);
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateDto pdto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Product product = new ProductCreateDtoMapper(_context).MapFrom(pdto);
                    product.InsertionDate = DateTime.Now;
                    await _context.AddAsync(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    return BadRequest();
                }
            }
            return View(pdto);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var pdto = new ProductCreateDtoMapper(_context).MapTo(product);

            return View(pdto);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCreateDto pdto)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = new ProductCreateDtoMapper(_context).MapFrom(pdto);
                    product.ModificationDate = DateTime.Now;
                    await _context.Products
                        .Where(p => p.Id == id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(b => b.Name, product.Name)
                            .SetProperty(b => b.Price, product.Price)
                            .SetProperty(b => b.CategoryId, product.CategoryId)
                            .SetProperty(b => b.ModificationDate, product.ModificationDate)
                        );
                    //_context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
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
            return View(pdto);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var pdto = new ProductDetailsDtoMapper(_context).MapTo(product);
            return View(pdto);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
