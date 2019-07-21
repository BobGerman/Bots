using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace ConsultingBot.InvokeActivityHandlers
{
    public interface IInvokeActivityService
    {
        Task<InvokeResponse> HandleInvokeActivityAsync(ITurnContext turnContext);
    }
}
