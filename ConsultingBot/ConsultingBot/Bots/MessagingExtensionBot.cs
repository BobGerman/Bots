using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder.Abstractions;
using Microsoft.Bot.Builder;

namespace ConsultingBot.Bots
{
        public class MessagingExtensionBot : IBot
        {
            private readonly IActivityProcessor activityProcessor;

            public MessagingExtensionBot(IActivityProcessor activityProcessor)
            {
                this.activityProcessor = activityProcessor;
            }
            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                await this.activityProcessor.ProcessIncomingActivityAsync(turnContext).ConfigureAwait(false);
            }
        }
    
}
