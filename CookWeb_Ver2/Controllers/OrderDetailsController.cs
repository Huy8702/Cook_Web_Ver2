using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CookWeb_Ver2.Data;

namespace CookWeb_Ver2.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.OrderDetails.Include(o => o.Machine).Include(o => o.Order).Include(o => o.Recipes).Where(o=>o.Status < 99 || o.Status == 100);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: OrderDetails
        public async Task<IActionResult> History(string searchName, string searchMachine, string searchUser, DateTime? searchFinishTime)
        {
            var applicationDbContext = _context.OrderDetails.Include(o => o.Machine).Include(o => o.Order).Include(o => o.Recipes).
                Where(o => o.Status == 99).OrderByDescending(o => o.FinishTime);

            if (!string.IsNullOrEmpty(searchName))
            {
                applicationDbContext = (IOrderedQueryable<OrderDetail>)applicationDbContext.Where(o => o.Recipes != null && o.Recipes.Name.Contains(searchName));
            }
            if (!string.IsNullOrEmpty(searchUser))
            {
                applicationDbContext = (IOrderedQueryable<OrderDetail>)applicationDbContext.Where(o => o.Code != null && o.Code.Contains(searchUser));
            }
            if (!string.IsNullOrEmpty(searchMachine))
            {
                applicationDbContext = (IOrderedQueryable<OrderDetail>)applicationDbContext.Where(o => o.Machine != null && o.Machine.Name.Contains(searchMachine));
            }
            if (searchFinishTime.HasValue)
            {
                var searchDate = searchFinishTime.Value.Date; // Lấy ra ngày của thời gian tìm kiếm
                var nextDay = searchDate.AddDays(1); // Lấy ngày tiếp theo
                applicationDbContext = (IOrderedQueryable<OrderDetail>)applicationDbContext.Where(o => o.FinishTime >= searchDate && o.FinishTime < nextDay);
            }

            // Lọc theo trạng thái
            applicationDbContext = (IOrderedQueryable<OrderDetail>)applicationDbContext.Where(o => o.Status == 99);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: OrderDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.Machine)
                .Include(o => o.Order)
                .Include(o => o.Recipes)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        public async Task<IActionResult> Confirm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.Machine)
                .Include(o => o.Order)
                .Include(o => o.Recipes)
                .FirstOrDefaultAsync(m => m.Id == id);
            int oldOrder = await _context.OrderDetails.MaxAsync(p => p.OrdinalNumber);
            if (orderDetail == null)
            {
                return NotFound();
            }
            orderDetail.Status = 0;
            orderDetail.OrdinalNumber = oldOrder + 1;
            _context.OrderDetails.Update(orderDetail);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: OrderDetails/Create
        public IActionResult Create()
        {
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Code");
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Code");
            ViewData["RecipesID"] = new SelectList(_context.Recipes, "Id", "Name");
            return View();
        }

        // POST: OrderDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderId,MachineId,RecipesID,Code,OrdinalNumber,Description,Status")] OrderDetail orderDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Code", orderDetail.MachineId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Code", orderDetail.OrderId);
            ViewData["RecipesID"] = new SelectList(_context.Recipes, "Id", "Name", orderDetail.RecipesID);
            return View(orderDetail);
        }

        // GET: OrderDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Code", orderDetail.MachineId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Code", orderDetail.OrderId);
            ViewData["RecipesID"] = new SelectList(_context.Recipes, "Id", "Name", orderDetail.RecipesID);
            return View(orderDetail);
        }

        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,MachineId,RecipesID,Code,OrdinalNumber,Description,Status")] OrderDetail orderDetail)
        {
            if (id != orderDetail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderDetailExists(orderDetail.Id))
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
            ViewData["MachineId"] = new SelectList(_context.Machines, "Id", "Code", orderDetail.MachineId);
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Code", orderDetail.OrderId);
            ViewData["RecipesID"] = new SelectList(_context.Recipes, "Id", "Name", orderDetail.RecipesID);
            return View(orderDetail);
        }

        // GET: OrderDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.Machine)
                .Include(o => o.Order)
                .Include(o => o.Recipes)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("index", "orderdetails");
        }

        public async Task<IActionResult> DeleteInHis(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("history", "orderdetails");
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.Id == id);
        }
    }
}
