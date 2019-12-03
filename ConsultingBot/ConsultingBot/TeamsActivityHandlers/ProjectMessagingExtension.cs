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

namespace ConsultingBot.TeamsActivityHandlers
{
    public class ProjectMessagingExtension
    {
        private TestCard testCard = new TestCard();

        private readonly IConfiguration configuration;
        public ProjectMessagingExtension(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // Called when the messaging extension query is entered
        public async Task<MessagingExtensionResponse> HandleMessagingExtensionQueryAsync(ITurnContext turnContext, MessagingExtensionQuery query)
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

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult()
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments
                }
            };
        }

        // Called when the task module is fetched for an action
        public async Task<MessagingExtensionActionResponse> HandleMessagingExtensionFetchTaskAsync(ITurnContext turnContext, MessagingExtensionAction query)
        {
            var emptyRequest = new RequestDetails();
            ConsultingDataService dataService = new ConsultingDataService();
            emptyRequest.possibleProjects = await dataService.GetProjects("");
            IEnumerable<TeamsChannelAccount> members = await TeamsInfo.GetMembersAsync(turnContext);
            emptyRequest.possiblePersons = members.Select((w) => new Person
                                                   {
                                                      name = w.Name,
                                                      email = w.Email
                                                   })
                                                   .ToList();

            var card = await AddToProjectCard.GetCard(turnContext, emptyRequest);
            var response = new Microsoft.Bot.Schema.Teams.TaskModuleContinueResponse()
            {
                Type = "continue",
                Value = new TaskModuleTaskInfo()
                {
                    Title = "Select a sample",
                    Card = card.ToAttachment()
                }
            };

            return new MessagingExtensionActionResponse
            {
                Task = response
            };
        }

        // Called when the task module from an action messaging extension  is submitted
        public async Task<MessagingExtensionActionResponse> HandleMessagingExtensionSubmitActionAsync(ITurnContext turnContext, MessagingExtensionAction query)
        {
            var val = JObject.FromObject(query.Data); // turnContext.Activity.Value as JObject;
            var payload = val.ToObject<AddToProjectCard.AddToProjectCardActionValue>();
            var submitData = val["msteams"]["value"];
            payload.submissionId = submitData.Value<string>("submissionId");
            payload.command = submitData.Value<string>("command");
            payload.monthZero = submitData.Value<string>("monthZero");
            payload.monthOne = submitData.Value<string>("monthOne");
            payload.monthTwo = submitData.Value<string>("monthTwo");

            I left off here






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
                return body;
            }
            else
            {
                return new MessagingExtensionActionResponse
                {
                    Task = SampleCardSelectionCard.GetSampleCardTaskModuleResponse(sampleChoice, userText)
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
