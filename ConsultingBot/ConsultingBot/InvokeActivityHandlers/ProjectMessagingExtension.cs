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
    public class ProjectMessagingExtension : IInvokeActivityHandler
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
            var projects = consultingDataService.GetProjects(queryText);
            var attachments = new List<MessagingExtensionAttachment>();
            foreach (var project in projects)
            {
                var resultCard = new HeroCard()
                {
                    Title = $"{ project.Client.Name } - { project.Name }",
                    Subtitle = $"{ project.Description }",
                    Text = $"{ project.Address }<br />{ project.City }, { project.State }, { project.Zip }<br />Contact is { project.Contact }",
                    Images = new List<CardImage>() { new CardImage() { Url = getMapUrl(project.Client) } },
                    Buttons = new List<CardAction>()
                    {
                        new CardAction() { Title = "Project Team" , Type = "openUrl", Value = project.TeamUrl },
                        new CardAction() { Title = "Project Documents" , Type = "openUrl", Value = project.DocumentsUrl }

                    }
                };
                var previewCard = new ThumbnailCard()
                {
                    Title = $"{project.Client.Name} - {project.Name}",
                    Text = project.Description,
                    Images = new List<CardImage>() { new CardImage() { Url = project.Client.LogoUrl } }
                };
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
            var taskModule = new TestTaskModule();

            return new InvokeResponse
            {
                Status = 200,
                Body = new MessagingExtensionActionResponse
                {
                    Task = taskModule.TaskModuleResponseTask(query, false),
                },
            };
        }

        // Called when the task module from an action messaging extension  is submitted
        private async Task<InvokeResponse> HandleMessagingExtensionSubmitActionAsync(ITurnContext turnContext, MessagingExtensionAction query)
        {
            // Inspect the query to determine if we're done
            bool done = false;
            JObject data = null;
            if (query.Data != null)
            {
                data = JObject.FromObject(query.Data);
                done = (bool)data["done"];
            }

            var body = new MessagingExtensionActionResponse();
            if (data != null && done)
            {
                // We are done, build a card based on our interaction and insert it into the compose box
                string sharedMessage = string.Empty;
                if (query.CommandId.Equals("shareMessage") && query.CommandContext.Equals("message"))
                {
                    sharedMessage = $"Shared message: <div style=\"background:#F0F0F0\">{JObject.FromObject(query.MessagePayload).ToString()}</div><br/>";
                }

                var preview = new ThumbnailCard("Created Card (preview)", null, $"Your input: {data["userText"]?.ToString()}").ToAttachment();
                var heroCard = new HeroCard("Created Card (hero)", null, $"{sharedMessage}Your input: {data["userText"]?.ToString()}").ToAttachment();
                var resultCards = new List<MessagingExtensionAttachment> {
                    heroCard.ToMessagingExtensionAttachment(preview)
                };

                body.ComposeExtension = new MessagingExtensionResult("list", "result", resultCards);
            }
            else if ((query.CommandId != null && query.CommandId.Equals("createWithPreview")) || query.BotMessagePreviewAction != null)
            {
                // We are not done so re-render the task module
                if (query.BotMessagePreviewAction == null)
                {
                    body.ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "botMessagePreview",
                        ActivityPreview = new Activity
                        {
                            Attachments = new List<Attachment> { testCard.GetCard(query, null) },
                        },
                    };
                }
                else
                {
                    // Something is wrong in the Teams client - handle it
                    var userEditActivities = query.BotActivityPreview;
                    var card = userEditActivities?[0]?.Attachments?[0];
                    if (card == null)
                    {
                        body.Task = new TaskModuleMessageResponse
                        {
                            Type = "message",
                            Value = "Missing user edit card. Something wrong on Teams client.",
                        };
                    }
                    else if (query.BotMessagePreviewAction.Equals("send"))
                    {
                        Activity activity = turnContext.Activity.CreateReply();
                        activity.Attachments = new List<Attachment> { card };
                        await turnContext.SendActivityAsync(activity).ConfigureAwait(false);
                    }
                    else if (query.BotMessagePreviewAction.Equals("edit"))
                    {
                        body.Task = new TaskModuleContinueResponse
                        {
                            Type = "continue",
                            Value = new TaskModuleTaskInfo
                            {
                                Card = card,
                            },
                        };
                    }
                }
            }
            else
            {
                // Update the task module
                TestTaskModule taskModule = new TestTaskModule();
                body.Task = taskModule.TaskModuleResponseTask(query, false);
            }

            return new InvokeResponse
            {
                Status = 200,
                Body = body,
            };
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
