using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace ConsultingBot.Cards
{
    interface IMyCard
    {
        Attachment GetCardAttachment(ITurnContext turnContext);

        Task<InvokeResponse> HandleInvokeAsync(ITurnContext turnContext);
    }
}
