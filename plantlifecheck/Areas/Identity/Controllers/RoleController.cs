using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Areas.User;
using plantlifecheck.Data;
using System.Text.Json;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Areas.Controllers
{


    public class RoleModification
    {
        public string RoleId { get; set; }
        public string UserId { get; set; }
        public string IsInRole { get; set; }
        public string ReadWrite { get; set; }
    }

    public class RoleEdit
    {
        public RoleEdit()
        {
            Users = new List<RoleUser>();
        }
        public CustomRole Role { get; set; }
        //public IEnumerable<IdentityUser> Members { get; set; }
        //public IEnumerable<IdentityUser> NonMembers { get; set; }
        public List<RoleUser> Users { get; set; }

    }
    public class RoleUser
    {
        public CustomRole Role { get; set; }
        public CustomUser User { get; set; }
        public string IsInRole { get; set; }
        public string ReadWrite { get; set; }

    }

    
    [Area("Identity")]
    [Authorize(Roles = "Admin,Manager")]
    public class RoleController : Controller
    {
        private RoleManager<CustomRole> roleManager;
        private readonly ApplicationDbContext _context;
        private UserManager<CustomUser> userManager;


        public RoleController(ApplicationDbContext context, RoleManager<CustomRole> roleMgr, UserManager<CustomUser> userMrg)
        {
            _context = context;
            roleManager = roleMgr;
            userManager = userMrg;
        }
        public ViewResult Index() => View(roleManager.Roles);

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create([Required]string name)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await roleManager.CreateAsync(new CustomRole(name.ToUpper().Trim()));
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            return View(name);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            CustomRole role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                IdentityResult result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "No role found");
            return View("Index", roleManager.Roles);
        }



        // removed for clarity

        public async Task<IActionResult> Update(string id)
        {
            CustomRole role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                List<IdentityUser> members = new List<IdentityUser>();
                List<IdentityUser> nonMembers = new List<IdentityUser>();
                //foreach (CustomUser user in userManager.Users)
                //{
                //    var list = await userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                //    list.Add(user);
                //}
                //return View(new RoleEdit
                //{
                //    Role = role,
                //    Members = members,
                //    NonMembers = nonMembers
                //});

                //return View(userManager.Users.ToList());
                var roleedit = new RoleEdit();
                roleedit.Role = role;
                foreach (CustomUser user in userManager.Users)
                {
                    var userrole = new RoleUser();
                    userrole.User = user;
                    userrole.IsInRole = await userManager.IsInRoleAsync(user, role.Name) ? "true" : "false";
                    var CustomUserRole = _context.CustomUserRole.Find(user.Id, role.Id);
                    if (CustomUserRole != null)
                    {
                        userrole.ReadWrite = CustomUserRole.ReadWrite ? "true" : "false";
                    }
                    else
                    {
                        userrole.ReadWrite = "false";
                    }


                    roleedit.Users.Add(userrole);
                }

                return View(roleedit);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        //public async Task<ActionResult> UpdateOnChangeAsync(string RoleId, string UserId, string IsInRole, string ReadWrite)
        public async Task<ActionResult> UpdateOnChangeAsync(RoleModification model)
        {
            Console.WriteLine(">>>>>>>>>>>>>>>>" + JsonSerializer.Serialize(model));
            var RoleId = model.RoleId;
            var UserId = model.UserId;
            var IsInRole = model.IsInRole;
            var ReadWrite = model.ReadWrite;

            IdentityResult result;
            if (ModelState.IsValid)
            {
                Console.WriteLine(">>>>>>>"+ RoleId);
                Console.WriteLine(">>>>>>>" + UserId);
                Console.WriteLine(">>>>>>>" + IsInRole ?? "-");
                Console.WriteLine(">>>>>>>" + ReadWrite ?? "-");

                CustomUser user = await userManager.FindByIdAsync(UserId);
                CustomRole role = await roleManager.FindByIdAsync(RoleId);

                if ((user != null)&&(RoleId != null))
                {
                    if (IsInRole == "true")
                    {
                        Console.WriteLine(">>>>>>> add user to role");
                        result = await userManager.AddToRoleAsync(user, role.Name);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                    if (IsInRole == "false")
                    {
                        Console.WriteLine(">>>>>>> remove user to role");
                        result = await userManager.RemoveFromRoleAsync(user, role.Name);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                    if (ReadWrite == "true")
                    {
                        var CustomUserRole = _context.CustomUserRole.Find(user.Id, role.Id);
                        if (CustomUserRole != null)
                        {
                            Console.WriteLine(">>>>>>> add user to ReadWrite");
                            CustomUserRole.ReadWrite = true;
                            _context.CustomUserRole.Update(CustomUserRole);
                            _context.SaveChanges();
                        }
                        
                    }
                    if (ReadWrite == "false")
                    {
                        var CustomUserRole = _context.CustomUserRole.Find(user.Id, role.Id);
                        if (CustomUserRole != null)
                        {
                            Console.WriteLine(">>>>>>> remove user to ReadWrite");
                            CustomUserRole.ReadWrite = false;
                            _context.CustomUserRole.Update(CustomUserRole);
                            _context.SaveChanges();
                        }
                    }
                }

            }

            return Json(RoleId);
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
    }
}
