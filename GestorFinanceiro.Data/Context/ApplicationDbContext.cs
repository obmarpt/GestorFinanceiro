using GestorFinanceiro.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace GestorFinanceiro.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<SessaoGestao> SessoesGestao { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<SessaoCategoria> SessaoCategorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SessaoCategoria>()
                .HasKey(sc => new { sc.SessaoGestaoId, sc.CategoriaId });

            modelBuilder.Entity<SessaoCategoria>()
                .HasOne(sc => sc.SessaoGestao)
                .WithMany(s => s.SessaoCategorias)
                .HasForeignKey(sc => sc.SessaoGestaoId);

            modelBuilder.Entity<SessaoCategoria>()
                .HasOne(sc => sc.Categoria)
                .WithMany(c => c.SessaoCategorias)
                .HasForeignKey(sc => sc.CategoriaId);
        }
    }
}

