﻿// <copyright file="MessageActivityHandler.cs" company="Microsoft">
// Licensed under the MIT License.
// </copyright>

namespace Microsoft.Bot.Builder.Teams.MessagingExtensionBot.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Abstractions;
    using Microsoft.Bot.Builder.Abstractions.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handles Teams invoke activity.
    /// </summary>
    /// <seealso cref="ITeamsInvokeActivityHandler" />
    public class MessageActivityHandler : IMessageActivityHandler
    {
        /// <summary>
        /// Handles the message activity asynchronously.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <returns>Task tracking operation.</returns>
        public async Task HandleMessageAsync(ITurnContext turnContext)
        {
            ITeamsContext teamsContext = turnContext.TurnState.Get<ITeamsContext>();

            string actualText = teamsContext.GetActivityTextWithoutMentions();
            if (actualText.Equals("Cards", StringComparison.OrdinalIgnoreCase))
            {
                // Demo card 1 - adaptive card with bot bulder actions
                AdaptiveCards.AdaptiveCard adaptiveCard = new AdaptiveCards.AdaptiveCard();
                adaptiveCard.Body.Add(new AdaptiveCards.AdaptiveTextBlock("Bot Builder actions"));

                var action1 = new CardAction("imback", "imBack", null, null, null, "text");
                var action2 = new CardAction("messageBack", "message back", null, "text received by bots", "text display to users", JObject.Parse(@"{ ""key"" : ""value"" }"));
                var action3 = new CardAction("invoke", "invoke", null, null, null, JObject.Parse(@"{ ""key"" : ""value"" }"));
                adaptiveCard.Actions.Add(action1.ToAdaptiveCardAction());
                adaptiveCard.Actions.Add(action2.ToAdaptiveCardAction());
                adaptiveCard.Actions.Add(action3.ToAdaptiveCardAction());

                // Task module action
                var taskModuleAction = new TaskModuleAction("Launch Task Module", @"{ ""hiddenKey"": ""hidden value from task module launcher"" }");

                // Demo card 2 - launch task module from adaptive card
                AdaptiveCards.AdaptiveCard taskModuleCard1 = new AdaptiveCards.AdaptiveCard();
                taskModuleCard1.Body.Add(new AdaptiveCards.AdaptiveTextBlock("Task Module Adaptive Card"));
                taskModuleCard1.Actions.Add(taskModuleAction.ToAdaptiveCardAction());

                // Demo card 3 - launch task module from hero card (or any bot-builder framework card)
                HeroCard taskModuleCard2 = new HeroCard("Launch Task Module", null, null, null, new List<CardAction> { taskModuleAction });

                Activity replyActivity = turnContext.Activity.CreateReply();
                replyActivity.Attachments = new List<Attachment>()
                {
                    adaptiveCard.ToAttachment(),
                    taskModuleCard1.ToAttachment(),
                    taskModuleCard2.ToAttachment(),
                };
                await turnContext.SendActivityAsync(replyActivity).ConfigureAwait(false);
            }
            else
            {
                Activity replyActivity = turnContext.Activity.CreateReply();
                replyActivity.TextFormat = "xml";
                replyActivity.Text = $"You said: {turnContext.Activity.Text}";
                await turnContext.SendActivityAsync(replyActivity).ConfigureAwait(false);
            }
        }
    }
}
