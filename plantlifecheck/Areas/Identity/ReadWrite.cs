using Areas.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using plantlifecheck.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Areas.Identity
{
    public class ReadWrite
    {


        private readonly ApplicationDbContext _context;
        private readonly IServiceProvider _serviceProvider;


        public ReadWrite(IHost host)
        {
            var scope = host.Services.CreateScope();
            _serviceProvider = scope.ServiceProvider;
            _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        }

        public bool isEnable(string RoleName, ClaimsPrincipal claimsuser)
        {
            
            //Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>claimsuser< " + claimsuser.Identity.Name);
            CustomRole role = _context.Roles.AsNoTracking().FirstOrDefault(n => n.Name == RoleName);
            if (role != null)
            {
                //_context.Entry(role).Reload();
                var user = _context.Users.AsNoTracking().FirstOrDefault(n => n.UserName == claimsuser.Identity.Name);
                if (user != null)
                {
                    //_context.Entry(user).Reload();

                    //var CustomUserRole = _context.CustomUserRole.Find(user.Id, role.Id);
                    var CustomUserRole = _context.CustomUserRole.AsNoTracking().Where(c => c.UserId == user.Id && c.RoleId == role.Id).FirstOrDefault();
                    

                    if (CustomUserRole != null)
                    {
                        
                        //if ((role.Name == "Admin") || ((role.Name == "Manager"))) return true;

                        //_context.Entry(CustomUserRole).Reload();

                        //return CustomUserRole.ReadWrite ? true : false;
                        if (CustomUserRole.ReadWrite)
                        {
                            return true;
                        } else
                        {
                            var roleAdmin = _context.Roles.AsNoTracking().FirstOrDefault(n => n.Name == "Admin");
                            var CustomAdminRole = _context.CustomUserRole.AsNoTracking().Where(c => c.UserId == user.Id && c.RoleId == roleAdmin.Id).FirstOrDefault();
                            if ((roleAdmin!=null)&&(CustomAdminRole != null))
                            {
                                return true;
                            }
                            else
                            {
                                var roleManager = _context.Roles.AsNoTracking().FirstOrDefault(n => n.Name == "Manager");
                                var CustomManagerRole = _context.CustomUserRole.AsNoTracking().Where(c => c.UserId == user.Id && c.RoleId == roleManager.Id).FirstOrDefault();
                                if ((roleManager != null) && (CustomAdminRole != null))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
