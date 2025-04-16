using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using InventoryManagement.Models;
using System.Security.Claims;

namespace InventoryManagement.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .ToListAsync();
            return View(products);
        }

        // GET: Product/Search
        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && 
                    (string.IsNullOrEmpty(searchTerm) || 
                     p.Name.Contains(searchTerm) || 
                     p.Description.Contains(searchTerm) ||
                     p.Category.Name.Contains(searchTerm)))
                .ToListAsync();

            return PartialView("_ProductList", products);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null || !product.IsActive)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Quantity,CategoryId,ImageUrl")] Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logger.LogInformation("Attempting to create product with Name: {Name}", product.Name);
                    
                    product.CreatedAt = DateTime.UtcNow;
                    product.IsActive = true;
                    product.CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                    product.UpdatedBy = product.CreatedBy;
                    
                    if (string.IsNullOrEmpty(product.ImageUrl))
                    {
                        product.ImageUrl = "/images/default-product.png";  // Set a default image URL
                    }

                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Product {ProductId} created by {UserId}", product.Id, product.CreatedBy);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogWarning("Invalid ModelState when creating product");
                    foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning("Validation error: {ErrorMessage}", modelError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", "An error occurred while creating the product. Please try again.");
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(product);
        }

        // GET: Product/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive)
            {
                return NotFound();
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Quantity,CategoryId,ImageUrl")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null || !existingProduct.IsActive)
                    {
                        return NotFound();
                    }

                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Quantity = product.Quantity;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.ImageUrl = product.ImageUrl;
                    existingProduct.UpdatedAt = DateTime.UtcNow;
                    existingProduct.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Product {ProductId} updated by {UserId}", product.Id, existingProduct.UpdatedBy);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(product);
        }

        // GET: Product/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null || !product.IsActive)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
                product.UpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product {ProductId} soft deleted by {UserId}", product.Id, product.UpdatedBy);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
} 