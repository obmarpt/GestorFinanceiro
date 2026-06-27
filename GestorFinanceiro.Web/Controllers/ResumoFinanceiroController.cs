using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/sessoesfinanceiras/{sessaoId}/[controller]")]
    public class ResumoFinanceiroController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResumoFinanceiroController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetResumoFinanceiro(int sessaoId)
        {
            var sessao = _context.SessoesFinanceiras
                .Include(s => s.ReceitasRecorrentes)
                .FirstOrDefault(s => s.Id == sessaoId);

            if (sessao == null)
                return NotFound("Sessão financeira não encontrada.");

            var hoje = DateTime.Today;
            var mesAtual = hoje.Month;
            var anoAtual = hoje.Year;

            // ?? Gerar receitas recorrentes automaticamente
            var receitasParaCriar = new List<Receita>();

            foreach (var regra in sessao.ReceitasRecorrentes.Where(r => r.Ativa))
            {
                var jaExiste = _context.Receitas.Any(r =>
                    r.SessaoFinanceiraId == sessaoId &&
                    r.Descricao == regra.Descricao &&
                    r.Data.Month == mesAtual &&
                    r.Data.Year == anoAtual
                );

                if (!jaExiste && hoje.Day >= regra.DiaDoMes)
                {
                    receitasParaCriar.Add(new Receita
                    {
                        Descricao = regra.Descricao,
                        Valor = regra.Valor,
                        Data = new DateTime(anoAtual, mesAtual, regra.DiaDoMes),
                        SessaoFinanceiraId = sessaoId
                    });
                }
            }

            if (receitasParaCriar.Any())
            {
                _context.Receitas.AddRange(receitasParaCriar);
                _context.SaveChanges();
            }

            // ?? Cálculos financeiros
            var totalReceitas = _context.Receitas
                .Where(r => r.SessaoFinanceiraId == sessaoId)
                .Sum(r => r.Valor);

            var totalDespesas = _context.Despesas
                .Where(d => d.SessaoFinanceiraId == sessaoId)
                .Sum(d => d.Valor);

            var saldo = totalReceitas - totalDespesas;

            var despesasPorCategoria = _context.Despesas
                .Where(d => d.SessaoFinanceiraId == sessaoId && d.Categoria != null)
                .Include(d => d.Categoria)
                .GroupBy(d => d.Categoria!.Nome)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Sum(d => d.Valor)
                })
                .ToList();

            return Ok(new
            {
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                Saldo = saldo,
                DespesasPorCategoria = despesasPorCategoria
            });
        }
    }
}
