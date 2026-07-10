using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgenciaPublicidad.Data;
using AgenciaPublicidad.Models;

namespace AgenciaPublicidad.Controllers
{
    public class CampanasController : Controller
    {
        private readonly AgenciaDbContext _context;

        public CampanasController(AgenciaDbContext context)
        {
            _context = context;
        }

        // GET: Campanas
        public async Task<IActionResult> Index()
        {
            var campanas = _context.Campanas.Include(c => c.Cliente);
            return View(await campanas.ToListAsync());
        }

        // GET: Campanas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campana = await _context.Campanas
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(m => m.campana_id == id);
            if (campana == null)
            {
                return NotFound();
            }

            return View(campana);
        }

        // GET: Campanas/Create
        public IActionResult Create()
        {
            ViewData["cliente_id"] = new SelectList(_context.Clientes, "cliente_id", "nombre_empresa");
            return View();
        }

        // POST: Campanas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("campana_id,cliente_id,nombre,presupuesto,fecha_inicio")] Campana campana)
        {
            if (ModelState.IsValid)
            {
                _context.Add(campana);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["cliente_id"] = new SelectList(_context.Clientes, "cliente_id", "nombre_empresa", campana.cliente_id);
            return View(campana);
        }

        // GET: Campanas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campana = await _context.Campanas.FindAsync(id);
            if (campana == null)
            {
                return NotFound();
            }
            ViewData["cliente_id"] = new SelectList(_context.Clientes, "cliente_id", "nombre_empresa", campana.cliente_id);
            return View(campana);
        }

        // POST: Campanas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("campana_id,cliente_id,nombre,presupuesto,fecha_inicio")] Campana campana)
        {
            if (id != campana.campana_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(campana);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CampanaExists(campana.campana_id))
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
            ViewData["cliente_id"] = new SelectList(_context.Clientes, "cliente_id", "nombre_empresa", campana.cliente_id);
            return View(campana);
        }

        // GET: Campanas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campana = await _context.Campanas
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(m => m.campana_id == id);
            if (campana == null)
            {
                return NotFound();
            }

            return View(campana);
        }

        // POST: Campanas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var campana = await _context.Campanas.FindAsync(id);
            if (campana != null)
            {
                _context.Campanas.Remove(campana);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CampanaExists(int id)
        {
            return _context.Campanas.Any(e => e.campana_id == id);
        }
    }
}
