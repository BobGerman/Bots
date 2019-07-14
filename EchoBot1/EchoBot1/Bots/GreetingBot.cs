using EchoBot1.Models;
using EchoBot1.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot1.Bots
{
    public class GreetingBot: ActivityHandler
    {
        private readonly BotStateService _botStateService;
        public GreetingBot(BotStateService botStateService)
        {
            _botStateService = botStateService;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await GetName(turnContext, cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await GetName(turnContext, cancellationToken);
                }
            }
        }

        private async Task GetName(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // Get the state properties
            UserProfile userProfile = 
                await _botStateService.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
            ConversationData conversationData =
                await _botStateService.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

            if (!string.IsNullOrEmpty(userProfile.Name))
            {
                // We have the name, send a greeting
                await turnContext.SendActivityAsync(MessageFactory.Text($"Hi {userProfile.Name}"), cancellationToken);
            }
            else
            { 
                // We don't have the name; get it
                if (conversationData.PromptedUserForName)
                {
                    userProfile.Name = turnContext.Activity.Text.Trim();
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hi {userProfile.Name}"), cancellationToken);
                    conversationData.PromptedUserForName = false;
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"What is your name?"), cancellationToken);
                    conversationData.PromptedUserForName = true;
                }

                // Set the name and conversation state
                await _botStateService.UserProfileAccessor.SetAsync(turnContext, userProfile);
                await _botStateService.ConversationDataAccessor.SetAsync(turnContext, conversationData);

                // Save the changes
                await _botStateService.UserState.SaveChangesAsync(turnContext);
                await _botStateService.ConversationState.SaveChangesAsync(turnContext);
            }
        }
    }
}
