using System.Threading.Tasks;
using ConsultingBot.Cards;
using ConsultingBot.InvokeActivityHandlers;

namespace Microsoft.Bot.Builder.Teams.MessagingExtensionBot.Engine
{
    public class InvokeActivityHandler : IInvokeActivityHandler
    {
        // Dispatcher for all Bot Invoke activities
        public async Task<InvokeResponse> HandleInvokeActivityAsync(ITurnContext turnContext)
        {
            ITeamsContext teamsContext = turnContext.TurnState.Get<ITeamsContext>();

            // Messaging extensions
            if (teamsContext.IsRequestMessagingExtensionQuery() ||
                teamsContext.IsRequestMessagingExtensionFetchTask() ||
                teamsContext.IsRequestMessagingExtensionSubmitAction())
            {
                var projectMessagingExtension = new TestMessagingExtension();
                return await projectMessagingExtension.HandleInvokeActivityAsync (turnContext).ConfigureAwait(false);
            }

            // Link previews
            if (teamsContext.IsRequestAppBasedLinkQuery())
            {
                var projectLinkQuery = new TestLinkQuery();
                return await projectLinkQuery.HandleInvokeActivityAsync
                    (turnContext).ConfigureAwait(false);
            }

            // Task modules
            if (teamsContext.IsRequestTaskModuleFetch() ||
                teamsContext.IsRequestTaskModuleSubmit())
            {
                var taskModule = new TestTaskModule();
                return await taskModule.HandleInvokeActivityAsync(turnContext);
            }

            return await Task.FromResult<InvokeResponse>(null);
        }
    }
}
