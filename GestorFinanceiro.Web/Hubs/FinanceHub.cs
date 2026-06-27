using Microsoft.AspNetCore.SignalR;

namespace GestorFinanceiro.Web.Hubs
{
    public class FinanceHub : Hub
    {
        public async Task NotificarResumoAtualizado(int sessaoFinanceiraId)
        {
            await Clients.All.SendAsync(
                "ResumoAtualizado",
                sessaoFinanceiraId
            );
        }
    }
}
//“O FinanceHub é responsável apenas pela comunicação em tempo real,
//separando a lógica de negócio da notificação aos clientes.”

//[Controller] --? [FinanceHub] --? [Browser]