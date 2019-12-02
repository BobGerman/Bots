using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsultingBot.Cards
{
    public static class AddToProjectCard
    {
        public const string SubmissionId = "AddToProjectSubmit";

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

        public class CardSubmitData
        {
            public string submissionId { get; set; }
            public string personName { get; set; }
            public string clientName { get; set; }
            public string projectId { get; set; }
            public string forecast0 { get; set; }
            public string forecast1 { get; set; }
            public string forecast2 { get; set; }
        }

        public static async Task<InvokeResponse> OnSubmit(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var val = turnContext.Activity.Value as JObject;
            var payload = val.ToObject<CardSubmitData>();

            var templateJson = String.Empty;
            var assembly = Assembly.GetEntryAssembly();
            var resourceStream = assembly.GetManifestResourceStream("ConsultingBot.Cards.AddToProjectConfirmationCard.json");
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                templateJson = await reader.ReadToEndAsync();
            }

            var dataJson = JsonConvert.SerializeObject(payload);

            var transformer = new AdaptiveTransformer();
            var cardJson = transformer.Transform(templateJson, dataJson);

            var card = AdaptiveCard.FromJson(cardJson).Card;

            var replyActivity = turnContext.Activity.CreateReply();
            replyActivity.Attachments.Add(card.ToAttachment());

            await turnContext.SendActivityAsync(replyActivity).ConfigureAwait(false);
            //await turnContext.UpdateActivityAsync(replyActivity).ConfigureAwait(false);

            return new InvokeResponse() { Status = 200 };
        }

    }
}
