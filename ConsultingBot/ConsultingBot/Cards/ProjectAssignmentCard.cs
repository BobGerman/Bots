using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsultingBot.Cards
{
    public static class ProjectAssignmentCard
    {
        public static AdaptiveCard GetCard(ITurnContext turnContext, RequestDetails requestDetails)
        {
            var card = new AdaptiveCard();

            card.Body.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveCards.AdaptiveColumn>()
                {
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveImage(requestDetails.project.Client.LogoUrl)
                        }
                    },
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                             new AdaptiveTextBlock($"{requestDetails.project.Client.Name}"),
                             new AdaptiveTextBlock($"{requestDetails.project.Name}"),
                             new AdaptiveTextBlock($"Adding {requestDetails.personName}"),
                        }
                    },
                }
            });

            card.Body.Add(new AdaptiveCards.AdaptiveChoiceSetInput()
            {
                Id = "roleChoice",
                Style = AdaptiveChoiceInputStyle.Compact,
                IsMultiSelect = false,
                Value = "1",
                Choices = new List<AdaptiveChoice>()
                {
                    new AdaptiveChoice()
                    {
                        Title = "Architect",
                        Value = "Architect"
                    },
                    new AdaptiveChoice()
                    {
                        Title = "Developer",
                        Value = "Developer"
                    },
                    new AdaptiveChoice()
                    {
                        Title = "Project Manager",
                        Value = "Project Manager"
                    },
                }
            });

            var month1Name = GetMonthFromNow(0).ToString("MMMM");
            card.Body.Add(new AdaptiveTextBlock($"Forecast for {month1Name}"));
            card.Body.Add(new AdaptiveCards.AdaptiveTextInput()
            {
                Id = "forecast1",
                Placeholder = "0",
                Value = "",
            });

            var month2Name = GetMonthFromNow(1).ToString("MMMM");
            card.Body.Add(new AdaptiveTextBlock($"Forecast for {month2Name}"));
            card.Body.Add(new AdaptiveCards.AdaptiveTextInput()
            {
                Id = "forecast2",
                Placeholder = "0",
                Value = "",
            });
            var month3Name = GetMonthFromNow(2).ToString("MMMM");
            card.Body.Add(new AdaptiveTextBlock($"Forecast for {month3Name}"));
            card.Body.Add(new AdaptiveCards.AdaptiveTextInput()
            {
                Id = "forecast3",
                Placeholder = "0",
                Value = "",
            });

            var payload = new ProjectAssignmentCardActionValue()
            {
                submissionId = ProjectAssignmentCard.SubmissionId,
                clientUrl = requestDetails.project.Client.LogoUrl,
                clientName = requestDetails.project.Client.Name,
                projectName = requestDetails.project.Name,
                personName = requestDetails.personName,
                forecastMonth1 = month1Name,
                forecastMonth2 = month2Name,
                forecastMonth3 = month3Name,
            };

            CardAction action = new CardAction()
            {
                Title = "Submit",
                Type = "invoke",
                Value = JsonConvert.SerializeObject(payload)
            };

            card.Actions.Add(action.ToAdaptiveCardAction());

            return card;
        }

        // GetMonthFromNow() - returns the 1st of the month +/- delta months
        private static DateTime GetMonthFromNow(int delta)
        {
            var now = DateTime.Now;
            var month = ((now.Month-1+delta) % 12) + 1;
            var year = (now.Year + (month < now.Month ? 1 : 0));
            return new DateTime(year, month, 1);
        }

        public static async Task<InvokeResponse> HandleInvokeActivityAsync(ITurnContext turnContext)
        {
            var val = turnContext.Activity.Value as JObject;
            var payload = val.ToObject<ProjectAssignmentCardSubmitValue>();

            var responseCard = ProjectAssignmentConfirmationCard.GetCard(payload);

            var replyActivity = turnContext.Activity.CreateReply();
            replyActivity.Attachments.Add(responseCard.ToAttachment());

            await turnContext.SendActivityAsync(replyActivity).ConfigureAwait(false);
            //await turnContext.UpdateActivityAsync(replyActivity).ConfigureAwait(false);

            return new InvokeResponse() { Status = 200 };
        }

        public static string SubmissionId { get; } = nameof(ProjectAssignmentCard) + "submit";


        public class ProjectAssignmentCardActionValue : CardActionValue
        {
            // From request details
            public string clientUrl = "";
            public string clientName = "";
            public string projectName = "";
            public string personName = "";
            public string forecastMonth1 = "";
            public string forecastMonth2 = "";
            public string forecastMonth3 = "";
        }

        public class ProjectAssignmentCardSubmitValue : ProjectAssignmentCardActionValue
        {
            // From adaptive card
            public string roleChoice = "";
            public string forecast1 = "0";
            public string forecast2 = "0";
            public string forecast3 = "0";
        }
    }
}
