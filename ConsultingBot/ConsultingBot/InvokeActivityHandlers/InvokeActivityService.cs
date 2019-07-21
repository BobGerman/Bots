using System.Threading.Tasks;
using ConsultingBot.Cards;
using ConsultingBot.InvokeActivityHandlers;
using ConsultingBot.TeamsManifest;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;

namespace ConsultingBot.InvokeActivityHandlers
{
    public class InvokeActivityService : IInvokeActivityService
    {
        private ProjectMessagingExtension projectMessagingExtension;
        public InvokeActivityService(ProjectMessagingExtension projectMessagingExtension)
        {
            this.projectMessagingExtension = projectMessagingExtension;
        }
        public async Task<InvokeResponse> HandleInvokeActivityAsync(ITurnContext turnContext)
        {
            ITeamsContext teamsContext = turnContext.TurnState.Get<ITeamsContext>();

            // Messaging extensions
            if (teamsContext.IsRequestMessagingExtensionQuery())
            {
                IInvokeActivityService messagingExtension;
                if (teamsContext.GetMessagingExtensionQueryData()?.CommandId == ManifestConstants.ComposeExtensions.ProjectQuery.Id)
                {
                    messagingExtension = this.projectMessagingExtension;
                }
                else
                {
                    messagingExtension = new TestMessagingExtension();
                }
                return await messagingExtension.HandleInvokeActivityAsync(turnContext).ConfigureAwait(false);
            }
            
            if (teamsContext.IsRequestMessagingExtensionFetchTask() ||
                teamsContext.IsRequestMessagingExtensionSubmitAction())
            {
                IInvokeActivityService messagingExtension;
                if (teamsContext.GetMessagingExtensionActionData()?.CommandId == ManifestConstants.ComposeExtensions.SampleCard.Id)
                {
                    messagingExtension = this.projectMessagingExtension;
                }
                else
                {
                    messagingExtension = new TestMessagingExtension();
                }
                return await messagingExtension.HandleInvokeActivityAsync (turnContext).ConfigureAwait(false);
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
