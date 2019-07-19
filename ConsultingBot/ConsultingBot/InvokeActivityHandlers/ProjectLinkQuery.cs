using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingBot.InvokeActivityHandlers
{
    public class ProjectLinkQuery
    {
        // Called when a link message handler runs (i.e. we render a preview to a link whose domain is 
        // included in the messageHandlers in the manifest)
        public async Task<InvokeResponse> HandleAppBasedLinkQueryAsync(ITurnContext turnContext, AppBasedLinkQuery query)
        {
            var previewImg = new List<CardImage>
            {
                new CardImage("https://assets.pokemon.com/assets/cms2/img/pokedex/full/025.png", "Pokemon"),
            };
            var preview = new ThumbnailCard("Preview Card", null, $"Your query URL: {query.Url}", previewImg).ToAttachment();
            var heroCard = new HeroCard("Preview Card", null, $"Your query URL: <pre>{query.Url}</pre>", previewImg).ToAttachment();
            var resultCards = new List<MessagingExtensionAttachment> { heroCard.ToMessagingExtensionAttachment(preview) };

            return new InvokeResponse
            {
                Status = 200,
                Body = new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult("list", "result", resultCards),
                },
            };
        }
    }
}
