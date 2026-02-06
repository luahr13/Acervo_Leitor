using System.ComponentModel.DataAnnotations;

namespace Acervo_Leitor.Models
{
    public class Livro
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; }

        [Required]
        [StringLength(150)]
        public string Autor { get; set; }

        [Display(Name = "Código de identificação")]
        [Required]
        [StringLength(30)]
        public string CodigoExemplar { get; set; }

        public bool Ativo { get; set; } = true;

        // Navegação
        public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
    }
}
