using System.ComponentModel.DataAnnotations;

namespace Acervo_Leitor.Models
{
    public class Turma
    {
        public int Id { get; set; }

        [Display(Name = "Nome da turma")]
        [Required]
        [StringLength(50)]
        public string Nome { get; set; } // Ex: "7º A", "3º EM"

        [Display(Name = "Ano letivo")]
        [Required]
        public int AnoLetivo { get; set; } // 2026

        public bool Ativa { get; set; } = true;

        public ICollection<Aluno> Alunos { get; set; } = new List<Aluno>();
    }
}
