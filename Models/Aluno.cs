using System.ComponentModel.DataAnnotations;

namespace Acervo_Leitor.Models
{
    public class Aluno
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nome { get; set; }

        [Display(Name = "Turma")]
        [Required(ErrorMessage = "Selecione uma turma")]
        public int? TurmaId { get; set; }
        public Turma? Turma { get; set; }

        [StringLength(20)]
        [Phone]
        public string Telefone { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
    }
}
