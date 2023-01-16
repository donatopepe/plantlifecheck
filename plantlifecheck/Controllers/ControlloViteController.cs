using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Areas.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using plantlifecheck.Data;
using plantlifecheck.Models;

namespace plantlifecheck.Views
{
    public class ControlloViteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<CustomUser> userManager;
        private RoleManager<CustomRole> roleManager;
        private readonly SqlConfigs _sqlConfigs;

        public ControlloViteController(ApplicationDbContext context, RoleManager<CustomRole> roleMgr, UserManager<CustomUser> userMrg, IOptions<SqlConfigs> sqlConfigs)
        {
            _context = context;
            userManager = userMrg;
            roleManager = roleMgr;
            _sqlConfigs = sqlConfigs.Value;
        }



        // GET: ControlloVite
        public async Task<IActionResult> Index(long? id_componente)
        {
            if (id_componente == null)
            {
                return NotFound();
            }
            ViewBag.id_componente = id_componente;
            var componente = await _context.Componente.Include(s => s.Macchina).FirstOrDefaultAsync(m => m.Id == id_componente);
            if (componente == null)
            {
                return NotFound();
            }
            ViewBag.id_macchina = componente.Macchina.Id;

            return View(await _context.ControlloVita.Include(s => s.Utente).Include(r => r.Componente).Include(m => m.Macchina).ThenInclude(n => n.Nome).Where(n => n.Componente.Id == id_componente).ToListAsync());

            //return View(await _context.ControlloVita.ToListAsync());
        }

        // GET: ControlloVite/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var controlloVita = await _context.ControlloVita.Include(s => s.Utente).Include(r => r.Componente).Include(m => m.Macchina).ThenInclude(n => n.Nome).FirstOrDefaultAsync(m => m.Id == id);
            if (controlloVita == null)
            {
                return NotFound();
            }

            return View(controlloVita);
        }

        // GET: ControlloVite/Create
        public IActionResult Create(long? id_componente)
        {
            if (id_componente == null)
            {
                return NotFound();
            }
            ViewBag.id_componente = id_componente;
            PopulateDropDownList();
            return View(new ControlloVita());
        }

        private void PopulateDropDownList()
        {
            var lista = new List<SelectListItem>();
            foreach (var connection in _sqlConfigs.Configs)
            {
                foreach (var query in connection.Queries)
                {
                    lista.Add(new SelectListItem(query.Name, query.Name));
                }
            }
            ViewBag.StoreProcedure = lista;
        }


        // POST: ControlloVite/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Abilitato,Nome,Descrizione,Partenza,Fermata,SP_Consuntivo_Abilitato,SP_Consuntivo,Consuntivo,Ultimo_aggiornamento_Consuntivo,preallarme_enable,preallarme_testo,preallarme_soglia,preallarme_data,allarme_enable,allarme_testo,allarme_soglia,allarme_data,UltimaModifica,RowVersion")] ControlloVita controlloVita, long id_componente)
        {
            if (ModelState.IsValid)
            {
                var controlloVitaCheck = await _context.ControlloVita.Include(s => s.Utente).Include(r => r.Componente).Include(m => m.Macchina).ThenInclude(n => n.Nome).FirstOrDefaultAsync(m => ((m.Componente.Id == id_componente) && (m.Nome == controlloVita.Nome)));
                if (controlloVitaCheck != null)
                {
                    ModelState.AddModelError(string.Empty, "Il nome deve essere univoco");
                    ViewBag.id_componente = id_componente;
                    PopulateDropDownList();
                    return View(controlloVita);
                }


                var componente = await _context.Componente.Include(s => s.Macchina).Include(s => s.Impianto).FirstOrDefaultAsync(m => m.Id == id_componente);
                if (componente == null)
                {

                    return NotFound();
                }


                controlloVita.Componente = componente;
                controlloVita.Macchina = componente.Macchina;
                controlloVita.Impianto = componente.Impianto;
                controlloVita.UltimaModifica = DateTime.Now;

                CustomUser user = await userManager.FindByNameAsync(User.Identity.Name);
                if (user != null)
                {
                    controlloVita.Utente = user;
                }

                _context.Add(controlloVita);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id_componente });
            }
            ViewBag.id_componente = id_componente;
            return View(controlloVita);
        }

        // GET: ControlloVite/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var controlloVita = await _context.ControlloVita.Include(m => m.Componente).FirstOrDefaultAsync(m => m.Id == id);
            if (controlloVita == null)
            {
                return NotFound();
            }
            PopulateDropDownList();
            return View(controlloVita);
        }

        // POST: ControlloVite/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Abilitato,Nome,Descrizione,Partenza,Fermata,SP_Consuntivo_Abilitato,SP_Consuntivo,preallarme_enable,preallarme_testo,preallarme_soglia,allarme_enable,allarme_testo,allarme_soglia,RowVersion")] ControlloVita controlloVita)
        {
            var controlloVita_old = await _context.ControlloVita.AsNoTracking().Include(m => m.Componente).Include(m => m.Macchina).FirstOrDefaultAsync(m => m.Id == id);
            if ((id != controlloVita.Id) || (controlloVita_old == null))
            {
                return NotFound();
            }
            if (controlloVita_old.Nome != controlloVita.Nome)
            {
                var controlloVitaCheck = await _context.ControlloVita.Include(s => s.Utente).Include(r => r.Componente).Include(m => m.Macchina).ThenInclude(n => n.Nome).FirstOrDefaultAsync(m => ((m.Componente.Id == controlloVita_old.Componente.Id) && (m.Nome == controlloVita.Nome)));
                if (controlloVitaCheck != null)
                {
                    ModelState.AddModelError(string.Empty, "Il nome deve essere univoco");
                    ViewBag.id_componente = controlloVita_old.Componente.Id;
                    PopulateDropDownList();
                    return View(controlloVita_old);
                }
            }
            if ((controlloVita_old != null) && (!controlloVita_old.RowVersion.SequenceEqual(controlloVita.RowVersion)))
            {
                ModelState.AddModelError(string.Empty, "Il record che hai editato è stato modificato da un'altro utente, la tua modifica è stata annullata");
                var id_componente = controlloVita_old.Componente.Id;
                ViewBag.id_componente = id_componente;
                return View(await _context.ControlloVita.Include(s => s.Utente).Include(s => s.Componente).Include(s => s.Macchina).ThenInclude(n => n.Nome).Where(n => n.Componente.Id == id_componente).ToListAsync());
            }


            if (ModelState.IsValid)
            {
                try
                {

                    controlloVita.UltimaModifica = DateTime.Now;

                    CustomUser user = await userManager.FindByNameAsync(User.Identity.Name);
                    if (user != null)
                    {
                        controlloVita.Utente = user;
                    }

                    _context.Update(controlloVita);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ControlloVitaExists(controlloVita.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id_componente = controlloVita_old.Componente.Id });
            }
            return View(controlloVita);
        }

        // GET: ControlloVite/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var controlloVita = await _context.ControlloVita.Include(s => s.Componente).Include(s => s.Macchina).Include(s => s.Utente).FirstOrDefaultAsync(m => m.Id == id);
            if (controlloVita == null)
            {
                return NotFound();
            }

            return View(controlloVita);
        }

        // POST: ControlloVite/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id, byte[] rowVersion)
        {
            var controlloVita = await _context.ControlloVita.Include(s => s.Componente).Include(s => s.Macchina).Include(s => s.Utente).FirstOrDefaultAsync(m => m.Id == id);
            var id_componente = controlloVita.Componente.Id;
            if ((controlloVita != null) && (!controlloVita.RowVersion.SequenceEqual(rowVersion)))
            {
                ModelState.AddModelError(string.Empty, "Il record non è stato cancellato perchè è stato modificato da un'altro utente");

                ViewBag.id_componente = id_componente;
                return View(await _context.ControlloVita.Include(s => s.Utente).Include(s => s.Componente).Include(s => s.Macchina).ThenInclude(n => n.Nome).Where(n => n.Componente.Id == id_componente).ToListAsync());
            }


            _context.ControlloVita.Remove(controlloVita);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id_componente });
        }

        private bool ControlloVitaExists(long id)
        {
            return _context.ControlloVita.Any(e => e.Id == id);
        }
    }
}
