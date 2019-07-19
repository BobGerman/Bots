namespace Microsoft.Bot.Builder.Teams.MessagingExtensionBot.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConsultingBot.Cards;
    using ConsultingBot.InvokeActivityHandlers;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Newtonsoft.Json.Linq;

    public class InvokeActivityHandler
    {
        // Send invoke activity to the right place
        public async Task<InvokeResponse> ProcessTeamsInvokeActivityAsync(ITurnContext turnContext)
        {
            ITeamsContext teamsContext = turnContext.TurnState.Get<ITeamsContext>();

            if (teamsContext.IsRequestMessagingExtensionQuery() ||
                teamsContext.IsRequestMessagingExtensionFetchTask() ||
                teamsContext.IsRequestMessagingExtensionSubmitAction())
            {
                var projectMessagingExtension = new TestMessagingExtension();
                return await projectMessagingExtension.ProcessInvokeActivityAsync (turnContext).ConfigureAwait(false);
            }

            if (teamsContext.IsRequestAppBasedLinkQuery())
            {
                var projectLinkQuery = new TestLinkQuery();
                return await projectLinkQuery.HandleAppBasedLinkQueryAsync(turnContext, teamsContext.GetAppBasedLinkQueryData()).ConfigureAwait(false);
            }

            if (teamsContext.IsRequestTaskModuleFetch() ||
                teamsContext.IsRequestTaskModuleSubmit())
            {
                var taskModule = new TestTaskModule();
                return await taskModule.HandleInvokeAsync(turnContext);
            }

            return await Task.FromResult<InvokeResponse>(null);
        }
    }
}
