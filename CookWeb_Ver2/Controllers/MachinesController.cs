using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CookWeb_Ver2.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Net.Sockets;
using CookWeb_Ver2.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CookWeb_Ver2.Controllers
{
    [Authorize]
    public class MachinesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string? _serverIpAddress;
        private readonly int _serverPort = 0;

        public MachinesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _serverIpAddress = configuration.GetValue<string>("TCPServer:IpAddress");
            _serverPort = configuration.GetValue<int>("TCPServer:Port");
        }

        private List<TypeTrayModel> typeTrayModels = new List<TypeTrayModel>
        {
            new TypeTrayModel{typeId = "takeaway", typeName="TakeAway - SquareBox" },
			new TypeTrayModel{typeId = "takeaway", typeName="TakeAway - CircleBox" },
			new TypeTrayModel{typeId = "eatin", typeName="EatIn - GN" },
		};

        // GET: Machines
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Machines.Include(m => m.MachineType);

            ViewBag.Status = Program.statusConnect;
            ViewBag.isConnected = Program.isConnected;

            return View(await applicationDbContext.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult> UpdateStatus(int[] selectedIds)
        {
            var machines =await _context.Machines.ToListAsync();
            for(int i=0; i< machines.Count(); i++)
            {
				if (selectedIds.Contains(machines[i].Id))
				{
					machines[i].Activate = true;
				}
				else
				{
					machines[i].Activate = false;
					
				}
				_context.SaveChanges();
			}
            return RedirectToAction("Index");
        }

		[HttpPost] //chua co chinh xong
		public async Task<IActionResult> ResetMachine()
		{
			var machines = await _context.Machines.Where(p => p.MachineTypeId == 2).ToListAsync();
            foreach(var item in machines)
            {
                item.IsCooking = false;
                _context.SaveChanges();
            }
            Program.flagReady = true;
            Program.isPauseCook[0] = Program.isPauseCook[1] = Program.isPauseCook[2] = false;
			return Json(new { success = true });
		}

		[HttpPost] //chua co chinh xong
		public IActionResult ConnectAgain()
		{
            Connect2Plc();
			return Json(new { success = true });
		}

		[HttpPost]
		public IActionResult PauseCook(bool isSecondClick)
		{
			if (isSecondClick)
			{
                Program.isPauseCook[0] = !Program.isPauseCook[0];
			}
			else
			{
				Program.isPauseCook[0] = !Program.isPauseCook[0];
			}

			return Json(new { success = true });
		}

		[HttpPost]
		public IActionResult PauseCook2(bool isSecondClick)
		{
			if (isSecondClick)
			{
                Program.isPauseCook[1] = !Program.isPauseCook[1];
			}
			else
			{
				Program.isPauseCook[1] = !Program.isPauseCook[1];
			}

			return Json(new { success = true });
		}

		[HttpPost]
		public IActionResult PauseCook3(bool isSecondClick)
		{
			if (isSecondClick)
			{
                Program.isPauseCook[2] = !Program.isPauseCook[2];
			}
			else
			{
				Program.isPauseCook[2] = !Program.isPauseCook[2];
			}

			return Json(new { success = true });
		}


		// GET: Machines/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .Include(m => m.MachineType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(machine);
        }

        // GET: Machines/Create
        public IActionResult Create()
        {
            ViewData["MachineTypeId"] = new SelectList(_context.MachineTypes, "Id", "Name");
            return View();
        }

        // POST: Machines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Name,Manufacturer,Model,ProductDate,WarrantyExpDate,LastMaintainDate,Location,Description,MachineTypeId")] Machine machine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(machine);
                await _context.SaveChangesAsync();
                machine.MachineId = machine.Id;
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["MachineTypeId"] = new SelectList(_context.MachineTypes, "Id", "Name", machine.MachineTypeId);
            return View(machine);
        }

        // GET: Machines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines.FindAsync(id);
            if (machine == null)
            {
                return NotFound();
            }
            ViewBag.lstTypeTray = new SelectList(typeTrayModels, "typeId", "typeName");
            ViewData["MachineTypeId"] = new SelectList(_context.MachineTypes, "Id", "Name", machine.MachineTypeId);
            return View(machine);
        }

        // POST: Machines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,Manufacturer,Model,ProductDate,WarrantyExpDate,LastMaintainDate,Location,Description,MachineTypeId")] Machine machine)
        {
            if (id != machine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(machine);
                    await _context.SaveChangesAsync();
                    machine.MachineId = machine.Id;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineExists(machine.Id))
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
            ViewBag.lstTypeTray = new SelectList(typeTrayModels, "typeId", "typeName");
            ViewData["MachineTypeId"] = new SelectList(_context.MachineTypes, "Id", "Name", machine.MachineTypeId);
            return View(machine);
        }

        // GET: Machines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .Include(m => m.MachineType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(machine);
        }

        // POST: Machines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machine = await _context.Machines.FindAsync(id);
            if(machine != null)
            {
				_context.Machines.Remove(machine);
				await _context.SaveChangesAsync();
			}
            
            return RedirectToAction(nameof(Index));
        }

        private bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.Id == id);
        }

        public void Connect2Plc()
        {
            if(_serverIpAddress != null)
            {
                try
                {
                    Program.Client = new TcpClient(_serverIpAddress, _serverPort);

                    if (Program.Client.Connected)
                    {
                        Program.statusConnect = "Connection successfully!";
                        Program.isConnected = true;
                    }
                    else
                    {
                        // Log the fact that the connection attempt failed
                        Console.WriteLine("Failed to connect to PLC");
                        Program.statusConnect = "Failed to connect to PLC";
                        Program.isConnected = false;
                    }
                }
                catch (SocketException ex)
                {
                    // Log the socket exception for debugging purposes
                    Console.WriteLine($"SocketException: {ex.Message}");
                    Program.statusConnect = $"SocketException: {ex.Message}";
                    Program.isConnected = false;
                }
                catch (Exception ex)
                {
                    // Log other exceptions for debugging purposes
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Program.statusConnect = $"An error occurred: {ex.Message}";
                    Program.isConnected = false;
                }
            }
            
        }
    }
}
