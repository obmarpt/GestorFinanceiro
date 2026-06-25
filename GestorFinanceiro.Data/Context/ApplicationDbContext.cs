using GestorFinanceiro.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<SessaoFinanceira> SessoesFinanceiras { get; set; }
        public DbSet<Receita> Receitas { get; set; }
        public DbSet<ReceitaRecorrente> ReceitasRecorrentes { get; set; }
        public DbSet<Despesa> Despesas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<SessaoFinanceiraCategoria> SessaoFinanceiraCategorias { get; set; }
        public DbSet<Meta> Metas { get; set; }
        public DbSet<MetaMovimento> MetaMovimentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SessaoFinanceira>()
                .HasOne(s => s.Utilizador)
                .WithMany(u => u.SessoesFinanceiras)
                .HasForeignKey(s => s.UtilizadorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Receita>()
                .HasOne(r => r.SessaoFinanceira)
                .WithMany(s => s.Receitas)
                .HasForeignKey(r => r.SessaoFinanceiraId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReceitaRecorrente>()
                .HasOne(rr => rr.SessaoFinanceira)
                .WithMany(s => s.ReceitasRecorrentes)
                .HasForeignKey(rr => rr.SessaoFinanceiraId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Despesa>()
                .HasOne(d => d.SessaoFinanceira)
                .WithMany(s => s.Despesas)
                .HasForeignKey(d => d.SessaoFinanceiraId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessaoFinanceiraCategoria>()
                .HasKey(sc => new { sc.SessaoFinanceiraId, sc.CategoriaId });

            modelBuilder.Entity<SessaoFinanceiraCategoria>()
                .HasOne(sc => sc.SessaoFinanceira)
                .WithMany(s => s.SessaoFinanceiraCategorias)
                .HasForeignKey(sc => sc.SessaoFinanceiraId);

            modelBuilder.Entity<SessaoFinanceiraCategoria>()
                .HasOne(sc => sc.Categoria)
                .WithMany(c => c.SessaoFinanceiraCategorias)
                .HasForeignKey(sc => sc.CategoriaId);

            modelBuilder.Entity<Meta>()
                .HasOne(m => m.Utilizador)
                .WithMany()
                .HasForeignKey(m => m.UtilizadorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MetaMovimento>()
                .HasOne(mm => mm.Meta)
                .WithMany()
                .HasForeignKey(mm => mm.MetaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
