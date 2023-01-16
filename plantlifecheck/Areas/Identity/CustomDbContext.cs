using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using IdentityRole = Microsoft.AspNetCore.Identity.IdentityRole;
using IdentityUser = Microsoft.AspNetCore.Identity.IdentityUser;


namespace Areas.User
{

    public class CustomUser : IdentityUser
    {
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "LastAccess")]
        public DateTime LastAccess { get; set; }

        [Display(Name = "CodePhone")]
        public int CodePhone { get; set; }

        //public virtual ICollection<CustomUserClaim> Claims { get; set; }
        //public virtual ICollection<CustomUserLogin> Logins { get; set; }
        //public virtual ICollection<CustomUserToken> Tokens { get; set; }
        //public virtual ICollection<CustomUserRole> UserRoles { get; set; }
    }

    public class CustomRole : IdentityRole
    {
        public CustomRole(string roleName) : base(roleName)
        {
        }
        public CustomRole() : base()
        {
        }
        //public virtual ICollection<CustomUserRole> UserRoles { get; set; }
        //public virtual ICollection<CustomRoleClaim> RoleClaims { get; set; }
    }

    public class CustomUserRole : Microsoft.AspNetCore.Identity.IdentityUserRole<string>
    {
        // public virtual CustomUser User { get; set; }
        // public virtual CustomRole Role { get; set; }
        public virtual bool ReadWrite { get; set; }
    }

    public class CustomUserClaim : Microsoft.AspNetCore.Identity.IdentityUserClaim<string>
    {
        // public virtual CustomUser User { get; set; }
    }

    public class CustomUserLogin : Microsoft.AspNetCore.Identity.IdentityUserLogin<string>
    {
        // public virtual CustomUser User { get; set; }
    }

    public class CustomRoleClaim : IdentityRoleClaim<string>
    {
        //public virtual CustomRole Role { get; set; }
    }

    public class CustomUserToken : IdentityUserToken<string>
    {
        //public virtual CustomUser User { get; set; }
    }

    public class CustomDbContext
        : IdentityDbContext<
            CustomUser, CustomRole, string,
            CustomUserClaim, CustomUserRole, CustomUserLogin,
            CustomRoleClaim, CustomUserToken>
    {

        private readonly ILogger<CustomDbContext> _logger;

        public CustomDbContext(DbContextOptions options, ILogger<CustomDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.LogTo(c => _logger.LogInformation(c), (e, l) => e == RelationalEventId.CommandError);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CustomUser>(b =>
            {
            //// Each User can have many UserClaims
            //b.HasMany(e => e.Claims)
            //    .WithOne()
            //    .HasForeignKey(uc => uc.UserId)
            //    .IsRequired();

            //// Each User can have many UserLogins
            //b.HasMany(e => e.Logins)
            //    .WithOne()
            //    .HasForeignKey(ul => ul.UserId)
            //    .IsRequired();

            //// Each User can have many UserTokens
            //b.HasMany(e => e.Tokens)
            //    .WithOne()
            //    .HasForeignKey(ut => ut.UserId)
            //    .IsRequired();

            //// Each User can have many entries in the UserRole join table
            //b.HasMany(e => e.UserRoles)
            //    .WithOne()
            //    .HasForeignKey(ur => ur.UserId)
            //    .IsRequired();
        });

            modelBuilder.Entity<CustomRole>(b =>
            {
            //// Each Role can have many entries in the UserRole join table
            //b.HasMany(e => e.UserRoles)
            //    .WithOne()
            //    .HasForeignKey(ur => ur.RoleId)
            //    .IsRequired();

            //// Each Role can have many associated RoleClaims
            //b.HasMany(e => e.RoleClaims)
            //    .WithOne()
            //    .HasForeignKey(rc => rc.RoleId)
            //    .IsRequired();
        });

            base.OnModelCreating(modelBuilder);
        }


        public DbSet<CustomUserRole> CustomUserRole { get; set; }
    }


}
