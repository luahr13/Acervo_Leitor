using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Acervo_Leitor.Models
{
    public class Emprestimo
    {
        public int Id { get; set; }


        [Display(Name = "Aluno")]
        [Required]
        public int? AlunoId { get; set; }


        [Display(Name = "Livro")]
        [Required]
        public int? LivroId { get; set; }

        [Display(Name = "Data do Empréstimo")]
        [Required]
        public DateTime DataEmprestimo { get; set; } = DateTime.Now;

        [Display(Name = "Devolver até")]
        [Required]
        public DateTime? DataPrevistaDevolucao { get; set; }

        [Display(Name = "Devolvido em")]
        public DateTime? DataDevolucao { get; set; }

        // Navegação
        public Aluno? Aluno { get; set; }
        public Livro? Livro { get; set; }

        // Propriedade computada para status
        [NotMapped]
        public string Status
        {
            get
            {
                if (DataDevolucao != null) return "Baixa"; // Verde
                if (DateTime.Now > DataPrevistaDevolucao) return "Atraso"; // Vermelho
                return "Aberto"; // Amarelo
            }
        }
    }
}
