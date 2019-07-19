using ConsultingBot.Cards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingBot.InvokeActivityHandlers
{
    public class ProjectsMessagingExtension
    {
        // Called when the messaging extension query is entered
        public async Task<InvokeResponse> HandleMessagingExtensionQueryAsync(ITurnContext turnContext, MessagingExtensionQuery query)
        {
            string queryText = "";
            queryText = query?.Parameters.FirstOrDefault(p => p.Name == "queryText").Value as string;
            // TODO: Get items matching query here
            // TODO: Build list of MessagingExtensionAttachment objects and return it
            var heroCard = new HeroCard("Result Card", null, $"<pre>Query result for {queryText}</pre>");
            var previewCard = new ThumbnailCard("Search Item Card", null, "This is to show the search result");
            return new InvokeResponse
            {
                Body = new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult()
                    {
                        Type = "result",
                        AttachmentLayout = "list",
                        Attachments = new List<MessagingExtensionAttachment>()
                            {
                                heroCard.ToAttachment().ToMessagingExtensionAttachment(previewCard.ToAttachment()),
                                heroCard.ToAttachment().ToMessagingExtensionAttachment(previewCard.ToAttachment()),
                            },
                    },
                },
                Status = 200,
            };
        }

        // Called when the task module is fetched for an action
        public async Task<InvokeResponse> HandleMessagingExtensionFetchTaskAsync(ITurnContext turnContext, MessagingExtensionAction query)
        {
            var responseCard = new TaskModuleResponseCard();

            return new InvokeResponse
            {
                Status = 200,
                Body = new MessagingExtensionActionResponse
                {
                    Task = responseCard.TaskModuleResponseTask(query, false),
                },
            };
        }



    }
}
