using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.Include(o => o.OrderDetails).ToListAsync();
            return View(orders);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.ToList();
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GuestName,GuestEmail,TotalPrice")] Order order, int[] ProductIds, int[] Quantities)
        {
            if (ModelState.IsValid)
            {
                order.OrderDetails = new List<OrderDetail>();

                for (int i = 0; i < ProductIds.Length; i++)
                {
                    var product = await _context.Products.FindAsync(ProductIds[i]);
                    if (product != null)
                    {
                        var orderDetail = new OrderDetail
                        {
                            ProductId = product.ProductId,
                            Quantity = Quantities[i],
                            UnitPrice = product.Price
                        };
                        order.OrderDetails.Add(orderDetail);
                    }
                }

                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.FindAsync(id);

            if (order == null) return NotFound();
            
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")] // ✅ Ensures it maps to the expected "Delete" action
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index"); // ✅ Redirect to the Orders list after deletion
        }
    }
}
