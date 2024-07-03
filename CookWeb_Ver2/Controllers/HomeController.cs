using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using CookWeb_Ver2.Data;
using CookWeb_Ver2.Models;

namespace CookWeb_Ver2.Controllers
{
    public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string currentFilter, int? pageNumber,string keyword)
        {
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            var recipes = from r in _context.Recipes.Where(r => r.IsInvisible == false).OrderBy(p => p.TypeCookId)
                          select r;

            if (!String.IsNullOrEmpty(searchString))
            {
                recipes = recipes.Where(s => s.Name.Contains(searchString)).OrderBy(p => p.TypeCookId);
            }
            if(keyword != "All" && keyword != null)
            {
                if(keyword == "Cơm")
                {
                    recipes = recipes.Where(s => s.Name.Contains(keyword) || s.Name.Contains("Phở"));
                }
                recipes = recipes.Where(s => s.Name.Contains(keyword)).OrderBy(p => p.TypeCookId);
            }

            int pageSize = 200;

            return View(await PaginatedList<Recipes>.CreateAsync(recipes.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<OrderDetailViewModel> orderDetails)
        {
            var role = Request.Cookies["MYCOOKIE"];
            Order newOrder = new Order();
            if (role != null)
            {
                newOrder.Code = role.ToString();
                _context.Add(newOrder);
                _context.SaveChanges();
            }
            else
            {
                newOrder.Code = "NA";
                _context.Add(newOrder);
                _context.SaveChanges();
            }

            foreach (var orderDetail in orderDetails)
            {
                for (int i = 0; i < orderDetail.Quantity; i++)
                {
                    Recipes? recipe = await _context.Recipes.FindAsync(orderDetail.RecipeId);
                    if (recipe != null)
                    {
                        Machine machine = await _context.Machines.Where(p => p.MachineTypeId == recipe.TypeCookId).FirstAsync();
                        OrderDetail newOrderDetail = new OrderDetail
                        {
                            OrderId = newOrder.Id,
                            RecipesID = recipe.Id,
                            MachineId = machine.Id,
                            Code = newOrder.Code,
                            OrdinalNumber = 0,
                            Status = 100,
                            TypeFood = orderDetail.TypeFood,
                        };

                        _context.OrderDetails.Add(newOrderDetail);
                        await _context.SaveChangesAsync();
                    } 
                }
            }

            ViewBag.lstRecipes = await _context.Recipes.ToListAsync();
            return View();
        }

        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
