using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using plantlifecheck.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;
using plantlifecheck.Controllers;
using Areas.User;
using Areas.Controllers;
using Areas.TagHelpers;
using Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Areas.Identity;
using plantlifecheck.Models;
using plantlifecheck.Services;


namespace plantlifecheck
{
    public class Startup
    {
        private ILogger<Startup> _logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>Prova invio email");
            //var EmailSettings = new EmailSettings();
            //Configuration.GetSection("EmailSettings").Bind(EmailSettings);
            //IOptions<EmailSettings> myOptions = Options.Create<EmailSettings>(EmailSettings);
            //var loggerFactory = (ILoggerFactory)new LoggerFactory();
            //var logger = loggerFactory.CreateLogger<EmailSender>();
            //EmailSender _emailSender = new EmailSender(myOptions, logger);
            //_emailSender.SendEmailTextAsync("[REDACTED]@[REDACTED].com", "Prova invio sms da plantchecklife", "Prova invio sms da plantchecklife");


        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential 
                // cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                // requires using Microsoft.AspNetCore.Http;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("ApplicationDbContext")));

            services.AddDatabaseDeveloperPageExceptionFilter(); //Fornisce informazioni utili sull'errore nell' ambiente di sviluppo.




            ////// inizio gestione autenticazione utente

            services.AddIdentity<CustomUser, CustomRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultUI()
                    .AddDefaultTokenProviders();

            services.AddRazorPages();

            services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;

                // Password settings.
                //options.Password.RequireDigit = true;
                //options.Password.RequireLowercase = true;
                //options.Password.RequireNonAlphanumeric = true;
                //options.Password.RequireUppercase = true;
                //options.Password.RequiredLength = 6;
                //options.Password.RequiredUniqueChars = 1;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 0;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;

                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                //options.Cookie.Name = "plantlifecheck";
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            ////// fine gestione autenticazione utente
            ///

            services.AddControllersWithViews();

            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<SMSSender>();
            services.AddSingleton<MailSmsSender>();
            services.AddSingleton<AppVersionService>();
            services.AddSingleton<ReadWrite>();

            services.Configure<SqlConfigs>(Configuration.GetSection("SqlConfigs"));
            services.AddHostedService<MyServicePoller>();
        }
        public class YourCustomMiddleware
        {
            private readonly RequestDelegate _next;

            public YourCustomMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext httpContext)
            {
                if (httpContext.User != null && httpContext.User.Identity.IsAuthenticated)
                {

                    httpContext.User.Identities.FirstOrDefault().AddClaim(new Claim("your claim", "your field"));
                }
                await _next(httpContext);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _logger = new Logger<Startup>(loggerFactory);
            //AllarmiSMS allarmiSMS = new AllarmiSMS(loggerFactory);
            //string v = allarmiSMS.ComponiInviaSMS("Prova invio sms PlantLifeCheck", "3283387195");
            //Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>sms:" + v);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();

            app.UseAuthentication();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    name: "MyAreaRoles",
                    areaName: "Identity",
                    pattern: "Role/{controller=Role}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
