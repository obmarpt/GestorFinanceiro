using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanceiro.Web.Helpers
{
    /// <summary>
    /// Gera receitas mensais a partir de regras ativas (lógica migrada da API).
    /// </summary>
    public static class ReceitaRecorrenteHelper
    {
        public static async Task<int> GerarReceitasPendentesAsync(
            ApplicationDbContext context,
            int? sessaoId = null,
            int? utilizadorId = null,
            CancellationToken cancellationToken = default)
        {
            var hoje = DateTime.Today;
            var mesAtual = hoje.Month;
            var anoAtual = hoje.Year;

            var query = context.ReceitasRecorrentes
                .AsQueryable()
                .Where(r => r.Ativa);

            if (sessaoId.HasValue)
                query = query.Where(r => r.SessaoFinanceiraId == sessaoId.Value);

            if (utilizadorId.HasValue)
            {
                query = query.Where(r =>
                    context.SessoesFinanceiras.Any(s =>
                        s.Id == r.SessaoFinanceiraId &&
                        s.UtilizadorId == utilizadorId.Value));
            }

            var regras = await query.ToListAsync(cancellationToken);
            if (regras.Count == 0)
                return 0;

            var receitasParaCriar = new List<Receita>();

            foreach (var regra in regras)
            {
                if (hoje.Day < regra.DiaDoMes)
                    continue;

                if (regra.DataInicio.Date > hoje)
                    continue;

                if (regra.DataFim.HasValue && regra.DataFim.Value.Date < hoje)
                    continue;

                var dia = Math.Min(regra.DiaDoMes, DateTime.DaysInMonth(anoAtual, mesAtual));

                var jaExiste = await context.Receitas.AnyAsync(r =>
                    r.SessaoFinanceiraId == regra.SessaoFinanceiraId &&
                    r.Descricao == regra.Descricao &&
                    r.Data.Month == mesAtual &&
                    r.Data.Year == anoAtual,
                    cancellationToken);

                if (jaExiste)
                    continue;

                receitasParaCriar.Add(new Receita
                {
                    Descricao = regra.Descricao,
                    Valor = regra.Valor,
                    Data = new DateTime(anoAtual, mesAtual, dia),
                    SessaoFinanceiraId = regra.SessaoFinanceiraId
                });
            }

            if (receitasParaCriar.Count == 0)
                return 0;

            context.Receitas.AddRange(receitasParaCriar);
            await context.SaveChangesAsync(cancellationToken);
            return receitasParaCriar.Count;
        }
    }
}
