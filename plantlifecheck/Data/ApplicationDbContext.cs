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
using Areas.Identity.Pages.Account;
using plantlifecheck.Controllers;
using plantlifecheck.Models;
using Areas.User;

namespace plantlifecheck.Data
{
    public class ApplicationDbContext : CustomDbContext
    { 
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger)
            : base(options, logger)
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
            modelBuilder.Entity<Impianto>(j =>
            {
                j.Property(pt => pt.UltimaModifica).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            modelBuilder.Entity<Macchina>(j =>
            {
                j.Property(pt => pt.UltimaModifica).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            modelBuilder.Entity<Componente>(j =>
            {
                j.Property(pt => pt.UltimaModifica).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            modelBuilder.Entity<ControlloVita>(j =>
            {
                j.Property(pt => pt.UltimaModifica).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            
            base.OnModelCreating(modelBuilder);
        }


        public DbSet<plantlifecheck.Models.Impianto> Impianto { get; set; }
        public DbSet<plantlifecheck.Models.Macchina> Macchina { get; set; }
        public DbSet<plantlifecheck.Models.Componente> Componente { get; set; }
        public DbSet<plantlifecheck.Models.ControlloVita> ControlloVita { get; set; }
       
    }
}
