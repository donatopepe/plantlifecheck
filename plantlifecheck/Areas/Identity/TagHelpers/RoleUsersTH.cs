using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Areas.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Areas.User;

namespace Areas.TagHelpers
{

        [HtmlTargetElement("td", Attributes = "i-role")]
        public class RoleUsersTH : TagHelper
        {
            private UserManager<CustomUser> userManager;
            private RoleManager<CustomRole> roleManager;

            public RoleUsersTH(UserManager<CustomUser> usermgr, RoleManager<CustomRole> rolemgr)
            {
                userManager = usermgr;
                roleManager = rolemgr;
            }

            [HtmlAttributeName("i-role")]
            public string Role { get; set; }

            public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
            {
                List<string> names = new List<string>();
                CustomRole role = await roleManager.FindByIdAsync(Role);
                if (role != null)
                {
                    foreach (var user in userManager.Users)
                    {
                        if (user != null && await userManager.IsInRoleAsync(user, role.Name))
                            names.Add(user.UserName);
                    }
                }
            Console.WriteLine(">>>>>>>>>>>>>Count:"+names.Count);
                output.Content.SetContent(names.Count == 0 ? "No Users" : string.Join(", ", names));
            }
        }
 
}
