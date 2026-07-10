using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgenciaPublicidad.Data;
using AgenciaPublicidad.Models;

namespace AgenciaPublicidad.Controllers
{
    public class EntregablesController : Controller
    {
        private readonly AgenciaDbContext _context;

        public EntregablesController(AgenciaDbContext context)
        {
            _context = context;
        }

        // GET: Entregables
        public async Task<IActionResult> Index()
        {
            var entregables = _context.Entregables
                .Include(e => e.Campana)
                .Include(e => e.Disenador);
            return View(await entregables.ToListAsync());
        }

        // GET: Entregables/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entregable = await _context.Entregables
                .Include(e => e.Campana)
                .Include(e => e.Disenador)
                .FirstOrDefaultAsync(m => m.entregable_id == id);
            if (entregable == null)
            {
                return NotFound();
            }

            return View(entregable);
        }

        // GET: Entregables/Create
        public IActionResult Create()
        {
            ViewData["campana_id"] = new SelectList(_context.Campanas, "campana_id", "nombre");
            ViewData["disenador_id"] = new SelectList(_context.Disenadores, "disenador_id", "nombre");
            return View();
        }

        // POST: Entregables/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("entregable_id,campana_id,disenador_id,tipo,fecha_entrega")] Entregable entregable)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entregable);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["campana_id"] = new SelectList(_context.Campanas, "campana_id", "nombre", entregable.campana_id);
            ViewData["disenador_id"] = new SelectList(_context.Disenadores, "disenador_id", "nombre", entregable.disenador_id);
            return View(entregable);
        }

        // GET: Entregables/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entregable = await _context.Entregables.FindAsync(id);
            if (entregable == null)
            {
                return NotFound();
            }
            ViewData["campana_id"] = new SelectList(_context.Campanas, "campana_id", "nombre", entregable.campana_id);
            ViewData["disenador_id"] = new SelectList(_context.Disenadores, "disenador_id", "nombre", entregable.disenador_id);
            return View(entregable);
        }

        // POST: Entregables/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("entregable_id,campana_id,disenador_id,tipo,fecha_entrega")] Entregable entregable)
        {
            if (id != entregable.entregable_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entregable);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntregableExists(entregable.entregable_id))
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
            ViewData["campana_id"] = new SelectList(_context.Campanas, "campana_id", "nombre", entregable.campana_id);
            ViewData["disenador_id"] = new SelectList(_context.Disenadores, "disenador_id", "nombre", entregable.disenador_id);
            return View(entregable);
        }

        // GET: Entregables/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entregable = await _context.Entregables
                .Include(e => e.Campana)
                .Include(e => e.Disenador)
                .FirstOrDefaultAsync(m => m.entregable_id == id);
            if (entregable == null)
            {
                return NotFound();
            }

            return View(entregable);
        }

        // POST: Entregables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entregable = await _context.Entregables.FindAsync(id);
            if (entregable != null)
            {
                _context.Entregables.Remove(entregable);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EntregableExists(int id)
        {
            return _context.Entregables.Any(e => e.entregable_id == id);
        }
    }
}
