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

            #region Unused activity types
            //if (teamsContext.IsRequestO365ConnectorCardActionQuery())
            //{
            //    return await this.HandleO365ConnectorCardActionAsync(turnContext, teamsContext.GetO365ConnectorCardActionQueryData()).ConfigureAwait(false);
            //}

            //if (teamsContext.IsRequestSigninStateVerificationQuery())
            //{
            //    return await this.HandleSigninStateVerificationActionAsync(turnContext, teamsContext.GetSigninStateVerificationQueryData()).ConfigureAwait(false);
            //}

            //if (teamsContext.IsRequestFileConsentResponse())
            //{
            //    return await this.HandleFileConsentResponseAsync(turnContext, teamsContext.GetFileConsentQueryData()).ConfigureAwait(false);
            //}
            #endregion

            #region Messagine extensions and link queries
            if (teamsContext.IsRequestMessagingExtensionQuery())
            {
                var projectMessagingExtension = new ProjectsMessagingExtension();
                return await projectMessagingExtension.HandleMessagingExtensionQueryAsync(turnContext, teamsContext.GetMessagingExtensionQueryData()).ConfigureAwait(false);
            }

            if (teamsContext.IsRequestAppBasedLinkQuery())
            {
                var projectLinkQuery = new ProjectLinkQuery();
                return await projectLinkQuery.HandleAppBasedLinkQueryAsync(turnContext, teamsContext.GetAppBasedLinkQueryData()).ConfigureAwait(false);
            }

            if (teamsContext.IsRequestMessagingExtensionFetchTask())
            {
                var projectMessagingExtension = new ProjectsMessagingExtension();
                return await projectMessagingExtension.HandleMessagingExtensionFetchTaskAsync(turnContext, teamsContext.GetMessagingExtensionActionData()).ConfigureAwait(false);
            }
            #endregion

            if (teamsContext.IsRequestMessagingExtensionSubmitAction() ||
                teamsContext.IsRequestTaskModuleFetch() ||
                teamsContext.IsRequestTaskModuleSubmit())
            {
                var card = new TaskModuleResponseCard();
                return await card.HandleInvokeAsync(turnContext);
            }

            return await this.HandleInvokeTaskAsync(turnContext).ConfigureAwait(false);
        }

        // Called when an Invoke button on a card is clicked
        private async Task<InvokeResponse> HandleInvokeTaskAsync(ITurnContext turnContext)
        {
             return await Task.FromResult<InvokeResponse>(null);
        }





    }
}
