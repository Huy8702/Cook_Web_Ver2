using CookWeb_Ver2.Data;
using CookWeb_Ver2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CookWeb_Ver2.Controllers
{
	public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManage;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _roleManage = roleManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            var roleSelected = _roleManage.Roles.Where(x => x.Name != "Admin");
            ViewBag.lstRole = new SelectList(roleSelected, "Name", "Name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register(RegisterViewModel model,string roleName)
        {
            var roleSelected = _roleManage.Roles.Where(x => x.Name != "Admin");
            ViewBag.lstRole = new SelectList(roleSelected, "Name", "Name");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                var Ref_role = await _roleManage.FindByNameAsync(model.RoleName);
                if (Ref_role == null)
                {
                    var roleNew = new IdentityRole
                    {
                        Name = roleName,
                    };
                    var result_role = await _roleManage.CreateAsync(roleNew);

                    var result_AddUserInRole = await _userManager.AddToRoleAsync(user, roleNew.Name);

                    foreach (var err in result_role.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }

                    foreach (var err in result_AddUserInRole.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                }
                else
                {
                    var result_AddUserInRole = await _userManager.AddToRoleAsync(user, roleName);
                    foreach (var err in result_AddUserInRole.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                }
                    
                if (result.Succeeded)
                {
                    return RedirectToAction("index", "home");
                }

                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? ReturnUrl = "")
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    //UPdate flag IsAdmin when login
                    #region UpdateDB RolesSystem
                    var userss = _userManager.Users.Where(p => p.Email == model.Email).FirstOrDefault();
                    if (userss != null)
                    {
                        var role = await _userManager.GetRolesAsync(userss);

                        string key = "MYCOOKIE";
                        string value = $"{role.FirstOrDefault()}";
                        CookieOptions cookieOptions = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(1)
                        };
                        Response.Cookies.Append(key, value, cookieOptions);
                    }
                    
                    #endregion

                    if (!string.IsNullOrEmpty(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }
                        
                }

                ModelState.AddModelError("", "Invalid Login Attempt");
            }
            return View(model);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already in use.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            //SignOut action
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
