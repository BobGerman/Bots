using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;

namespace ConsultingBot.Cards
{
    public class TestCard : IMyCard
    {
        public Attachment GetCardAttachment(ITurnContext turnContext)
        {
            var teamsContext = turnContext.TurnState.Get<ITeamsContext>();
            TaskModuleRequest query = teamsContext.GetTaskModuleRequestData();
            string textValue = null;

            if (query.Data != null)
            {
                var data = JObject.FromObject(query.Data);
                textValue = data["userText"]?.ToString();
            }

            return this.GetResponseCard(query, textValue);
        }

        public async Task<InvokeResponse> HandleInvokeAsync(ITurnContext turnContext)
        {
            ITeamsContext teamsContext = turnContext.TurnState.Get<ITeamsContext>();

            if (teamsContext.IsRequestMessagingExtensionSubmitAction())
            {
                return await this.HandleMessagingExtensionSubmitActionAsync(turnContext, teamsContext.GetMessagingExtensionActionData()).ConfigureAwait(false);
            }

            if (teamsContext.IsRequestTaskModuleFetch())
            {
                return await this.HandleTaskModuleFetchAsync(turnContext, teamsContext.GetTaskModuleRequestData()).ConfigureAwait(false);
            }

            if (teamsContext.IsRequestTaskModuleSubmit())
            {
                return await this.HandleTaskModuleSubmitAsync(turnContext, teamsContext.GetTaskModuleRequestData()).ConfigureAwait(false);
            }

            return await this.HandleInvokeTaskAsync(turnContext).ConfigureAwait(false);
        }

        // Builds the response card displayed in the task module
        private Attachment GetResponseCard(TaskModuleRequest query, string textValue)
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
                            Attachments = new List<Attachment> { this.GetResponseCard(query, null) },
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

        // Called when a Task Module Action on a card is clicked and the task module needs to be rendered
        private async Task<InvokeResponse> HandleTaskModuleFetchAsync(ITurnContext turnContext, TaskModuleRequest query)
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
        private async Task<InvokeResponse> HandleTaskModuleSubmitAsync(ITurnContext turnContext, TaskModuleRequest query)
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

        private async Task<InvokeResponse> HandleInvokeTaskAsync(ITurnContext turnContext)
        {
            return await Task.FromResult<InvokeResponse>(null);
        }

        // Called when fetching the contents of the task module
        public TaskModuleResponseBase TaskModuleResponseTask(TaskModuleRequest query, bool done)
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
                        Card = this.GetResponseCard(query, textValue),
                    },
                };
            }
        }


    }
}
