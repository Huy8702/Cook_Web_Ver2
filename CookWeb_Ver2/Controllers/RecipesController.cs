using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CookWeb_Ver2.Data;
using Microsoft.AspNetCore.Authorization;
using CookWeb_Ver2.Models;

namespace CookWeb_Ver2.Controllers
{
    [Authorize]
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RecipesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        //// GET: Recipes
        public async Task<IActionResult> Index(string searchString, string currentFilter, int? pageNumber)
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
            var recipes = from r in _context.Recipes
                           select r;

            if (!String.IsNullOrEmpty(searchString))
            {
                recipes = recipes.Where(s => s.Name.Contains(searchString));
            }

            //int pageSize = 30;
            return View(await PaginatedList<Recipes>.CreateAsync(recipes.AsNoTracking(), pageNumber ?? 1, int.MaxValue));
        }


        // GET: Recipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipes = await _context.Recipes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (recipes == null)
            {
                return NotFound();
            }

            return View(recipes);
        }
        private List<TypeCookIdModel> GetTypeCookId()
        {
            // Khai báo danh sách MachineTypes cứng
            var TypeName = new List<TypeCookIdModel>
            {
                new TypeCookIdModel { Id = 1, TypeCookName = "Bếp chảo gang", TypeCookId=1 },
                new TypeCookIdModel { Id = 2, TypeCookName = "Bếp xào", TypeCookId=2 },
                new TypeCookIdModel { Id = 3, TypeCookName = "Bếp chiên", TypeCookId =3 },
            };

            return TypeName;
        }

        private List<TypeDishIdModel> GetTypeDishId()
        {
            // Khai báo danh sách MachineTypes cứng
            var TypeName = new List<TypeDishIdModel>
            {
                new TypeDishIdModel { Id = 1, TypeDishId = 1 },
                new TypeDishIdModel { Id = 2, TypeDishId = 2 },
                new TypeDishIdModel { Id = 3, TypeDishId = 3 },
                new TypeDishIdModel { Id = 4, TypeDishId = 4 },
                new TypeDishIdModel { Id = 5, TypeDishId = 5 },
                new TypeDishIdModel { Id = 6, TypeDishId = 6 }

            };

            return TypeName;
        }
        // GET: Recipes/Create
        public IActionResult Create()
        {
            var typeNameS = GetTypeCookId();
            var typeNameD = GetTypeDishId();
            ViewBag.lstTypeCook = new SelectList(typeNameS, "TypeCookId", "TypeCookName");
            ViewBag.lstTypeDish = new SelectList(typeNameD,"Id","TypeDishId");
            return View();
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CookingTime,NumberTrayBox,ImagePath,Description,TypeCookId,TypeDishId")] Recipes recipes, IFormFile fileInput, int SelectedTypeCook)
        {
            var typeNameS = GetTypeCookId();
            var typeNameD = GetTypeDishId();
            ViewBag.lstTypeCook = new SelectList(typeNameS, "TypeCookId", "TypeCookName");
            ViewBag.lstTypeDish = new SelectList(typeNameD, "TypeDishId");
            var typeName = typeNameS.FirstOrDefault(x => x.TypeCookId == SelectedTypeCook);
            var typeNameDD = typeNameD.FirstOrDefault(x => x.TypeDishId == SelectedTypeCook);
            if (typeName != null && typeNameDD != null)
                recipes.TypeCookId = typeName.TypeCookId;
                recipes.TypeDishId = typeNameDD.TypeDishId;
            if (ModelState.IsValid)
            {
                // Lưu file vào thư mục
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileInput.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "uploads/Recipes", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileInput.CopyToAsync(stream);
                }

                // Lưu đường dẫn của file vào thuộc tính ImagePath của model
                recipes.ImagePath = "/uploads/Recipes/" + fileName;

                _context.Add(recipes);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(recipes);
        }

        // GET: Recipes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var typeNameS = GetTypeCookId();
            var typeNameD = GetTypeDishId();
            ViewBag.lstTypeCook = new SelectList(typeNameS, "TypeCookId", "TypeCookName");
            ViewBag.lstTypeDish = new SelectList(typeNameD, "Id", "TypeDishId");

            if (id == null)
            {
                return NotFound();
            }

            var recipes = await _context.Recipes.FindAsync(id);
            
            if (recipes == null)
            {
                return NotFound();
            }
            return View(recipes);
        }

        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CookingTime,NumberTrayBox,ImagePath,Description,IsInvisible")] Recipes recipes, IFormFile? fileInput, int? SelectedTypeCook, int? SelectedTypeDish)
        {
            var typeNameS = GetTypeCookId();
            var typeNameD = GetTypeDishId();
            ViewBag.lstTypeCook = new SelectList(typeNameS, "TypeCookId", "TypeCookName");
            ViewBag.lstTypeDish = new SelectList(typeNameD, "Id", "TypeDishId");

            if (id != recipes.Id)
            {
                return NotFound();
            }
            else
            {
                if (SelectedTypeCook != null)
                {
                    var typeName = typeNameS.FirstOrDefault(x => x.TypeCookId == SelectedTypeCook);
                    if (typeName != null)
                    {
                        recipes.TypeCookId = typeName.TypeCookId;
                    }
                }

                if (SelectedTypeDish != null)
                {
                    var typeName = typeNameD.FirstOrDefault(x => x.TypeDishId == SelectedTypeDish);
                    if (typeName != null)
                    {
                        recipes.TypeDishId = typeName.TypeDishId;
                    }
                }

                if (fileInput != null)
                {
                    // Lưu file vào thư mục
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(fileInput.FileName);
                    var filePath = Path.Combine(_env.WebRootPath, "uploads/Recipes", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileInput.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn của file vào thuộc tính ImagePath của model
                    recipes.ImagePath = "/uploads/Recipes/" + fileName;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recipes);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipesExists(recipes.Id))
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
            return View(recipes);
        }

        // GET: Recipes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipes = await _context.Recipes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (recipes == null)
            {
                return NotFound();
            }

            return View(recipes);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recipes = await _context.Recipes.FindAsync(id);
            if (recipes != null)
            {
                _context.Recipes.Remove(recipes);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RecipesExists(int id)
        {
            return _context.Recipes.Any(e => e.Id == id);
        }
    }
}
