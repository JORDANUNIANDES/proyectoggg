using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgenciaPublicidad.Data;
using AgenciaPublicidad.Models;

namespace AgenciaPublicidad.Controllers
{
    public class DisenadoresController : Controller
    {
        private readonly AgenciaDbContext _context;

        public DisenadoresController(AgenciaDbContext context)
        {
            _context = context;
        }

        // GET: Disenadores
        public async Task<IActionResult> Index()
        {
            return View(await _context.Disenadores.ToListAsync());
        }

        // GET: Disenadores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disenador = await _context.Disenadores
                .FirstOrDefaultAsync(m => m.disenador_id == id);
            if (disenador == null)
            {
                return NotFound();
            }

            return View(disenador);
        }

        // GET: Disenadores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Disenadores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("disenador_id,nombre,especialidad,email,telefono")] Disenador disenador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(disenador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(disenador);
        }

        // GET: Disenadores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disenador = await _context.Disenadores.FindAsync(id);
            if (disenador == null)
            {
                return NotFound();
            }
            return View(disenador);
        }

        // POST: Disenadores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("disenador_id,nombre,especialidad,email,telefono")] Disenador disenador)
        {
            if (id != disenador.disenador_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(disenador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DisenadorExists(disenador.disenador_id))
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
            return View(disenador);
        }

        // GET: Disenadores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disenador = await _context.Disenadores
                .FirstOrDefaultAsync(m => m.disenador_id == id);
            if (disenador == null)
            {
                return NotFound();
            }

            return View(disenador);
        }

        // POST: Disenadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var disenador = await _context.Disenadores.FindAsync(id);
            if (disenador != null)
            {
                _context.Disenadores.Remove(disenador);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DisenadorExists(int id)
        {
            return _context.Disenadores.Any(e => e.disenador_id == id);
        }
    }
}
