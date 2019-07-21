using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Teams.MessagingExtensionBot.Engine;
using ConsultingBot.InvokeActivityHandlers;

namespace ConsultingBot.Bots
{
        public class MessagingExtensionBot : IBot
        {
            private readonly ProjectMessagingExtension projectMessagingExtension;
            public MessagingExtensionBot(ProjectMessagingExtension projectMessageExtension)
            {
                this.projectMessagingExtension = projectMessageExtension;
            }
            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    {
                        var handler = new MessageActivityHandler();
                        await handler.HandleMessageAsync(turnContext).ConfigureAwait(false);
                        break;
                    }

                case ActivityTypes.Invoke:
                    {
                        var handler = new InvokeActivityHandler(this.projectMessagingExtension);
                        InvokeResponse invokeResponse = await handler.HandleInvokeActivityAsync(turnContext).ConfigureAwait(false);

                        await turnContext.SendActivityAsync(
                            new Activity
                            {
                                Value = invokeResponse,
                                Type = ActivityTypesEx.InvokeResponse,
                            }).ConfigureAwait(false);
                        break;
                    }
            }
        }
    }
    
}
