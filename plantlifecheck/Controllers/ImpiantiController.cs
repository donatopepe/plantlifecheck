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
    public class ImpiantiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<CustomUser> userManager;
        private RoleManager<CustomRole> roleManager;

        public ImpiantiController(ApplicationDbContext context, RoleManager<CustomRole> roleMgr, UserManager<CustomUser> userMrg)
        {
            _context = context;
            userManager = userMrg;
            roleManager = roleMgr;
        }

        // GET: Impianti
        public async Task<IActionResult> Index()
        {
            return View(await _context.Impianto.Include(m => m.Nome).Include(s => s.Utente).ToListAsync());
        }

        // GET: Impianti/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var impianto = await _context.Impianto.Include(m => m.Nome).Include(s => s.Utente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (impianto == null)
            {
                return NotFound();
            }

            return View(impianto);
        }

        // GET: Impianti/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Impianti/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Decrizione")] Impianto impianto, [Bind("Name")] string name)
        {

            if (ModelState.IsValid)
            {

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
                    return View(impianto);
                }


                CustomUser user = await userManager.FindByNameAsync(User.Identity.Name);
                if (user != null)
                {
                    IdentityResult result = await userManager.AddToRoleAsync(user, role.Name);
                    //if (!result.Succeeded)
                    //    Console.WriteLine(">>>>>>>>>>>>>>>>>error create role add user");
                }
                impianto.Nome = role;
                impianto.Utente = user;
                impianto.UltimaModifica = DateTime.Now;
                _context.Add(impianto);
                await _context.SaveChangesAsync();
                var CustomUserRole = _context.CustomUserRole.Find(user.Id, role.Id);
                Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>trovato :" + CustomUserRole.UserId);
                CustomUserRole.ReadWrite = true;
                _context.CustomUserRole.Update(CustomUserRole);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(impianto);
        }

        // GET: Impianti/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var impianto = await _context.Impianto.Include(m => m.Nome).FirstOrDefaultAsync(m => m.Id == id);
            if (impianto == null)
            {
                return NotFound();
            }
            return View(impianto);
        }

        // POST: Impianti/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Nome,Decrizione,RowVersion")] Impianto impianto)
        {
            var impianto_old = await _context.Impianto.Include(m => m.Nome).AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if ((id != impianto.Id) || (impianto_old == null))
            {
                return NotFound();
            }
            //Console.WriteLine("impianto_old.RowVersion:" + BitConverter.ToString(impianto_old.RowVersion));
            //Console.WriteLine("impianto.RowVersion:" + BitConverter.ToString(impianto.RowVersion));
            //Console.WriteLine("impianto.RowVersion check:" + impianto_old.RowVersion.SequenceEqual(impianto.RowVersion));
            if ((impianto_old != null)&&(!impianto_old.RowVersion.SequenceEqual(impianto.RowVersion)))
            {
                ModelState.AddModelError(string.Empty, "Il record che hai editato è stato modificato da un'altro utente, la tua modifica è stata annullata");
                return View("index",await _context.Impianto.Include(m => m.Nome).ToListAsync());
            }
            if (ModelState.IsValid)
            {
                try
                {

                    CustomUser user = await userManager.FindByNameAsync(User.Identity.Name);
                    CustomRole role;
                    if (impianto_old != null)
                    {
                        CustomRole role_old = await roleManager.FindByNameAsync(impianto_old.Nome.Name.ToUpper().Trim());
                        if (role_old != null)
                        {

                            if (role_old.Name != impianto.Nome.Name.ToUpper().Trim())
                            {
                                CustomRole rolecheck = await roleManager.FindByNameAsync(impianto.Nome.Name.ToUpper().Trim());
                                if (rolecheck != null)
                                {
                                    ModelState.AddModelError(string.Empty, "Il nome deve essere univoco");
                                    return View(impianto);
                                }
                                role_old.Name = impianto.Nome.Name.ToUpper().Trim();
                                IdentityResult result = await roleManager.UpdateAsync(role_old);
                            }
                            role = role_old;
                        }
                        else
                        {
                            role = new CustomRole(impianto.Nome.Name.ToUpper().Trim());
                            IdentityResult result = await roleManager.CreateAsync(role);
                        }
                        if (user != null)
                        {
                            IdentityResult result = await userManager.AddToRoleAsync(user, role.Name);
                            //if (!result.Succeeded)
                            //    Console.WriteLine(">>>>>>>>>>>>>>>>>error create role add user");
                        }
                        impianto.Nome = role;
                        impianto.Utente = user;
                        impianto.UltimaModifica = DateTime.Now;
                        _context.Update(impianto);
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
                    if (!ImpiantoExists(impianto.Id))
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
            return View(impianto);
        }

        // GET: Impianti/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var impianto = await _context.Impianto.Include(m => m.Nome).Include(s => s.Utente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (impianto == null)
            {
                return NotFound();
            }

            return View(impianto);
        }

        // POST: Impianti/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id, byte[] rowVersion)
        {
            var impianto = await _context.Impianto.Include(m => m.Nome).Include(n => n.Macchine).FirstOrDefaultAsync(m => m.Id == id); ;
            if ((impianto != null) && (!impianto.RowVersion.SequenceEqual(rowVersion)))

            {
                ModelState.AddModelError(string.Empty, "Il record non è stato cancellato perchè è stato modificato da un'altro utente");
                return View("index", await _context.Impianto.Include(m => m.Nome).ToListAsync());
            }
            _context.Roles.Remove(impianto.Nome);
            _context.RemoveRange(impianto.Macchine);
            _context.Impianto.Remove(impianto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImpiantoExists(long id)
        {
            return _context.Impianto.Any(e => e.Id == id);
        }
    }
}
