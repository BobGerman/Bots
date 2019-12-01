using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingBot.Cards
{
    public static class AddToProjectCard
    {
        public static AdaptiveCard GetCard(ITurnContext turnContext, RequestDetails requestDetails)
        {
            var templateJson = @"
            {
                ""type"": ""AdaptiveCard"",
                ""version"": ""1.0"",
                ""body"": [
                    {
                        ""type"": ""TextBlock"",
                        ""text"": ""Hello {name}""
                    }
                ]
            }";

            var dataJson = @"
            {
                ""name"": ""Mickey Mouse""
            }";

            var transformer = new AdaptiveTransformer();
            var cardJson = transformer.Transform(templateJson, dataJson);

            var result = AdaptiveCard.FromJson(cardJson).Card;
            return result;
        }

    }
}
