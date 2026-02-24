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
        public async Task<IActionResult> Index(string status, string busca, int page = 1)
        {
            int pageSize = 10;

            var emprestimos = _context.Emprestimos
                .Include(e => e.Aluno)
                .Include(e => e.Livro)
                .AsQueryable();

            // 📌 Filtrar pelo status
            if (!string.IsNullOrEmpty(status) && status != "Todos")
            {
                emprestimos = status switch
                {
                    "Baixa" => emprestimos.Where(e => e.DataDevolucao != null),

                    "Aberto" => emprestimos.Where(e =>
                        e.DataDevolucao == null &&
                        e.DataPrevistaDevolucao >= DateTime.Now),

                    "Atraso" => emprestimos.Where(e =>
                        e.DataDevolucao == null &&
                        e.DataPrevistaDevolucao < DateTime.Now),

                    _ => emprestimos
                };
            }

            // 🔍 Filtrar por aluno ou livro
            if (!string.IsNullOrEmpty(busca))
            {
                busca = busca.ToLower();

                emprestimos = emprestimos.Where(e =>
                    e.Aluno.Nome.ToLower().Contains(busca) ||
                    e.Livro.Titulo.ToLower().Contains(busca));
            }

            // 📊 Paginação
            int totalItems = await emprestimos.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var resultado = await emprestimos
                .OrderByDescending(e => e.DataEmprestimo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(resultado);
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
            // 🔹 Alunos ativos com Nome + Turma
            var alunos = _context.Alunos
                .Include(a => a.Turma)
                .Where(a => a.Ativo)
                .Select(a => new
                {
                    a.Id,
                    Nome = a.Nome + " - " + a.Turma.Nome
                })
                .OrderBy(a => a.Nome)
                .ToList();

            ViewData["AlunoId"] = new SelectList(alunos, "Id", "Nome");


            // 🔹 Livros DISPONÍVEIS (NUNCA emprestados OU devolvidos)
            var livrosDisponiveis = _context.Livros
                .Where(l => l.Ativo)
                .Where(l => !_context.Emprestimos.Any(e =>
                    e.LivroId == l.Id &&
                    (e.DataDevolucao == null) // Aberto OU Atraso
                ))
                .Select(l => new
                {
                    l.Id,
                    Nome = l.Titulo + " (Cod: " + l.CodigoExemplar + ")"
                })
                .OrderBy(l => l.Nome)
                .ToList();

            ViewData["LivroId"] = new SelectList(livrosDisponiveis, "Id", "Nome");

            return View();
        }

        // Metodo Auxiliar DropDown
        private async Task<IActionResult> RecarregarDropdowns(Emprestimo emprestimo)
        {
            var alunos = await _context.Alunos
                .Include(a => a.Turma)
                .Where(a => a.Ativo)
                .Select(a => new
                {
                    a.Id,
                    Nome = a.Nome + " - " + a.Turma.Nome
                }).ToListAsync();

            ViewData["AlunoId"] = new SelectList(alunos, "Id", "Nome", emprestimo.AlunoId);

            var livros = await _context.Livros
                .Where(l => l.Ativo)
                .Where(l => !_context.Emprestimos.Any(e =>
                    e.LivroId == l.Id &&
                    e.DataDevolucao == null))
                .Select(l => new
                {
                    l.Id,
                    Nome = l.Titulo + " (Cod: " + l.CodigoExemplar + ")"
                }).ToListAsync();

            ViewData["LivroId"] = new SelectList(livros, "Id", "Nome", emprestimo.LivroId);

            return View(emprestimo);
        }

        // POST: Emprestimos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Emprestimo emprestimo)
        {
            // 🔹 Aluno ativo
            if (!await _context.Alunos.AnyAsync(a => a.Id == emprestimo.AlunoId && a.Ativo))
                ModelState.AddModelError("AlunoId", "Aluno inválido ou inativo.");

            // 🔹 Livro ativo
            if (!await _context.Livros.AnyAsync(l => l.Id == emprestimo.LivroId && l.Ativo))
                ModelState.AddModelError("LivroId", "Livro inválido ou inativo.");

            // 🚨 LIVRO NÃO PODE TER EMPRESTIMO ATIVO (Aberto OU Atraso)
            var livroBloqueado = await _context.Emprestimos.AnyAsync(e =>
                e.LivroId == emprestimo.LivroId &&
                e.DataDevolucao == null);

            if (livroBloqueado)
                ModelState.AddModelError("LivroId", "Este livro já está emprestado e não foi devolvido.");

            // Datas
            if (emprestimo.DataPrevistaDevolucao < emprestimo.DataEmprestimo)
                ModelState.AddModelError("DataPrevistaDevolucao", "A data prevista não pode ser menor que a data do empréstimo.");

            if (ModelState.IsValid)
            {
                _context.Add(emprestimo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Recarregar dropdowns se falhar
            return await RecarregarDropdowns(emprestimo);
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
