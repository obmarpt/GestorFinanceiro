using GestorFinanceiro.Web.Models;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace GestorFinanceiro.Web.Helpers
{
    public static class FinanceApiHelper
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<(List<Data.Models.Receita> Receitas, List<Data.Models.Despesa> Despesas, string? Erro)>
            ObterReceitasEDespesasAsync(HttpClient client)
        {
            var receitasResponse = await client.GetAsync("api/Receita");
            var despesasResponse = await client.GetAsync("api/Despesa");

            if (!receitasResponse.IsSuccessStatusCode || !despesasResponse.IsSuccessStatusCode)
                return ([], [], "Não foi possível carregar receitas e despesas.");

            var receitas = await receitasResponse.Content.ReadFromJsonAsync<List<Data.Models.Receita>>(JsonOptions) ?? [];
            var despesas = await despesasResponse.Content.ReadFromJsonAsync<List<Data.Models.Despesa>>(JsonOptions) ?? [];

            return (receitas, despesas, null);
        }

        public static List<SessaoResumoViewModel> ConstruirResumosPorSessao(
            IEnumerable<Data.Models.SessaoFinanceira> sessoes,
            IEnumerable<Data.Models.Receita> receitas,
            IEnumerable<Data.Models.Despesa> despesas)
        {
            return sessoes.Select(s => new SessaoResumoViewModel
            {
                SessaoId = s.Id,
                Nome = s.Nome,
                Descricao = s.Descricao,
                DataCriacao = s.DataCriacao,
                TotalReceitas = receitas.Where(r => r.SessaoFinanceiraId == s.Id).Sum(r => r.Valor),
                TotalDespesas = despesas.Where(d => d.SessaoFinanceiraId == s.Id).Sum(d => d.Valor)
            }).ToList();
        }

        public static (decimal TotalReceitas, decimal TotalDespesas) CalcularTotaisAgregados(
            IEnumerable<SessaoResumoViewModel> resumos)
        {
            return (resumos.Sum(r => r.TotalReceitas), resumos.Sum(r => r.TotalDespesas));
        }

        public static List<Data.Models.SessaoFinanceira> FiltrarSessoesDoUtilizador(
            IEnumerable<Data.Models.SessaoFinanceira> sessoes,
            ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            return sessoes.Where(s =>
            {
                if (!string.IsNullOrEmpty(userId) && s.UtilizadorId.ToString() == userId)
                    return true;

                if (s.Utilizador == null)
                    return false;

                return (!string.IsNullOrEmpty(username) && s.Utilizador.Username == username)
                    || (!string.IsNullOrEmpty(email) && s.Utilizador.Email == email);
            }).ToList();
        }
    }
}
