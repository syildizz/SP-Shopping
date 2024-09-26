using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SP_Shopping.ViewModels;

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
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            List<ProductDetailsViewModel> pdvmList = [];
            foreach (var product in products)
            {
                var pdvm = new ProductDetailsViewModel();
                pdvm.SetProductFields(product);
                pdvmList.Add(pdvm);
            }
            return View(pdvmList);
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

            var pdvm = new ProductDetailsViewModel();
            pdvm.SetProductFields(product);

            return View(pdvm);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            IEnumerable<Category> categories = _context.Categories.ToList();
            var pcvm = new ProductCreateViewModel();
            pcvm.SetCategorySelectList(categories);
            return View(pcvm);
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel pcvm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Category? category = await _context.Categories
                        .Where(c => c.Id == pcvm.CategorySelectedOptionValue)
                        .FirstOrDefaultAsync();
                    Product product = pcvm.GetProductFields(category);
                    await _context.AddAsync(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    return BadRequest();
                }
            }
            return View(pcvm);
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
            IEnumerable<Category> categories = _context.Categories.ToList();
            ProductCreateViewModel pcvm = new ProductCreateViewModel();
            pcvm.SetProductFields(product);
            pcvm.SetCategorySelectList(categories, product.CategoryId);
            return View(pcvm);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCreateViewModel pcvm)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.Products
                        .Where(p => p.Id == id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(b => b.Name, pcvm.Name)
                            .SetProperty(b => b.Price, pcvm.Price)
                            .SetProperty(b => b.CategoryId, pcvm.CategorySelectedOptionValue)
                            .SetProperty(b => b.ModificationDate, DateTime.Now)
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
            return View(pcvm);
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

            var pdvm = new ProductDetailsViewModel();
            pdvm.SetProductFields(product);

            return View(pdvm);
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
