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
        private TestCard testCard = new TestCard();

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
        public async Task<InvokeResponse> HandleMessagingExtensionSubmitActionAsync(ITurnContext turnContext, MessagingExtensionAction query)
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
    }
}
