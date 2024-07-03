using CookWeb_Ver2.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using System.Formats.Asn1;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace CookWeb_Ver2.Controllers
{
    [Authorize]
    public class MonitorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MonitorsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Sort()
        {
            return View(await _context.OrderDetails.Include(m=>m.Recipes)
                .Where(m=>m.Status==0).OrderBy(m=>m.OrdinalNumber).ToListAsync());
        }

        public async Task<IActionResult> Index()
        {
            var role = Request.Cookies["MYCOOKIE"];
            if(role == "Admin" || role == "Staff")
            {
                List<OrderDetail> activateOrders = ViewBag.lstActivateOrder = await _context.OrderDetails.Include(m => m.Recipes).Include(m => m.Machine)
                .Where(m => m.Status < 99 && m.Machine != null && m.Machine.MachineTypeId == 2).OrderBy(p => p.OrdinalNumber).ToListAsync();

                //List<OrderDetail> activateOrdersChien = ViewBag.lstActivateOrder_Chien = await _context.OrderDetails.Include(m => m.Recipes).Include(m => m.Machine)
                //.Where(m => m.Status < 99 && m.Machine.MachineTypeId == 3).OrderBy(p => p.OrdinalNumber).ToListAsync();

                //List<OrderDetail> activateOrdersChaoGang = ViewBag.lstActivateOrder_ChaoGang = await _context.OrderDetails.Include(m => m.Recipes).Include(m => m.Machine)
                //.Where(m => m.Status < 99 && m.Machine.MachineTypeId == 1).OrderBy(p => p.OrdinalNumber).ToListAsync();

                //List<Machine> activateMachines = ViewBag.lstActivateMachine = await _context.Machines.Where(m => m.IsCooking == false).ToListAsync();
                
                ViewBag.lstCookeds = await _context.OrderDetails.Include(m => m.Recipes)
                    .Where(m => m.Status == 99).OrderByDescending(m => m.Id).Take(9).ToListAsync();

                if (activateOrders.Count() > 0)
                {
                    ViewBag.nextOrder = _context.OrderDetails.Where(p => p.Status == 0 && p.Machine != null && p.Machine.MachineTypeId == 2).FirstOrDefault();

                }

                //if (activateOrdersChien.Count() > 0)
                //{
                //    OrderDetail orderChien = ViewBag.nextOrderChien = _context.OrderDetails.Where(p => p.Status == 0 && p.Machine.MachineTypeId == 3).FirstOrDefault();
                //}

                //if (activateOrdersChaoGang.Count() > 0)
                //{
                //    OrderDetail orderChaoGang = ViewBag.nextOrderChaoGang = _context.OrderDetails.Where(p => p.Status == 0 && p.Machine.MachineTypeId == 1).FirstOrDefault();
                //}
            }
            else
            {
                List<OrderDetail> activateOrders = ViewBag.lstActivateOrder = await _context.OrderDetails.Include(m => m.Recipes).Include(m => m.Machine)
                .Where(m => m.Status < 99 && m.Machine!= null && m.Machine.MachineTypeId == 2 && m.Code == role).OrderBy(p => p.OrdinalNumber).ToListAsync();
                
                //List<Machine> activateMachines = ViewBag.lstActivateMachine = await _context.Machines.Where(m => m.IsCooking == false).ToListAsync();
                
                //ViewBag.lstCookeds = await _context.OrderDetails.Include(m => m.Recipes).Include(m => m.Machine)
                //    .Where(m => m.Status == 99 && m.Code == role).OrderByDescending(m => m.Id).Take(9).ToListAsync(); 

                //List<OrderDetail> activateOrdersChien = ViewBag.lstActivateOrder_Chien = await _context.OrderDetails.Include(m => m.Recipes).Include(m => m.Machine)
                //.Where(m => m.Status < 99 && m.Machine.MachineTypeId == 3 && m.Code == role).OrderBy(p => p.OrdinalNumber).ToListAsync();

                //List<OrderDetail> activateOrdersChaoGang = ViewBag.lstActivateOrder_ChaoGang = await _context.OrderDetails.Include(m => m.Recipes).Include(m => m.Machine)
                //.Where(m => m.Status < 99 && m.Machine.MachineTypeId == 1 && m.Code == role).OrderBy(p => p.OrdinalNumber).ToListAsync();
              
                ViewBag.lstCookeds = await _context.OrderDetails.Include(m => m.Recipes)
                    .Where(m => m.Status == 99 && m.Code == role).OrderByDescending(m => m.Id).Take(9).ToListAsync();

                if (activateOrders.Count() > 0)
                {
                    ViewBag.nextOrder = _context.OrderDetails.Where(p => p.Status == 0 && p.Machine!= null && p.Machine.MachineTypeId == 2 && p.Code == role).FirstOrDefault();
                }

                //if (activateOrdersChien.Count() > 0)
                //{
                //    OrderDetail orderChien = ViewBag.nextOrderChien = _context.OrderDetails.Where(p => p.Status == 0 && p.Machine.MachineTypeId == 3 && p.Code == role).FirstOrDefault();
                //}

                //if (activateOrdersChaoGang.Count() > 0)
                //{
                //    OrderDetail orderChaoGang = ViewBag.nextOrderChaoGang = _context.OrderDetails.Where(p => p.Status == 0 && p.Machine.MachineTypeId == 1 && p.Code == role).FirstOrDefault();
                //}
            }
            return View();
        }

        public async Task<IActionResult> OrderCooking(int orderID)
        {
            var orderCooking = await _context.OrderDetails.Include(m=>m.Machine).Include(m=> m.Recipes.StepsMakeRecipes)
                .FirstOrDefaultAsync(m=>m.Id == orderID);

            int sumTime = 0;
            if(orderCooking != null && orderCooking.Recipes != null && orderCooking.Recipes.StepsMakeRecipes != null)
            {
                foreach (var item in orderCooking.Recipes.StepsMakeRecipes)
                {
                    sumTime += item._Second;
                }
                ViewBag.sumTime = sumTime;
            }

            //display errors
            if(orderCooking != null)
            {
                var ErrorIndex = orderCooking.ErrorCode;
                var errorInfo = _context.ErrorLists.FirstOrDefault(m => m.ErrorIndex == ErrorIndex);
                if (errorInfo != null)
                {
                    if (ErrorIndex != 0)
                    {
                        ViewBag.MessageCook = $"Error {ErrorIndex}: {errorInfo.ErrorInfo}!";
                    }
                    else
                    {
                        ViewBag.MessageCook = $"{errorInfo.ErrorInfo}!";
                    }
                }
            }
            return View(orderCooking);
        }

        [HttpPost]
        public ActionResult UpdateFoodPositions(int[] sortedList)
        {
            try
            {
                List<OrderDetail> filteredRecords = _context.OrderDetails.Where(r => sortedList.Contains(r.Id)).ToList();
                for (int i = 0; i < sortedList.Count(); i++)
                {
                    foreach (var item in filteredRecords)
                    {
                        if (item.Id == sortedList[i])
                        {
                            item.OrdinalNumber = i + 1;
                        }
                    }
                }
                _context.SaveChanges();
                // Thực hiện cập nhật lại vị trí món ăn trong cơ sở dữ liệu
                // Sử dụng sortedList để cập nhật dữ liệu theo thứ tự đã sắp xếp

                // Trả về kết quả thành công cho frontend
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
