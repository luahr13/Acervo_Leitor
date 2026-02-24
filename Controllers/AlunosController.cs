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
    public class AlunosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlunosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Alunos
        public async Task<IActionResult> Index(
            string busca,
            bool? ativo,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Alunos
                .Include(a => a.Turma)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busca))
                query = query.Where(a => a.Nome.Contains(busca));

            if (ativo.HasValue)
                query = query.Where(a => a.Ativo == ativo);

            var totalItems = await query.CountAsync();

            var alunos = await query
                .OrderBy(a => a.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Busca = busca;
            ViewBag.Ativo = ativo;

            return View(alunos);
        }

        // GET: Alunos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aluno = await _context.Alunos
                .Include(a => a.Turma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aluno == null)
            {
                return NotFound();
            }

            return View(aluno);
        }

        // GET: Alunos/Create
        public IActionResult Create()
        {
            ViewData["TurmaId"] = new SelectList(
                _context.Turmas
                .Where(t => t.Ativa)
                .OrderBy(t => t.AnoLetivo)
                .ThenBy(t => t.Nome),
                "Id",
                "Nome"
            );

            return View();
        }

        // POST: Alunos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,TurmaId,Telefone,Ativo")] Aluno aluno)
        {
            var turmaValida = aluno.TurmaId.HasValue &&
                await _context.Turmas.AnyAsync(t => t.Id == aluno.TurmaId && t.Ativa);

            if (!turmaValida)
            {
                ModelState.AddModelError("TurmaId", "Turma inválida ou inativa.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(aluno);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["TurmaId"] = new SelectList(
                _context.Turmas.Where(t => t.Ativa),
                "Id",
                "Nome",
                aluno.TurmaId
            );

            return View(aluno);
        }

        // GET: Alunos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aluno = await _context.Alunos.FindAsync(id);
            if (aluno == null)
            {
                return NotFound();
            }
            ViewData["TurmaId"] = new SelectList(_context.Turmas.Where(t => t.Ativa), "Id", "Nome", aluno.TurmaId);
            return View(aluno);
        }

        // POST: Alunos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,TurmaId,Telefone,Ativo")] Aluno aluno)
        {
            if (id != aluno.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aluno);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlunoExists(aluno.Id))
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
            ViewData["TurmaId"] = new SelectList(_context.Turmas.Where(t => t.Ativa), "Id", "Nome", aluno.TurmaId);
            return View(aluno);
        }

        // GET: Alunos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aluno = await _context.Alunos
                .Include(a => a.Turma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aluno == null)
            {
                return NotFound();
            }

            return View(aluno);
        }

        // POST: Alunos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aluno = await _context.Alunos.FindAsync(id);
            if (aluno != null)
            {
                _context.Alunos.Remove(aluno);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlunoExists(int id)
        {
            return _context.Alunos.Any(e => e.Id == id);
        }

        //GET: Aluno/Nome
        [HttpGet]
        public async Task<IActionResult> BuscarAlunos(string termo)
        {
            var alunos = await _context.Alunos
                .Where(a => a.Nome.Contains(termo))
                .Select(a => new { a.Id, a.Nome })
                .ToListAsync();

            return Json(alunos);
        }
    }
}
