using Areas.User;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using plantlifecheck.Data;
using Serilog;
using Serilog.Formatting.Compact;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace plantlifecheck
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Directory.CreateDirectory("logs");
            Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(new RenderedCompactJsonFormatter(), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
            .WriteTo.File(new RenderedCompactJsonFormatter(), "logs/plantlifecheck.ndjson", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10, fileSizeLimitBytes: 104857600, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
            .CreateLogger();

            var host = CreateHostBuilder(args).Build();

            //ChiamataProva();

            CreateDbIfNotExists(host);
            Log.Information("Application start");
            host.Run();
            Log.Information("Application stop");
        }

        private static void ChiamataProva()
        {
            Log.Information("Chiamata di prova start");




            var connectionStrings = new Dictionary<string, string>();
            connectionStrings.Add("DWH", "");
            var connections = new Connection(connectionStrings);
            var parameters = new Dictionary<string, object>();
            parameters.Add("@pSearchDate", DateTime.Today);
            string sql = "spCoilCodeWeightGetSum";
            var storeprocedure = QueryCommandExecutor.ExecuteQueryDictionarySp(connections.Get("DWH"), sql, parameters);

            foreach (var entry in storeprocedure)
                foreach (var colonne in entry)
                    Console.WriteLine("[{0} {1}]", colonne.Key, colonne.Value == null ? "null" : colonne.Value);

            Log.Information("Chiamata di prova start stop");
        }


        private static async Task CreateRoles(IServiceProvider serviceProvider)
        {
            try
            {
                //initializing custom roles 
                var RoleManager = serviceProvider.GetRequiredService<RoleManager<CustomRole>>();
                var UserManager = serviceProvider.GetRequiredService<UserManager<CustomUser>>();
                var applicationDbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var Configuration = serviceProvider.GetRequiredService<IConfiguration>();
                string[] roleNames = { "Admin", "Manager" };
                IdentityResult roleResult;

                foreach (var roleName in roleNames)
                {
                    var roleExist = await RoleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        //create the roles and seed them to the database: Question 1
                        roleResult = await RoleManager.CreateAsync(new CustomRole(roleName));
                    }
                }


                //Ensure you have these values in your appsettings.json file
                string userPWD = Configuration.GetValue<string>("UserPassword");
                string user = Configuration.GetValue<string>("AdminUserEmail");
                //_logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>User:"+ user);
                //_logger.LogInformation(">>>>>>>>>>>>>>>>>>>>>>>Pwd:" + userPWD);
                //Here you could create a super user who will maintain the web app
                var poweruser = new CustomUser
                {

                    UserName = user,
                    Email = user,
                    EmailConfirmed = true,
                    LastAccess = DateTime.Now
                    //Name = user
                };
                var _user = await UserManager.FindByEmailAsync(user);

                if (_user == null)
                {
                    var createPowerUser = await UserManager.CreateAsync(poweruser, userPWD);

                    if (createPowerUser.Succeeded)
                    {
                        //here we tie the new user to the role
                        var addRole = await UserManager.AddToRoleAsync(poweruser, "Admin");
                        if (addRole.Succeeded)
                        {
                            var username = await UserManager.FindByNameAsync(poweruser.UserName);
                            var rolename = await RoleManager.FindByNameAsync("Admin");
                            var CustomUserRole = applicationDbContext.CustomUserRole.Find(username.Id, rolename.Id);
                            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>trovato :" + CustomUserRole.UserId);
                            CustomUserRole.ReadWrite = true;
                            applicationDbContext.CustomUserRole.Update(CustomUserRole);
                            applicationDbContext.SaveChanges();
                        }

                    }

                }
                else
                {

                    var addRole = await UserManager.AddToRoleAsync(_user, "Admin");
                    if (addRole.Succeeded)
                    {
                        var username = await UserManager.FindByNameAsync(_user.UserName);
                        var rolename = await RoleManager.FindByNameAsync("Admin");
                        var CustomUserRole = applicationDbContext.CustomUserRole.Find(username.Id, rolename.Id);
                        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>trovato :" + CustomUserRole.UserId);
                        CustomUserRole.ReadWrite = true;
                        applicationDbContext.CustomUserRole.Update(CustomUserRole);
                        applicationDbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(">>>>>>>>>>>>>>>>>>>>>>>errore connessione database: " + ex.Message);
                //Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>errore connessione database: " + ex.Message);
            }
        }

        private static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    //context.Database.EnsureDeleted();  //in fase di sviluppo ricreo sempre il database
                    context.Database.EnsureCreated();


                    CreateRoles(services).Wait();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("sqlconfig.json", optional: false, reloadOnChange: false);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
