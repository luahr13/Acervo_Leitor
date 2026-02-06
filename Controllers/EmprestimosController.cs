using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Acervo_Leitor.Data;
using Acervo_Leitor.Models;

namespace Acervo_Leitor.Controllers
{
    public class EmprestimosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmprestimosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Emprestimos
        public async Task<IActionResult> Index(string status, string busca)
        {
            var emprestimos = _context.Emprestimos
                .Include(e => e.Aluno)
                .Include(e => e.Livro)
                .AsQueryable();

            // Filtrar pelo status
            if (!string.IsNullOrEmpty(status) && status != "Todos")
            {
                emprestimos = status switch
                {
                    "Baixa" => emprestimos.Where(e => e.DataDevolucao != null),
                    "Aberto" => emprestimos.Where(e => e.DataDevolucao == null && e.DataPrevistaDevolucao >= DateTime.Now),
                    "Atraso" => emprestimos.Where(e => e.DataDevolucao == null && e.DataPrevistaDevolucao < DateTime.Now),
                    _ => emprestimos
                };
            }

            // Filtrar por aluno ou livro
            if (!string.IsNullOrEmpty(busca))
            {
                busca = busca.ToLower();
                emprestimos = emprestimos.Where(e =>
                    e.Aluno.Nome.ToLower().Contains(busca) ||
                    e.Livro.Titulo.ToLower().Contains(busca)
                );
            }

            return View(await emprestimos.ToListAsync());
        }

        // GET: Emprestimos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emprestimo = await _context.Emprestimos
                .Include(e => e.Aluno)
                .Include(e => e.Livro)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (emprestimo == null)
            {
                return NotFound();
            }

            return View(emprestimo);
        }

        // GET: Emprestimos/Create
        public IActionResult Create()
        {
            // Apenas alunos ativos
            ViewData["AlunoId"] = new SelectList(_context.Alunos.Where(a => a.Ativo).OrderBy(a => a.Nome), "Id", "Nome");

            // Apenas livros ativos
            ViewData["LivroId"] = new SelectList(_context.Livros.Where(l => l.Ativo).OrderBy(l => l.Titulo), "Id", "Titulo");

            return View();
        }

        // POST: Emprestimos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AlunoId,LivroId,DataEmprestimo,DataPrevistaDevolucao")] Emprestimo emprestimo)
        {
            // Validação de aluno ativo
            var alunoValido = await _context.Alunos.AnyAsync(a => a.Id == emprestimo.AlunoId && a.Ativo);
            if (!alunoValido)
                ModelState.AddModelError("AlunoId", "Aluno inválido ou inativo.");

            // Validação de livro ativo
            var livroValido = await _context.Livros.AnyAsync(l => l.Id == emprestimo.LivroId && l.Ativo);
            if (!livroValido)
                ModelState.AddModelError("LivroId", "Livro inválido ou inativo.");

            if (ModelState.IsValid)
            {
                _context.Add(emprestimo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Recarregar dropdowns se houver erro
            ViewData["AlunoId"] = new SelectList(_context.Alunos.Where(a => a.Ativo), "Id", "Nome", emprestimo.AlunoId);
            ViewData["LivroId"] = new SelectList(_context.Livros.Where(l => l.Ativo), "Id", "Titulo", emprestimo.LivroId);
            return View(emprestimo);
        }

        // GET: Emprestimos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emprestimo = await _context.Emprestimos.FindAsync(id);
            if (emprestimo == null)
            {
                return NotFound();
            }
            ViewData["AlunoId"] = new SelectList(_context.Alunos.Where(a => a.Ativo), "Id", "Nome", emprestimo.AlunoId);
            ViewData["LivroId"] = new SelectList(_context.Livros.Where(l => l.Ativo), "Id", "Titulo", emprestimo.LivroId);
            return View(emprestimo);
        }

        // POST: Emprestimos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AlunoId,LivroId,DataEmprestimo,DataPrevistaDevolucao,DataDevolucao")] Emprestimo emprestimo)
        {
            if (id != emprestimo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(emprestimo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmprestimoExists(emprestimo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AlunoId"] = new SelectList(_context.Alunos.Where(a => a.Ativo), "Id", "Nome", emprestimo.AlunoId);
            ViewData["LivroId"] = new SelectList(_context.Livros.Where(l => l.Ativo), "Id", "Autor", emprestimo.LivroId);
            return View(emprestimo);
        }

        // GET: Emprestimos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emprestimo = await _context.Emprestimos
                .Include(e => e.Aluno)
                .Include(e => e.Livro)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (emprestimo == null)
            {
                return NotFound();
            }

            return View(emprestimo);
        }

        // POST: Emprestimos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var emprestimo = await _context.Emprestimos.FindAsync(id);
            if (emprestimo != null)
            {
                _context.Emprestimos.Remove(emprestimo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmprestimoExists(int id)
        {
            return _context.Emprestimos.Any(e => e.Id == id);
        }

        public async Task<IActionResult> DarBaixa(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Aluno)
                .Include(e => e.Livro)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null)
                return NotFound();

            return View(emprestimo);
        }

        [HttpPost]
        [ActionName("DarBaixa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DarBaixaConfirmado(int id, DateTime dataDevolucao)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Aluno)
                .Include(e => e.Livro)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprestimo == null)
                return NotFound();

            // 🔒 Já finalizado
            if (emprestimo.DataDevolucao != null)
            {
                ModelState.AddModelError("", "Este empréstimo já foi finalizado.");
                return View(emprestimo);
            }

            // 📅 Data inválida
            if (dataDevolucao < emprestimo.DataEmprestimo)
            {
                ModelState.AddModelError("", "A data de devolução não pode ser anterior à data do empréstimo.");
                return View(emprestimo);
            }

            // ✅ Tudo OK → dar baixa
            emprestimo.DataDevolucao = dataDevolucao;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
