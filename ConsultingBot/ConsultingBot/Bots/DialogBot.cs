// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsultingBot.InvokeActivityHandlers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Builder.Teams.MessagingExtensionBot.Engine;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsultingBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly BotState conversationState;
        protected readonly BotState userState;
        protected readonly Dialog dialog;
        protected readonly IInvokeActivityService invokeActivityService;
        protected readonly ILogger logger;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog,
            IInvokeActivityService invokeActivityService, 
            ILogger<DialogBot<T>> logger)
        {
            this.conversationState = conversationState;
            this.userState = userState;
            this.dialog = dialog;
            this.invokeActivityService = invokeActivityService;
            this.logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Invoke)
            {
                InvokeResponse invokeResponse = await invokeActivityService.HandleInvokeActivityAsync(turnContext).ConfigureAwait(false);

                await turnContext.SendActivityAsync(
                    new Activity
                    {
                        Value = invokeResponse,
                        Type = ActivityTypesEx.InvokeResponse,
                    }).ConfigureAwait(false);
            }
            else
            {
                await base.OnTurnAsync(turnContext, cancellationToken);
            }

            // Save any state changes that might have occured during the turn.
            await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await dialog.Run(turnContext, conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
        }
    }
}
