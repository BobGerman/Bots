namespace Microsoft.Bot.Builder.Teams.MessagingExtensionBot.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Abstractions.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Newtonsoft.Json.Linq;

    public class TeamsInvokeActivityHandler : TeamsInvokeActivityHandlerBase
    {

        // Called when the messaging extension query is entered
        public override async Task<InvokeResponse> HandleMessagingExtensionQueryAsync(ITurnContext turnContext, MessagingExtensionQuery query)
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
        public override async Task<InvokeResponse> HandleMessagingExtensionFetchTaskAsync(ITurnContext turnContext, MessagingExtensionAction query)
        {
            return new InvokeResponse
            {
                Status = 200,
                Body = new MessagingExtensionActionResponse
                {
                    Task = this.TaskModuleResponseTask(query, false),
                },
            };
        }

        // Called when the task module from an action messaging extension  is submitted
        public override async Task<InvokeResponse> HandleMessagingExtensionSubmitActionAsync(ITurnContext turnContext, MessagingExtensionAction query)
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
                            Attachments = new List<Attachment> { this.TaskModuleResponseCard(query, null) },
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
                body.Task = this.TaskModuleResponseTask(query, false);
            }

            return new InvokeResponse
            {
                Status = 200,
                Body = body,
            };
        }

        // Called when an Invoke button on a card is clicked
        public override async Task<InvokeResponse> HandleInvokeTaskAsync(ITurnContext turnContext)
        {
            return await base.HandleInvokeTaskAsync(turnContext);
        }

        // Called when a Task Module Action on a card is clicked and the task module needs to be rendered
        public override async Task<InvokeResponse> HandleTaskModuleFetchAsync(ITurnContext turnContext, TaskModuleRequest query)
        {
            return new InvokeResponse
            {
                Status = 200,
                Body = new TaskModuleResponse
                {
                    Task = this.TaskModuleResponseTask(query, false),
                },
            };
        }

        // Called when a task module from a card is submitted
        public override async Task<InvokeResponse> HandleTaskModuleSubmitAsync(ITurnContext turnContext, TaskModuleRequest query)
        {
            bool done = false;
            if (query.Data != null)
            {
                var data = JObject.FromObject(query.Data);
                done = (bool)data["done"];
            }

            return new InvokeResponse
            {
                Status = 200,
                Body = new TaskModuleResponse
                {
                    Task = this.TaskModuleResponseTask(query, done),
                },
            };
        }

        // Called when a link message handler runs (i.e. we render a preview to a link whose domain is 
        // included in the messageHandlers in the manifest)
        public override async Task<InvokeResponse> HandleAppBasedLinkQueryAsync(ITurnContext turnContext, AppBasedLinkQuery query)
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

        // Called when fetching the contents of the task module
        private TaskModuleResponseBase TaskModuleResponseTask(TaskModuleRequest query, bool done)
        {
            if (done)
            {
                return new TaskModuleMessageResponse()
                {
                    Type = "message",
                    Value = "Thanks for your inputs!",
                };
            }
            else
            {
                string textValue = null;
                if (query.Data != null)
                {
                    var data = JObject.FromObject(query.Data);
                    textValue = data["userText"]?.ToString();
                }

                return new TaskModuleContinueResponse()
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Title = "More Page",
                        Card = this.TaskModuleResponseCard(query, textValue),
                    },
                };
            }
        }

        // Builds the response card displayed in the task module
        private Attachment TaskModuleResponseCard(TaskModuleRequest query, string textValue)
        {
            AdaptiveCards.AdaptiveCard adaptiveCard = new AdaptiveCards.AdaptiveCard();

            adaptiveCard.Body.Add(new AdaptiveCards.AdaptiveTextBlock("Your Request:")
            {
                Size = AdaptiveCards.AdaptiveTextSize.Large,
                Weight = AdaptiveCards.AdaptiveTextWeight.Bolder,
            });

            adaptiveCard.Body.Add(new AdaptiveCards.AdaptiveContainer()
            {
                Style = AdaptiveCards.AdaptiveContainerStyle.Emphasis,
                Items = new List<AdaptiveCards.AdaptiveElement>
                {
                    new AdaptiveCards.AdaptiveTextBlock(JObject.FromObject(query).ToString())
                    {
                        Wrap = true,
                    },
                },
            });

            adaptiveCard.Body.Add(new AdaptiveCards.AdaptiveTextInput()
            {
                Id = "userText",
                Placeholder = "Type text here...",
                Value = textValue,
            });

            adaptiveCard.Actions.Add(new AdaptiveCards.AdaptiveSubmitAction()
            {
                Title = "Next",
                Data = JObject.Parse(@"{ ""done"": false }"),
            });

            adaptiveCard.Actions.Add(new AdaptiveCards.AdaptiveSubmitAction()
            {
                Title = "Submit",
                Data = JObject.Parse(@"{ ""done"": true }"),
            });

            return adaptiveCard.ToAttachment();
        }
    }
}
