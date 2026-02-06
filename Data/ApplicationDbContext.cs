using Acervo_Leitor.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Acervo_Leitor.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Turma> Turmas { get; set; }
        public DbSet<Livro> Livros { get; set; }
        public DbSet<Emprestimo> Emprestimos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Índice único: Turma (Nome + AnoLetivo + Ativa)
            modelBuilder.Entity<Turma>()
                .HasIndex(t => new { t.Nome, t.AnoLetivo, t.Ativa })
                .IsUnique();

            // Índice único: Código do exemplar do livro
            modelBuilder.Entity<Livro>()
                .HasIndex(l => l.CodigoExemplar)
                .IsUnique();

            // Regra: um livro só pode ter um empréstimo ativo
            modelBuilder.Entity<Emprestimo>()
                .HasIndex(e => new { e.LivroId, e.DataDevolucao })
                .IsUnique();
        }
    }
}
