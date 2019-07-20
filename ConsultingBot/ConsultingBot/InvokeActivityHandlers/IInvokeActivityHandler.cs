using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace ConsultingBot.InvokeActivityHandlers
{
    interface IInvokeActivityHandler
    {
        Task<InvokeResponse> HandleInvokeActivityAsync(ITurnContext turnContext);
    }
}
