using GestorFinanceiro.Data.Context;
using GestorFinanceiro.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace GestorFinanceiro.Web.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public string Username { get; set; } = string.Empty;
        public int TotalSessoes { get; set; }
        public decimal TotalReceitas { get; set; }
        public decimal TotalDespesas { get; set; }
        public decimal Saldo => TotalReceitas - TotalDespesas;
        public IList<SessaoResumoViewModel> ResumoPorSessao { get; set; } = [];
        public IList<MetaViewModel> Metas { get; set; } = [];
        public IList<HistoricoItemViewModel> Historico { get; set; } = [];
        public BolsaViewModel Bolsa { get; set; } = new();
        public string? MensagemErro { get; set; }
        public string ChartDataJson { get; set; } = "{}";
        public bool TemDadosGraficos { get; set; }

        public SessaoResumoViewModel? SessaoComMaiorSaldo =>
            ResumoPorSessao.Where(r => r.Saldo > 0).OrderByDescending(r => r.Saldo).FirstOrDefault();

        public int? EncontrarSessaoId(string termo) =>
            ResumoPorSessao.FirstOrDefault(r =>
                r.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))?.SessaoId;

        public async Task OnGetAsync() => await CarregarDadosAsync();

        public async Task<IActionResult> OnPostDepositarAsync(int metaId, int sessaoOrigemId, decimal valor)
        {
            try
            {
                var meta = await _context.Metas.FindAsync(metaId);
                if (meta == null)
                {
                    TempData["Erro"] = "Poupança não encontrada.";
                    return RedirectToPage();
                }

                var sessao = await _context.SessoesFinanceiras.FindAsync(sessaoOrigemId);
                if (sessao == null)
                {
                    TempData["Erro"] = "Sessão não encontrada.";
                    return RedirectToPage();
                }

                var totalReceitas = await _context.Receitas
                    .Where(r => r.SessaoFinanceiraId == sessaoOrigemId)
                    .SumAsync(r => r.Valor);

                var totalDespesas = await _context.Despesas
                    .Where(d => d.SessaoFinanceiraId == sessaoOrigemId)
                    .SumAsync(d => d.Valor);

                var saldoSessao = totalReceitas - totalDespesas;

                if (valor > saldoSessao)
                {
                    TempData["Erro"] = $"Saldo insuficiente na sessão. Disponível: {saldoSessao:N2} €.";
                    return RedirectToPage();
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Despesas.Add(new Data.Models.Despesa
                {
                    Descricao = $"Depósito para poupança: {meta.Nome}",
                    Valor = valor,
                    Data = DateTime.Now,
                    SessaoFinanceiraId = sessaoOrigemId
                });

                meta.ValorAtual += valor;

                _context.MetaMovimentos.Add(new Data.Models.MetaMovimento
                {
                    MetaId = metaId,
                    Tipo = "Deposito",
                    Valor = valor,
                    Data = DateTime.Now
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Sucesso"] = "Depósito realizado com sucesso.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLevantarAsync(int metaId, int sessaoDestinoId, decimal valor)
        {
            try
            {
                var meta = await _context.Metas.FindAsync(metaId);
                if (meta == null)
                {
                    TempData["Erro"] = "Poupança não encontrada.";
                    return RedirectToPage();
                }

                if (valor > meta.ValorAtual)
                {
                    TempData["Erro"] = $"Valor superior ao disponível na poupança ({meta.ValorAtual:N2} €).";
                    return RedirectToPage();
                }

                var sessao = await _context.SessoesFinanceiras.FindAsync(sessaoDestinoId);
                if (sessao == null)
                {
                    TempData["Erro"] = "Sessão não encontrada.";
                    return RedirectToPage();
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                _context.Receitas.Add(new Data.Models.Receita
                {
                    Descricao = $"Levantamento de poupança: {meta.Nome}",
                    Valor = valor,
                    Data = DateTime.Now,
                    SessaoFinanceiraId = sessaoDestinoId
                });

                meta.ValorAtual -= valor;

                _context.MetaMovimentos.Add(new Data.Models.MetaMovimento
                {
                    MetaId = metaId,
                    Tipo = "Levantamento",
                    Valor = valor,
                    Data = DateTime.Now
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Sucesso"] = "Levantamento realizado com sucesso.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTransferirBolsaSessaoAsync(int sessaoDestinoId, decimal valor)
        {
            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var bolsa = await _context.Bolsas
                    .FirstOrDefaultAsync(b => b.UtilizadorId == utilizadorId);

                if (bolsa == null || bolsa.Saldo < valor)
                {
                    TempData["Erro"] = $"Saldo insuficiente na bolsa. Disponível: {bolsa?.Saldo ?? 0:N2} €.";
                    return RedirectToPage();
                }

                var sessao = await _context.SessoesFinanceiras.FindAsync(sessaoDestinoId);
                if (sessao == null)
                {
                    TempData["Erro"] = "Sessão não encontrada.";
                    return RedirectToPage();
                }

                bolsa.Saldo -= valor;
                bolsa.DataAtualizacao = DateTime.Now;

                _context.Receitas.Add(new Data.Models.Receita
                {
                    Descricao = "Transferência da bolsa",
                    Valor = valor,
                    Data = DateTime.Now,
                    SessaoFinanceiraId = sessaoDestinoId
                });

                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Transferência da bolsa realizada com sucesso.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostTransferirBolsaMetaAsync(int metaDestinoId, decimal valor)
        {
            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var bolsa = await _context.Bolsas
                    .FirstOrDefaultAsync(b => b.UtilizadorId == utilizadorId);

                if (bolsa == null || bolsa.Saldo < valor)
                {
                    TempData["Erro"] = $"Saldo insuficiente na bolsa. Disponível: {bolsa?.Saldo ?? 0:N2} €.";
                    return RedirectToPage();
                }

                var meta = await _context.Metas.FindAsync(metaDestinoId);
                if (meta == null)
                {
                    TempData["Erro"] = "Meta não encontrada.";
                    return RedirectToPage();
                }

                bolsa.Saldo -= valor;
                bolsa.DataAtualizacao = DateTime.Now;
                meta.ValorAtual += valor;

                _context.MetaMovimentos.Add(new Data.Models.MetaMovimento
                {
                    MetaId = metaDestinoId,
                    Tipo = "Deposito",
                    Valor = valor,
                    Data = DateTime.Now
                });

                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Transferência para poupança realizada com sucesso.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApagarHistoricoAsync(int registoId, string tipoRegisto)
        {
            try
            {
                switch (tipoRegisto)
                {
                    case "receita":
                        var receita = await _context.Receitas.FindAsync(registoId);
                        if (receita == null)
                        {
                            TempData["Erro"] = "Registo não encontrado.";
                            return RedirectToPage();
                        }
                        _context.Receitas.Remove(receita);
                        break;

                    case "despesa":
                        var despesa = await _context.Despesas.FindAsync(registoId);
                        if (despesa == null)
                        {
                            TempData["Erro"] = "Registo não encontrado.";
                            return RedirectToPage();
                        }
                        _context.Despesas.Remove(despesa);
                        break;

                    case "metamovimento":
                        var movimento = await _context.MetaMovimentos
                            .Include(m => m.Meta)
                            .FirstOrDefaultAsync(m => m.Id == registoId);

                        if (movimento == null)
                        {
                            TempData["Erro"] = "Registo não encontrado.";
                            return RedirectToPage();
                        }

                        if (movimento.Tipo == "Deposito")
                            movimento.Meta.ValorAtual -= movimento.Valor;
                        else if (movimento.Tipo == "Levantamento")
                            movimento.Meta.ValorAtual += movimento.Valor;

                        if (movimento.Meta.ValorAtual < 0)
                            movimento.Meta.ValorAtual = 0;

                        _context.MetaMovimentos.Remove(movimento);
                        break;

                    default:
                        TempData["Erro"] = "Tipo de registo desconhecido.";
                        return RedirectToPage();
                }

                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Registo apagado com sucesso.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }
            return RedirectToPage();
        }

        private async Task CarregarDadosAsync()
        {
            Username = User.Identity?.Name ?? "Utilizador";
            var utilizadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var sessoes = await _context.SessoesFinanceiras
                    .Where(s => s.UtilizadorId == utilizadorId)
                    .OrderByDescending(s => s.DataCriacao)
                    .ToListAsync();

                TotalSessoes = sessoes.Count;

                List<Data.Models.Receita> receitas = [];
                List<Data.Models.Despesa> despesas = [];

                if (sessoes.Count > 0)
                {
                    var ids = sessoes.Select(s => s.Id).ToList();

                    receitas = await _context.Receitas
                        .Where(r => ids.Contains(r.SessaoFinanceiraId))
                        .ToListAsync();

                    despesas = await _context.Despesas
                        .Where(d => ids.Contains(d.SessaoFinanceiraId))
                        .ToListAsync();

                    ResumoPorSessao = sessoes.Select(s => new SessaoResumoViewModel
                    {
                        SessaoId = s.Id,
                        Nome = s.Nome,
                        Descricao = s.Descricao,
                        DataCriacao = s.DataCriacao,
                        TotalReceitas = receitas.Where(r => r.SessaoFinanceiraId == s.Id).Sum(r => r.Valor),
                        TotalDespesas = despesas.Where(d => d.SessaoFinanceiraId == s.Id).Sum(d => d.Valor)
                    })
                    .OrderByDescending(x => x.DataCriacao)
                    .ToList();

                    // Excluir transferências e movimentos de poupança dos totais globais
                    var receitasReais = receitas.Where(r =>
                        r.Descricao == null ||
                        (!r.Descricao.Contains("←") && !r.Descricao.Contains("Levantamento de poupança"))).ToList();

                    var despesasReais = despesas.Where(d =>
                        d.Descricao == null ||
                        (!d.Descricao.Contains("→") && !d.Descricao.Contains("Depósito para poupança"))).ToList();

                    var resumosReais = sessoes.Select(s => new SessaoResumoViewModel
                    {
                        SessaoId = s.Id,
                        Nome = s.Nome,
                        Descricao = s.Descricao,
                        DataCriacao = s.DataCriacao,
                        TotalReceitas = receitasReais.Where(r => r.SessaoFinanceiraId == s.Id).Sum(r => r.Valor),
                        TotalDespesas = despesasReais.Where(d => d.SessaoFinanceiraId == s.Id).Sum(d => d.Valor)
                    }).ToList();

                    TotalReceitas = resumosReais.Sum(r => r.TotalReceitas);
                    TotalDespesas = resumosReais.Sum(r => r.TotalDespesas);

                    ChartDataJson = JsonSerializer.Serialize(new
                    {
                        sessoes = ResumoPorSessao.Select(x => new { id = x.SessaoId, nome = x.Nome, receitas = x.TotalReceitas, despesas = x.TotalDespesas }).ToList(),
                        totalReceitas = TotalReceitas,
                        totalDespesas = TotalDespesas
                    });

                    TemDadosGraficos = TotalReceitas > 0 || TotalDespesas > 0;
                }

                // Metas
                var todasMetas = await _context.Metas
                    .Where(m => m.UtilizadorId == utilizadorId)
                    .OrderBy(m => m.DataCriacao)
                    .ToListAsync();

                Metas = todasMetas
                    .Select(m => new MetaViewModel { Id = m.Id, Nome = m.Nome, Descricao = m.Descricao, ValorAlvo = m.ValorAlvo, ValorAtual = m.ValorAtual, UtilizadorId = m.UtilizadorId, DataCriacao = m.DataCriacao })
                    .ToList();

                // Bolsa
                var bolsaData = await _context.Bolsas
                    .FirstOrDefaultAsync(b => b.UtilizadorId == utilizadorId);

                Bolsa = bolsaData != null
                    ? new BolsaViewModel { UtilizadorId = utilizadorId, Saldo = bolsaData.Saldo, DataAtualizacao = bolsaData.DataAtualizacao }
                    : new BolsaViewModel { UtilizadorId = utilizadorId, Saldo = 0m, DataAtualizacao = DateTime.Now };

                // Historico
                var metaIds = Metas.Select(m => m.Id).ToHashSet();
                var sessaoNomes = sessoes.ToDictionary(s => s.Id, s => s.Nome);

                var itensReceitas = receitas
                    .Select(r => new HistoricoItemViewModel
                    {
                        RegistoId = r.Id,
                        TipoRegisto = "receita",
                        Data = r.Data,
                        Tipo = "Receita",
                        Icone = "↑",
                        Valor = r.Valor,
                        Descricao = $"{r.Descricao} · {(sessaoNomes.TryGetValue(r.SessaoFinanceiraId, out var nomeR) ? nomeR : "")}"
                    });

                var itensDespesas = despesas
                    .Where(d => d.Descricao == null || !d.Descricao.Contains("→") && !d.Descricao.Contains("poupança"))
                    .Select(d => new HistoricoItemViewModel
                    {
                        RegistoId = d.Id,
                        TipoRegisto = "despesa",
                        Data = d.Data,
                        Tipo = "Despesa",
                        Icone = "↓",
                        Valor = d.Valor,
                        Descricao = $"{d.Descricao} · {(sessaoNomes.TryGetValue(d.SessaoFinanceiraId, out var nomeD) ? nomeD : "")}"
                    });

                var transferencias = despesas
                    .Where(d => d.Descricao != null && d.Descricao.Contains("→"))
                    .Select(d => new HistoricoItemViewModel
                    {
                        RegistoId = d.Id,
                        TipoRegisto = "despesa",
                        Data = d.Data,
                        Tipo = "Transferencia",
                        Icone = "⇄",
                        Valor = d.Valor,
                        Descricao = d.Descricao ?? ""
                    });

                var movimentos = await _context.MetaMovimentos
                    .Include(m => m.Meta)
                    .OrderByDescending(m => m.Data)
                    .ToListAsync();

                var metaMovimentos = movimentos
                    .Where(m => metaIds.Contains(m.MetaId))
                    .Select(m => new HistoricoItemViewModel
                    {
                        RegistoId = m.Id,
                        TipoRegisto = "metamovimento",
                        Data = m.Data,
                        Tipo = m.Tipo == "Deposito" ? "Deposito na Conta Poupança" : "Levantamento da Conta Poupança",
                        Icone = m.Tipo == "Deposito" ? "↑" : "↓",
                        Valor = m.Valor,
                        Descricao = $"Conta Poupança: {Metas.FirstOrDefault(meta => meta.Id == m.MetaId)?.Nome ?? ""}"
                    });

                Historico = itensReceitas
                    .Concat(itensDespesas)
                    .Concat(transferencias)
                    .Concat(metaMovimentos)
                    .OrderByDescending(h => h.Data)
                    .ThenByDescending(h => h.RegistoId)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                MensagemErro = $"Não foi possível carregar o dashboard: {ex.Message}";
            }
        }
    }
}