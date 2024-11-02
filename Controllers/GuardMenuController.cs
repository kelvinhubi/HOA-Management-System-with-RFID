using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Data;
using Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Models;

namespace Cessna_HOA_MANAGEMENT_SYSTEM_WITH_RFID.Controllers
{
    public class GuardMenuController : Controller
    {
        private readonly AppDbContext _db;

        public GuardMenuController(AppDbContext context)
        {
            _db = context;
        }

        // GET: Guard_Information
        public async Task<IActionResult> Index()
        {
            return View(await _db.Guard_Information.ToListAsync());
        }

        // GET: Guard_Information/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guard_Information = await _db.Guard_Information
                .FirstOrDefaultAsync(m => m.ID == id);
            if (guard_Information == null)
            {
                return NotFound();
            }

            return View(guard_Information);
        }

        // GET: Guard_Information/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Guard_Information/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Username,Password,Name")] Guard_Information guard_Information)
        {
            if (ModelState.IsValid)
            {
                _db.Add(guard_Information);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(guard_Information);
        }

        // GET: Guard_Information/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guard_Information = await _db.Guard_Information.FindAsync(id);
            if (guard_Information == null)
            {
                return NotFound();
            }
            return View(guard_Information);
        }

        // POST: Guard_Information/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Username,Password,Name")] Guard_Information guard_Information)
        {
            if (id != guard_Information.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(guard_Information);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Guard_InformationExists(guard_Information.ID))
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
            return View(guard_Information);
        }

        // GET: Guard_Information/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guard_Information = await _db.Guard_Information
                .FirstOrDefaultAsync(m => m.ID == id);
            if (guard_Information == null)
            {
                return NotFound();
            }

            return View(guard_Information);
        }

        // POST: Guard_Information/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var guard_Information = await _db.Guard_Information.FindAsync(id);
            if (guard_Information != null)
            {
                _db.Guard_Information.Remove(guard_Information);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Guard_InformationExists(int id)
        {
            return _db.Guard_Information.Any(e => e.ID == id);
        }

        public bool CheckRole()
        {
            var usertype = HttpContext.Session.GetString("UserType");
            Console.WriteLine(usertype);
            if (usertype != null)
            {
                if (usertype == "Guard")
                {
                    return true;

                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}
