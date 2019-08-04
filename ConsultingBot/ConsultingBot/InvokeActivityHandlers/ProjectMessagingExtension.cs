using ConsultingBot.Cards;
using ConsultingData.Models;
using ConsultingData.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingBot.InvokeActivityHandlers
{
    public class ProjectMessagingExtension : IInvokeActivityService
    {
        private TestCard testCard = new TestCard();

        private readonly IConfiguration configuration;
        public ProjectMessagingExtension(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        #region Dispatcher for this Messaging Extension
        // TODO: Move into base class
        public async Task<InvokeResponse> HandleInvokeActivityAsync(ITurnContext turnContext)
        {
            ITeamsContext teamsContext = turnContext.TurnState.Get<ITeamsContext>();
            if (teamsContext.IsRequestMessagingExtensionQuery())
            {
                return await HandleMessagingExtensionQueryAsync(turnContext, teamsContext.GetMessagingExtensionQueryData()).ConfigureAwait(false);
            }

            if (teamsContext.IsRequestMessagingExtensionFetchTask())
            {
                return await HandleMessagingExtensionFetchTaskAsync(turnContext, teamsContext.GetMessagingExtensionActionData()).ConfigureAwait(false);
            }

            if (teamsContext.IsRequestMessagingExtensionSubmitAction())
            {
                return await HandleMessagingExtensionSubmitActionAsync(turnContext, teamsContext.GetMessagingExtensionActionData()).ConfigureAwait(false);
            }

            return await Task.FromResult<InvokeResponse>(null);
        }
        #endregion

        // Called when the messaging extension query is entered
        private async Task<InvokeResponse> HandleMessagingExtensionQueryAsync(ITurnContext turnContext, MessagingExtensionQuery query)
        {
            var queryText = "";
            queryText = query?.Parameters.FirstOrDefault(p => p.Name == "queryText").Value as string;

            var consultingDataService = new ConsultingDataService();
            var projects = await consultingDataService.GetProjects(queryText);
            var attachments = new List<MessagingExtensionAttachment>();
            foreach (var project in projects)
            {
                var resultCard = ProjectResultsCard.GetCard(project, getMapUrl(project.Client));
                var previewCard = ProjectPreviewCard.GetCard(project);
                attachments.Add(resultCard.ToAttachment().ToMessagingExtensionAttachment(previewCard.ToAttachment()));
            }

            return new InvokeResponse
            {
                Body = new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult()
                    {
                        Type = "result",
                        AttachmentLayout = "list",
                        Attachments = attachments
                    },
                },
                Status = 200,
            };
        }

        // Called when the task module is fetched for an action
        private async Task<InvokeResponse> HandleMessagingExtensionFetchTaskAsync(ITurnContext turnContext, MessagingExtensionAction query)
        {
            return new InvokeResponse
            {
                Status = 200,
                Body = new MessagingExtensionActionResponse
                {
                    Task = SampleCardSelectionCard.GetSampleCardTaskModuleResponse(),
                },
            };
        }

        // Called when the task module from an action messaging extension  is submitted
        private async Task<InvokeResponse> HandleMessagingExtensionSubmitActionAsync(ITurnContext turnContext, MessagingExtensionAction query)
        {
            bool done = false;
            string sampleChoice = "";
            string userText = "";

            JObject data = null;
            if (query.Data != null)
            {
                data = JObject.FromObject(query.Data);
                sampleChoice = (string)data["sampleChoice"];
                userText = (string)data["userText"];
                done = (bool)data["done"];
            }

            var body = new MessagingExtensionActionResponse();
            if (data != null && done && sampleChoice != "refresh")
            {
                switch (sampleChoice)
                {
                    case "1":
                        {
                            // Sample Hero Card
                            var card = SampleHeroCard.GetCard(userText);
                            var preview = new ThumbnailCard("Created Card (preview)", null, $"Your input: {userText}").ToAttachment();
                            var resultCards = new List<MessagingExtensionAttachment> {
                               card.ToAttachment().ToMessagingExtensionAttachment(preview)
                            };
                            body.ComposeExtension = new MessagingExtensionResult("list", "result", resultCards);
                            break;
                        }
                    default:
                        {
                            // Sample project card
                            var consultingDataService = new ConsultingDataService();
                            var project = (await consultingDataService.GetProjects()).FirstOrDefault();

                            var card = ProjectPreviewCard.GetCard(project);
                            var preview = new ThumbnailCard("Created Card (preview)", null, $"Your input: {userText}").ToAttachment();
                            var resultCards = new List<MessagingExtensionAttachment> {
                                card.ToAttachment().ToMessagingExtensionAttachment(preview)
                            };
                            body.ComposeExtension = new MessagingExtensionResult("list", "result", resultCards);
                            break;
                        }
                }
                return new InvokeResponse
                {
                    Status = 200,
                    Body = body,
                };
            }
            else
            {
                return new InvokeResponse
                {
                    Status = 200,
                    Body = new MessagingExtensionActionResponse
                    {
                        Task = SampleCardSelectionCard.GetSampleCardTaskModuleResponse(sampleChoice, userText)
                    },
                };
            }
        }

        private string getMapUrl(ConsultingClient client)
        {
            string coordinates = $"{ client.Latitude.ToString() },{ client.Longitude.ToString()}";

            string bingMapsKey = this.configuration["BingMapsAPIKey"];
            string result = $"https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/?{ coordinates }mapSize=450,600&pp={ coordinates }&key={ bingMapsKey }";

            return result;
        }
    }
}
