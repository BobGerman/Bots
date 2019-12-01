using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsultingBot.Cards
{
    public static class AddToProjectCard
    {
        public static async Task<AdaptiveCard> GetCard(ITurnContext turnContext, RequestDetails requestDetails)
        {
            var templateJson = String.Empty;
            var assembly = Assembly.GetEntryAssembly();
            var resourceStream = assembly.GetManifestResourceStream("ConsultingBot.Cards.AddToProjectCard.json");
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                templateJson = await reader.ReadToEndAsync();
            }

            var dataJson = JsonConvert.SerializeObject(requestDetails);

            var transformer = new AdaptiveTransformer();
            var cardJson = transformer.Transform(templateJson, dataJson);

            var result = AdaptiveCard.FromJson(cardJson).Card;
            return result;
        }

    }
}
