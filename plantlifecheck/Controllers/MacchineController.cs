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

namespace plantlifecheck.Views
{
    [Authorize]
    public class MacchineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<CustomUser> userManager;
        private RoleManager<CustomRole> roleManager;

        public MacchineController(ApplicationDbContext context, RoleManager<CustomRole> roleMgr, UserManager<CustomUser> userMrg)
        {
            _context = context;
            userManager = userMrg;
            roleManager = roleMgr;
        }

        // GET: Macchine
        public async Task<IActionResult> Index(long? id_impianto)
        {
            if (id_impianto == null)
            {
                return NotFound();
            }
            ViewBag.id_impianto = id_impianto;
            return View(await _context.Macchina.Include(m => m.Nome).Include(s => s.Utente).Include(m => m.Impianto).Where(n => n.Impianto.Id== id_impianto).ToListAsync());
        }

        // GET: Macchine/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var macchina = await _context.Macchina.Include(m => m.Nome).Include(s => s.Utente).Include(m => m.Impianto).FirstOrDefaultAsync(m => m.Id == id);
            if (macchina == null)
            {
                return NotFound();
            }

            return View(macchina);
        }

        // GET: Macchine/Create
        public IActionResult Create(long? id_impianto)
        {
            if (id_impianto == null)
            {
                return NotFound();
            }
            ViewBag.id_impianto = id_impianto;
            return View();
        }

        // POST: Macchine/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Decrizione")] Macchina macchina, [Bind("Name")] string name, long? id_impianto)
        {
            if (ModelState.IsValid)
            {
                var impianto = await _context.Impianto.FirstOrDefaultAsync(m => m.Id == id_impianto);
                if (impianto == null)
                {
                    return NotFound();
                }
                CustomRole role = await roleManager.FindByNameAsync(name.ToUpper().Trim());
                if (role == null)
                {
                    role = new CustomRole(name.ToUpper().Trim());
                    IdentityResult result = await roleManager.CreateAsync(role);
                    //if (result.Succeeded)
                    //    Console.WriteLine(">>>>>>>>>>>>>>>>>OK create role");
                    //else
                    //    Console.WriteLine(">>>>>>>>>>>>>>>>>error create role");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Il nome deve essere univoco");
                    ViewBag.id_impianto = id_impianto;
                    return View(macchina);

                }

                macchina.Nome = role;
                macchina.Impianto = impianto;
                macchina.UltimaModifica = DateTime.Now;

                CustomUser user = await userManager.FindByNameAsync(User.Identity.Name);
                if (user != null)
                {
                    IdentityResult result = await userManager.AddToRoleAsync(user, role.Name);
                    macchina.Utente = user;
                    //if (!result.Succeeded)
                    //    Console.WriteLine(">>>>>>>>>>>>>>>>>error create role add user");
                }


                _context.Add(macchina);
                await _context.SaveChangesAsync();
                //Console.WriteLine(">>>>>>>>>>>>>>>>>macchina crata con successo impianto:"+ id_impianto);
                return RedirectToAction(nameof(Index), new { id_impianto });
            }
            
            ViewBag.id_impianto = id_impianto;
            return View(macchina);
        }

        // GET: Macchine/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var macchina = await _context.Macchina.Include(m => m.Nome).Include(m => m.Impianto).FirstOrDefaultAsync(m => m.Id == id);
            if (macchina == null)
            {
                return NotFound();
            }
            return View(macchina);
        }

        // POST: Macchine/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Nome,Decrizione,RowVersion")] Macchina macchina)
        {
            var macchina_old = await _context.Macchina.Include(m => m.Nome).Include(m => m.Impianto).AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if ((id != macchina.Id)|| (macchina_old == null))
            {
                return NotFound();
            }
            //Console.WriteLine("impianto_old.RowVersion:" + BitConverter.ToString(impianto_old.RowVersion));
            //Console.WriteLine("impianto.RowVersion:" + BitConverter.ToString(impianto.RowVersion));
            //Console.WriteLine("impianto.RowVersion check:" + impianto_old.RowVersion.SequenceEqual(impianto.RowVersion));
            if ((macchina_old != null) && (!macchina_old.RowVersion.SequenceEqual(macchina.RowVersion)))
            {
                ModelState.AddModelError(string.Empty, "Il record che hai editato è stato modificato da un'altro utente, la tua modifica è stata annullata");
                var id_impianto = macchina_old.Impianto.Id;
                ViewBag.id_impianto = id_impianto;
                return View(await _context.Macchina.Include(m => m.Nome).Include(s => s.Utente).Include(m => m.Impianto).Where(n => n.Impianto.Id == id_impianto).ToListAsync());
            }
            if (ModelState.IsValid)
            {
                try
                {
                    CustomUser user = await userManager.FindByNameAsync(User.Identity.Name);
                    CustomRole role;
                    if (macchina_old != null)
                    {
                        CustomRole role_old = await roleManager.FindByNameAsync(macchina_old.Nome.Name.ToUpper().Trim());
                        if (role_old != null)
                        {

                            if (role_old.Name != macchina.Nome.Name.ToUpper().Trim())
                            {
                                CustomRole rolecheck = await roleManager.FindByNameAsync(macchina.Nome.Name.ToUpper().Trim());
                                if (rolecheck != null)
                                {
                                    ModelState.AddModelError(string.Empty, "Il nome deve essere univoco");
                                    return View(macchina_old);
                                }
                                role_old.Name = macchina.Nome.Name.ToUpper().Trim();
                                IdentityResult result = await roleManager.UpdateAsync(role_old);
                            }
                            role = role_old;
                        }
                        else
                        {
                            role = new CustomRole(macchina.Nome.Name.ToUpper().Trim());
                            IdentityResult result = await roleManager.CreateAsync(role);
                        }
                        if (user != null)
                        {
                            IdentityResult result = await userManager.AddToRoleAsync(user, role.Name);
                            //if (!result.Succeeded)
                            //    Console.WriteLine(">>>>>>>>>>>>>>>>>error create role add user");
                        }
                        macchina.Nome = role;
                        macchina.Utente = user;
                        macchina.UltimaModifica = DateTime.Now;
                        _context.Update(macchina);
                        await _context.SaveChangesAsync();
                        var CustomUserRole = _context.CustomUserRole.Find(user.Id, role.Id);
                        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>trovato :" + CustomUserRole.UserId);
                        CustomUserRole.ReadWrite = true;
                        _context.CustomUserRole.Update(CustomUserRole);
                        _context.SaveChanges();
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MacchinaExists(macchina.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id_impianto= macchina_old.Impianto.Id });
            }
            return View(macchina);
        }

        // GET: Macchine/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var macchina = await _context.Macchina.Include(m => m.Nome).Include(s => s.Utente).Include(m => m.Impianto).FirstOrDefaultAsync(m => m.Id == id);
            if (macchina == null)
            {
                return NotFound();
            }

            return View(macchina);
        }

        // POST: Macchine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id, byte[] rowVersion)
        {

            var macchina = await _context.Macchina.Include(m => m.Nome).Include(m => m.Impianto).FirstOrDefaultAsync(m => m.Id == id); ;
            var id_impianto = macchina.Impianto.Id;
            if ((macchina != null) && (!macchina.RowVersion.SequenceEqual(rowVersion)))
            {
                ModelState.AddModelError(string.Empty, "Il record non è stato cancellato perchè è stato modificato da un'altro utente");
                
                ViewBag.id_impianto = id_impianto;
                return View(await _context.Macchina.Include(m => m.Nome).Include(s => s.Utente).Include(m => m.Impianto).Where(n => n.Impianto.Id == id_impianto).ToListAsync());
            }


            _context.Roles.Remove(macchina.Nome);
            _context.Macchina.Remove(macchina);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id_impianto });
        }

        private bool MacchinaExists(long id)
        {
            return _context.Macchina.Any(e => e.Id == id);
        }
    }
}
