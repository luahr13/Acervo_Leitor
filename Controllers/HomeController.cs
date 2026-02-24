using System.Diagnostics;
using Acervo_Leitor.Models;
using Microsoft.AspNetCore.Mvc;
using Acervo_Leitor.Data;
using Microsoft.EntityFrameworkCore;

namespace Acervo_Leitor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 📚 Livros ativos
            ViewBag.LivrosAtivos = await _context.Livros
                                                .CountAsync(l => l.Ativo == true);

            // 👨‍🎓 Alunos ativos
            ViewBag.AlunosAtivos = await _context.Alunos
                                                .CountAsync(a => a.Ativo == true);

            // 🔄 Empréstimos abertos
            ViewBag.EmprestimosAbertos = await _context.Emprestimos
                                                            .CountAsync(e => e.DataDevolucao == null && e.DataPrevistaDevolucao >= DateTime.Now);

            // 🔄 Empréstimos Baixa
            ViewBag.EmprestimosBaixa = await _context.Emprestimos
                                                            .CountAsync(e => e.DataDevolucao != null);

            // 🔄 Empréstimos atrasados
            ViewBag.EmprestimosAtraso = await _context.Emprestimos
                                                            .CountAsync(e => e.DataDevolucao == null && e.DataPrevistaDevolucao < DateTime.Now);

            // (Opcional) Totais gerais
            ViewBag.TotalLivros = await _context.Livros.CountAsync();
            ViewBag.TotalAlunos = await _context.Alunos.CountAsync();
            ViewBag.TotalEmprestimos = await _context.Emprestimos.CountAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}