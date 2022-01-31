﻿#nullable disable
using Garage_2._0.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Garage_2._0.Controllers.VehiclesController
{
    public class VehiclesController : Controller
    {
        private readonly GarageVehicleContext _context;

        public VehiclesController(GarageVehicleContext context)
        {
            _context = context;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Vehicle.ToListAsync());
        }

        

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicle
                .FirstOrDefaultAsync(m => m.License == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Type,License,Color,Make,Model,Wheels")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                vehicle.Arrival = DateTime.Now; // This is what I did instead and it works. However now the edit part is a problem instead...I think I fix it now
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vehicle);
        }

        // GET: Vehicles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicle.FindAsync(id);
            
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Type,License,Color,Make,Model,Wheels")] Vehicle vehicle)
        {
            if (id != vehicle.License)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //This was the way I found to not make the arrival date change
                    _context.Entry(vehicle).Property(v => v.Type).IsModified = true;
                    _context.Entry(vehicle).Property(v => v.Color).IsModified = true;
                    _context.Entry(vehicle).Property(v => v.Make).IsModified = true;
                    _context.Entry(vehicle).Property(v => v.Model).IsModified = true;
                    _context.Entry(vehicle).Property(v => v.Wheels).IsModified = true;
                    
                    //_context.Update(vehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehicleExists(vehicle.License))
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
            return View(vehicle);
        }

        // GET: Vehicles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicle
                .FirstOrDefaultAsync(m => m.License == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var vehicle = await _context.Vehicle.FindAsync(id);
            _context.Vehicle.Remove(vehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(string id)
        {
            return _context.Vehicle.Any(e => e.License == id);
        }

        //Calculate Total Parked Time + View Model for the Receipt
        public async Task<IActionResult> ReceiptView(string regNo)
        {
            //regNo should come from check-out so
            //I am supply "Test123" as sample License to check
            Vehicle vehicle = _context.Vehicle.Find("Test123");
            Receipt receipt = new Receipt();
            if (vehicle != null)
            {
                receipt.Type = vehicle.Type;
                receipt.License = vehicle.License;
                receipt.Arrival = vehicle.Arrival;
                receipt.CheckOut=DateTime.Now;

                //Calculating Total Parked Time

                TimeSpan totalParkedTime = DateTime.Now.Subtract(vehicle.Arrival);

                receipt.ParkingDuration = totalParkedTime;
                double cost =  (totalParkedTime.Hours*20) + (totalParkedTime.Minutes*0.33);
                cost = Math.Round(cost, 2);
                receipt.Price = cost + "Sek";
            }
            else
                return NotFound();

            return View(nameof(ReceiptView), receipt);
        }
    }
}
