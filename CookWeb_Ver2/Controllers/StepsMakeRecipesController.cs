using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CookWeb_Ver2.Data;
using Microsoft.AspNetCore.Authorization;
using CookWeb_Ver2.Models;
using System.Xml.XPath;

namespace CookWeb_Ver2.Controllers
{
    [Authorize]
    public class StepsMakeRecipesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StepsMakeRecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StepsMakeRecipes
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
            var stepsMakeRecipes = from r in _context.StepsMakeRecipes.Include(r => r.Recipes)
                                   select r;

            if (!String.IsNullOrEmpty(searchString))
            {
                stepsMakeRecipes = stepsMakeRecipes.Where(s => s.StepName.Contains(searchString));
            }

            int pageSize = 10;
            return View(await PaginatedList<StepsMakeRecipes>.CreateAsync(stepsMakeRecipes.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: StepsMakeRecipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stepsMakeRecipes = await _context.StepsMakeRecipes
                .Include(s => s.Recipes)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stepsMakeRecipes == null)
            {
                return NotFound();
            }

            return View(stepsMakeRecipes);
        }

        // GET: StepsMakeRecipes/Create
        public IActionResult Create(int recipesId)
        {
            var recipes = _context.Recipes.Where(p => p.Id == recipesId).FirstOrDefault();
            if(recipes != null)
            {
                var stepNames = GetStepNames(recipes.TypeCookId);
                var methodNames = GetMethodNames(recipes.TypeCookId);
                var applicationDbContext = _context.StepsMakeRecipes.Where(i => i.RecipesId == recipesId).OrderBy(m => m.StepNumber);

                int i = applicationDbContext.Count();

                ViewBag.lstStepName = new SelectList(stepNames, "ActionCode", "StepName");
                ViewBag.lstMethod = new SelectList(methodNames, "MethodCode", "MethodName");
                ViewBag.lstStepsMakeRecipes = applicationDbContext.ToArray();
                ViewData["RecipesId"] = new SelectList(_context.Recipes, "Id", "Name", recipesId);
                ViewBag.Recipe = recipes;
                ViewBag.sumTime = recipes.CookingTime;
            }
            
            
            return View();
        }

        private List<MethodNameModel> GetMethodNames(int machineType)
        {
            if (machineType == 2) //Bep xao
            {
                // Khai báo danh sách MachineTypes cứng
                var methodNames = new List<MethodNameModel>
                {
                    new MethodNameModel {Id = 0, MethodName = "None of business", MethodCode = "0",},
                    //new MethodNameModel {Id = 1, MethodName = "Lac xong do", MethodCode = "1",},
                    new MethodNameModel {Id = 2, MethodName = "Do xong lac", MethodCode = "2",},
                    //new MethodNameModel {Id = 3, MethodName = "Do nhanh", MethodCode = "3",},
                    //new MethodNameModel {Id = 4, MethodName = "Do cham", MethodCode = "4",}
                };
                return methodNames;
            }
            else if (machineType == 1) //List actioncode của bếp chảo gang
            {

                var methodNames = new List<MethodNameModel>
                {
                    new MethodNameModel {Id = 0, MethodName = "None of business", MethodCode = "0",},
                    //new MethodNameModel {Id = 1, MethodName = "Lac xong do", MethodCode = "1",},
                    new MethodNameModel {Id = 2, MethodName = "Do xong lac", MethodCode = "2",},
                    //new MethodNameModel {Id = 3, MethodName = "Do nhanh", MethodCode = "3",},
                    //new MethodNameModel {Id = 4, MethodName = "Do cham", MethodCode = "4",}
                };

                return methodNames;
            }
            else //List actioncode của bếp chien
            {
                var methodNames = new List<MethodNameModel>
                {
                    new MethodNameModel {Id = 0, MethodName = "Món thường", MethodCode = "1",},
                    new MethodNameModel {Id = 1, MethodName = "Món x2 khay", MethodCode = "2",},
                };

                return methodNames;
            }
        }

        private List<StepNameModel> GetStepNames(int machineTypeId)
        {
            if(machineTypeId == 2) //Bep xao
            {
                // Khai báo danh sách MachineTypes cứng
                var stepNames = new List<StepNameModel>
                {
                    new StepNameModel { Id = 3,  StepName = "Thêm dầu", ActionCode="4" },
                    new StepNameModel { Id = 4,  StepName = "Thêm nước", ActionCode="7" },
                    new StepNameModel { Id = 5,  StepName = "Thêm sốt", ActionCode="10" },
                    new StepNameModel { Id = 6,  StepName = "Điều chỉnh nấu", ActionCode="13" },
                    new StepNameModel { Id = 2,  StepName = "Khò gas", ActionCode="16" },
                    new StepNameModel { Id = 1,  StepName = "Nguyên liệu khô 1", ActionCode="49" },
                    new StepNameModel { Id = 11,  StepName = "Nguyên liệu khô 2", ActionCode="52" },
                    new StepNameModel { Id = 7,  StepName = "Thêm khay 1", ActionCode="22" },
                    new StepNameModel { Id = 8,  StepName = "Thêm khay 2", ActionCode="25"},
                    new StepNameModel { Id = 9,  StepName = "Thêm khay 3", ActionCode="28" },
                    new StepNameModel { Id = 10, StepName = "Thêm khay 4", ActionCode="31" },
                };


                return stepNames;
            }
            else if(machineTypeId == 1) //List actioncode của bếp chảo gang
            {

                var stepNames = new List<StepNameModel>
                {
                    new StepNameModel { Id = 1, StepName = "Set machine", ActionCode="0" },
                };

                return stepNames;
            }
            else //List actioncode của bếp chien
            {
                var stepNames = new List<StepNameModel>
                {
                    new StepNameModel { Id = 1, StepName = "Set machine", ActionCode="0" },
                };

                return stepNames;
            }
        }

        // POST: StepsMakeRecipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StepCode,StepName,StepNumber,RecipesId,_Param,_Temp,_Second, _Angle,_Capacity,_FireLevel, _Method, _Speed")] StepsMakeRecipes stepsMakeRecipes, 
            int SelectedStep, int SelectedMethod, int selectedFire, int selectedSpeed)
        {
            var recipes = _context.Recipes.Where(p => p.Id == stepsMakeRecipes.RecipesId).FirstOrDefault();
            List<StepsMakeRecipes> lstStep = _context.StepsMakeRecipes.Where(m => m.RecipesId == stepsMakeRecipes.RecipesId).OrderBy(o => o.StepNumber).ToList();

            if (recipes != null)
            {
                var stepNames = GetStepNames(recipes.TypeCookId);
                var methodNames = GetMethodNames(recipes.TypeCookId);
                var stepName = stepNames.FirstOrDefault(x => x.ActionCode == SelectedStep.ToString());

                if (recipes.TypeCookId == 2)
                {
                    stepsMakeRecipes._FireLevel = selectedFire;
                    if(stepName != null)
                    {
                        stepsMakeRecipes.StepName = stepName.StepName;
                        stepsMakeRecipes.ActionCode = stepName.ActionCode;
                    }
                    stepsMakeRecipes._Method = 0;

                    if(lstStep.Count() != 0 && stepsMakeRecipes.StepNumber <= lstStep.Max(p => p.StepNumber))
                    {
                        var stepsNeedChange = lstStep.Where(p => p.StepNumber >= stepsMakeRecipes.StepNumber).ToList();
                        foreach(var item in stepsNeedChange)
                        {
                            item.StepNumber++;
                            _context.Update(item);
                            _context.SaveChanges();
                        }
                    }
                }

                if (recipes.TypeCookId == 3)
                {
                    stepsMakeRecipes._Method = SelectedMethod;
                    stepsMakeRecipes.StepName = "Set machine";
                    stepsMakeRecipes.ActionCode = "0";
                }

                if (recipes.TypeCookId == 1)
                {
                    stepsMakeRecipes.StepName = "Set machine";
                    stepsMakeRecipes.ActionCode = "0";
                }

                stepsMakeRecipes._Speed = selectedSpeed;
                //stepsMakeRecipes._Method = SelectedMethod;

                ViewBag.lstStepName = new SelectList(stepNames, "ActionCode", "StepName");
                ViewBag.lstMethod = new SelectList(methodNames, "MethodCode", "MethodName");

                var applicationDbContext = _context.StepsMakeRecipes.Include(i => i.Recipes).Where(i => i.RecipesId == stepsMakeRecipes.RecipesId).OrderBy(m => m.StepNumber);
                ViewBag.lstStepsMakeRecipes = applicationDbContext.ToArray();
                ViewData["RecipesId"] = new SelectList(_context.Recipes, "Id", "Name", stepsMakeRecipes.RecipesId);
                ViewBag.Recipe = recipes;

                if (ModelState.IsValid)
                {
                    _context.Add(stepsMakeRecipes);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Create", "StepsMakeRecipes", new { recipesId = stepsMakeRecipes.RecipesId });
                }
            }
            return View(stepsMakeRecipes);
        }

        // GET: StepsMakeRecipes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stepsMakeRecipes = await _context.StepsMakeRecipes.FindAsync(id);
           
            if (stepsMakeRecipes == null)
            {
                return NotFound();
            }
            else
            {
                var recipes = _context.Recipes.Where(p => p.Id == stepsMakeRecipes.RecipesId).FirstOrDefault();
                if (recipes != null)
                {
                    ViewBag.sumTime = recipes.CookingTime;
                    var stepNames = GetStepNames(recipes.TypeCookId);
                    var actioncode = (stepsMakeRecipes.ActionCode != null) ? stepsMakeRecipes.ActionCode : "0";
                    var nameStep = stepNames.FirstOrDefault(x => x.ActionCode == actioncode);
                    if (nameStep != null)
                    {
                        stepsMakeRecipes.StepName = nameStep.StepName;
                    }
                    
                    var methodNames = GetMethodNames(recipes.TypeCookId);

                    var applicationDbContext = _context.StepsMakeRecipes.Where(i => i.RecipesId == stepsMakeRecipes.RecipesId).OrderBy(m => m.StepNumber);
                    //int i = applicationDbContext.Count();

                    ViewBag.Recipe = recipes;
                    ViewBag.lstStepName = new SelectList(stepNames, "StepName", "StepName");
                    ViewBag.lstMethod = new SelectList(methodNames, "MethodCode", "MethodName");
                    ViewBag.lstStepsMakeRecipes = applicationDbContext.ToArray();
                    ViewData["RecipesId"] = new SelectList(_context.Recipes, "Id", "Name", stepsMakeRecipes.RecipesId);
                }
            }

            return View(stepsMakeRecipes);
        }

        // POST: StepsMakeRecipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StepCode,StepName,StepNumber,RecipesId,_Param,_Temp,_Second, _Angle,_Capacity,_FireLevel, _Method, _Speed")] StepsMakeRecipes stepsMakeRecipes, 
            int SelectedStep, int SelectedMethod, int selectedFire, int selectedSpeed)
        {
            if (id != stepsMakeRecipes.Id)
            {
                return NotFound();
            }

            var recipes = _context.Recipes.Where(p => p.Id == stepsMakeRecipes.RecipesId).FirstOrDefault();
            if (recipes != null)
            {
                ViewBag.sumTime = recipes.CookingTime;
                var stepNames = GetStepNames(recipes.TypeCookId);
                var methodNames = GetMethodNames(recipes.TypeCookId);
                //var stepName = stepNames.FirstOrDefault(x => x.ActionCode == SelectedStep.ToString());
                var stepName = stepNames.FirstOrDefault(x => x.StepName == stepsMakeRecipes.StepName);

                ViewBag.lstStepName = new SelectList(stepNames, "ActionCode", "StepName");
                ViewBag.lstMethod = new SelectList(methodNames, "MethodCode", "MethodName");

                var applicationDbContext = _context.StepsMakeRecipes.Where(i => i.RecipesId == stepsMakeRecipes.RecipesId);
                ViewBag.lstStepsMakeRecipes = applicationDbContext.ToArray();
                ViewBag.Recipe = recipes;

                if (ModelState.IsValid)
                {
                    try
                    {
                        var existingEntity = _context.StepsMakeRecipes.Find(id);
                        if (existingEntity != null)
                        {
                            //_context.StepsMakeRecipes.Update(existingEntity);
                            _context.Entry(existingEntity).State = EntityState.Detached; // Xóa bỏ thực thể đã tồn tại khỏi DbContext
                        }
                        //stepsMakeRecipes.StepName = stepName.StepName;
                        //stepsMakeRecipes.ActionCode = stepName.ActionCode;

                        if (recipes.TypeCookId == 2)
                        {
                            stepsMakeRecipes._FireLevel = selectedFire;
                            if(stepName != null)
                                stepsMakeRecipes.ActionCode = stepName.ActionCode;
                        }

                        if (recipes.TypeCookId == 3)
                        {
                            //stepsMakeRecipes._Method = SelectedMethod;
                            stepsMakeRecipes.StepName = "Set machine";
                            stepsMakeRecipes.ActionCode = "0";
                        }

                        if (recipes.TypeCookId == 1)
                        {
                            stepsMakeRecipes.StepName = "Set machine";
                            stepsMakeRecipes.ActionCode = "0";
                        }

                        stepsMakeRecipes._Speed = selectedSpeed;

                        //stepsMakeRecipes._Method = SelectedMethod;

                        _context.Update(stepsMakeRecipes);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!StepsMakeRecipesExists(stepsMakeRecipes.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            
            ViewData["RecipesId"] = new SelectList(_context.Recipes, "Id", "Name", stepsMakeRecipes.RecipesId);
            return RedirectToAction("Edit", "StepsMakeRecipes", new { receiptsId = stepsMakeRecipes.RecipesId });
        }

        // GET: StepsMakeRecipes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stepsMakeRecipes = await _context.StepsMakeRecipes
                .Include(s => s.Recipes)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stepsMakeRecipes == null)
            {
                return NotFound();
            }

            return View(stepsMakeRecipes);
        }

        // POST: StepsMakeRecipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stepsMakeRecipes = await _context.StepsMakeRecipes.FindAsync(id);
            List<StepsMakeRecipes> lstStep = _context.StepsMakeRecipes.Where(m => stepsMakeRecipes!= null && m.RecipesId == stepsMakeRecipes.RecipesId).OrderBy(o => o.StepNumber).ToList();

            if (stepsMakeRecipes != null)
            {
                if (stepsMakeRecipes.StepNumber <= lstStep.Max(p => p.StepNumber))
                {
                    var stepsNeedChange = lstStep.Where(p => p.StepNumber >= stepsMakeRecipes.StepNumber).ToList();
                    foreach (var item in stepsNeedChange)
                    {
                        item.StepNumber--;
                        _context.Update(item);
                        _context.SaveChanges();
                    }
                }

                _context.StepsMakeRecipes.Remove(stepsMakeRecipes);
                await _context.SaveChangesAsync();
                return RedirectToAction("Create", new { recipesId = stepsMakeRecipes.RecipesId });
            }
            return RedirectToAction("index","recipes");
        }

        private bool StepsMakeRecipesExists(int id)
        {
            return _context.StepsMakeRecipes.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task <IActionResult> CountSum(int value)
        {
            int count = 0;
            var lstStep = _context.StepsMakeRecipes.Where(p => p.RecipesId == value).ToList();
            foreach(var step in lstStep)
            {
                count += step._Second;
            }
            ViewBag.sumTime = count;
            var recipes = await _context.Recipes.Where(p => p.Id == value).FirstAsync();

            recipes.CookingTime = count;
            _context.Recipes.Update(recipes);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        //Them 2 buoc cuoi cung
        [HttpPost]
        public async Task<IActionResult> FinalStep (int value)
        {
            var lstStep = await _context.StepsMakeRecipes.Where(p => p.RecipesId == value).OrderBy(p => p.StepNumber).ToListAsync();
            var stepLast = lstStep.Last();
			var stepNearNearEnd = new StepsMakeRecipes
			{
				StepNumber = stepLast.StepNumber + 1,
				ActionCode = "13",
				_Angle = stepLast._Angle,
				_Second = 5,
				_FireLevel = 0,
				_Speed = 1,
				_Capacity = 0,
				_Method = 0,
				RecipesId = stepLast.RecipesId,
				_Param = "",
				StepName = "Điều chỉnh nấu",

			};
			var stepNearEnd = new StepsMakeRecipes
            {
                StepNumber = stepLast.StepNumber + 2,
				ActionCode = "13",
				_Angle = -45,
				_Second = 12,
				_FireLevel = 0,
				_Speed = 1,
				_Capacity = 0,
				_Method = 0,
				RecipesId = stepLast.RecipesId,
				_Param = "",
				StepName = "Điều chỉnh nấu",

			};
			_context.Add(stepNearNearEnd);
			_context.Add(stepNearEnd);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> FirstStep(int value)
        {
            var lstStep = await _context.StepsMakeRecipes.Where(p => p.RecipesId == value).OrderBy(p => p.StepNumber).ToListAsync();

            var Step01 = new StepsMakeRecipes
            {
                StepNumber = 1,
                ActionCode = "13",
                _Angle = 25,
                _Second = 30,
                _FireLevel = 5,
                _Speed = 2,
                _Capacity = 0,
                _Method = 0,
                RecipesId = value,
                _Param = "",
                StepName = "Điều chỉnh nấu",

            };
            var Step02 = new StepsMakeRecipes
            {
                StepNumber = 2,
                ActionCode = "4",
                _Angle = 100,
                _Second = 10,
                _FireLevel = 0,
                _Speed = 1,
                _Capacity = 20,
                _Method = 0,
                RecipesId = value,
                _Param = "",
                StepName = "Thêm dầu",

            };
            var step03 = new StepsMakeRecipes
            {
                StepNumber = 3,
                ActionCode = "13",
                _Angle = 25,
                _Second = 20,
                _FireLevel = 5,
                _Speed = 3,
                _Capacity = 0,
                _Method = 0,
                RecipesId = value,
                _Param = "",
                StepName = "Điều chỉnh nấu",

            };
            _context.Add(Step01);
            _context.Add(Step02);
            _context.Add(step03);
            _context.SaveChanges();

            return Ok();
        }
    }
}