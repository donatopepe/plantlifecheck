using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Areas.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using plantlifecheck.Data;
using plantlifecheck.Models;

namespace plantlifecheck
{
    [Authorize]
    public class ComponentiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<CustomUser> userManager;
        private RoleManager<CustomRole> roleManager;

        public ComponentiController(ApplicationDbContext context, RoleManager<CustomRole> roleMgr, UserManager<CustomUser> userMrg)
        {
            _context = context;
            userManager = userMrg;
            roleManager = roleMgr;
        }


        // GET: Componenti
        public async Task<IActionResult> Index(long? id_macchina)
        {

            if (id_macchina == null)
            {
                return NotFound();
            }
            ViewBag.id_macchina = id_macchina;

            var macchina = await _context.Macchina.Include(m => m.Impianto).FirstOrDefaultAsync(m => m.Id == id_macchina);
            if (macchina == null)
            {
                return NotFound();
            }
            ViewBag.id_impianto = macchina.Impianto.Id;
            return View(await _context.Componente.Include(s => s.Utente).Include(m => m.Macchina).ThenInclude(n => n.Nome).Where(n => n.Macchina.Id == id_macchina).ToListAsync());


            //return View(await _context.Componente.ToListAsync());
        }

        // GET: Componenti/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var componente = await _context.Componente.Include(s => s.Macchina).ThenInclude(n=>n.Nome).Include(s => s.Utente).FirstOrDefaultAsync(m => m.Id == id);
            if (componente == null)
            {
                return NotFound();
            }

            return View(componente);
        }

        // GET: Componenti/Create
        public IActionResult Create(long? id_macchina)
        {
            if (id_macchina == null)
            {
                return NotFound();
            }
            ViewBag.id_macchina = id_macchina;
            return View();
        }

        // POST: Componenti/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Decrizione")] Componente componente, long id_macchina)
        {
            if (ModelState.IsValid)
            {
                var componCheck = await _context.Componente.Include(s => s.Macchina).Include(s => s.Utente).FirstOrDefaultAsync(m => (m.Macchina.Id== id_macchina) && (m.Nome == componente.Nome));
                if (componCheck!=null)
                {
                    ModelState.AddModelError(string.Empty, "Il nome deve essere univoco");
                    ViewBag.id_macchina = id_macchina;
                    return View(componente);
                }


                var macchina = await _context.Macchina.Include(s=>s.Impianto).FirstOrDefaultAsync(m => m.Id == id_macchina);
                if (macchina == null)
                {
                    
                    return NotFound();
                }
                
                componente.Macchina = macchina;
                componente.Impianto = macchina.Impianto;
                componente.UltimaModifica = DateTime.Now;

                CustomUser user = await userManager.FindByNameAsync(User.Identity.Name);
                if (user != null)
                {
                    componente.Utente = user;
                }

                _context.Add(componente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id_macchina });
            }
            ViewBag.id_macchina = id_macchina;
            return View(componente);
        }

        // GET: Componenti/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var componente = await _context.Componente.Include(m => m.Macchina).FirstOrDefaultAsync(m => m.Id == id);
            if (componente == null)
            {
                return NotFound();
            }
            return View(componente);
        }

        // POST: Componenti/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Nome,Decrizione,RowVersion")] Componente componente)
        {
            var componente_old = await _context.Componente.AsNoTracking().Include(m => m.Macchina).FirstOrDefaultAsync(m => m.Id == id);
            if ((id != componente.Id)|| (componente_old==null))
            {
                return NotFound();
            }
            if (componente_old.Nome!= componente.Nome) {
                var componCheck = await _context.Componente.Include(s => s.Macchina).Include(s => s.Utente).FirstOrDefaultAsync(m => (m.Macchina.Id == componente_old.Macchina.Id) && (m.Nome == componente.Nome));
                if (componCheck != null)
                {
                    ModelState.AddModelError(string.Empty, "Il nome deve essere univoco");
                    ViewBag.id_macchina = componente_old.Macchina.Id;
                    return View(componente_old);
                }
            }
            if ((componente_old != null) && (!componente_old.RowVersion.SequenceEqual(componente.RowVersion)))
            {
                ModelState.AddModelError(string.Empty, "Il record che hai editato è stato modificato da un'altro utente, la tua modifica è stata annullata");
                var id_macchina = componente_old.Macchina.Id;
                ViewBag.id_macchina = id_macchina;
                return View(await _context.Componente.Include(s => s.Utente).Include(s => s.Macchina).ThenInclude(n => n.Nome).Where(n => n.Macchina.Id == id_macchina).ToListAsync());
            }

            if (ModelState.IsValid)
            {
                try
                {
                    componente.UltimaModifica = DateTime.Now;

                    CustomUser user = await userManager.FindByNameAsync(User.Identity.Name);
                    if (user != null)
                    {
                        componente.Utente = user;
                    }
                    _context.Update(componente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComponenteExists(componente.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id_macchina = componente_old.Macchina.Id});
            }
            return View(componente);
        }

        // GET: Componenti/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var componente = await _context.Componente.Include(s => s.Macchina).Include(s => s.Utente).FirstOrDefaultAsync(m => m.Id == id);
            if (componente == null)
            {
                return NotFound();
            }

            return View(componente);
        }

        // POST: Componenti/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id, byte[] rowVersion)
        {
            var componente = await _context.Componente.Include(s => s.Macchina).ThenInclude(n => n.Nome).FirstOrDefaultAsync(m => m.Id == id); ;
            var id_macchina = componente.Macchina.Id;
            if ((componente != null) && (!componente.RowVersion.SequenceEqual(rowVersion)))
            {
                ModelState.AddModelError(string.Empty, "Il record non è stato cancellato perchè è stato modificato da un'altro utente");

                ViewBag.id_macchina = id_macchina;
                return View(await _context.Componente.Include(s => s.Utente).Include(s => s.Macchina).ThenInclude(n => n.Nome).Where(n => n.Macchina.Id == id_macchina).ToListAsync());
            }


            _context.Componente.Remove(componente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id_macchina });
        }

        private bool ComponenteExists(long id)
        {
            return _context.Componente.Any(e => e.Id == id);
        }
    }
}
