using AdaptiveCards;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ConsultingBot.Cards
{
    public static class ProjectAssignmentCard
    {
        public static AdaptiveCard GetCard(RequestDetails requestDetails)
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
                        Value = "1"
                    },
                    new AdaptiveChoice()
                    {
                        Title = "Developer",
                        Value = "2"
                    },
                    new AdaptiveChoice()
                    {
                        Title = "Project Manager",
                        Value = "3"
                    },
                }
            });

            var monthName = DateTime.Now.ToString("MMMM");
            card.Body.Add(new AdaptiveTextBlock($"Forecast for {monthName}"));
            card.Body.Add(new AdaptiveCards.AdaptiveTextInput()
            {
                Id = "forecast1",
                Placeholder = "0",
                Value = "",
            });

            monthName = new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1 % 12, 1).ToString("MMMM");
            card.Body.Add(new AdaptiveTextBlock($"Forecast for {monthName}"));
            card.Body.Add(new AdaptiveCards.AdaptiveTextInput()
            {
                Id = "forecast2",
                Placeholder = "0",
                Value = "",
            });
            monthName = new DateTime(DateTime.Now.Year, DateTime.Now.Month + 2 % 12, 1).ToString("MMMM");
            card.Body.Add(new AdaptiveTextBlock($"Forecast for {monthName}"));
            card.Body.Add(new AdaptiveCards.AdaptiveTextInput()
            {
                Id = "forecast3",
                Placeholder = "0",
                Value = "",
            });
            string submitJson = @"{""msteams"":{""type"":""messageBack"",""displayText"":""I clicked this button"",""text"":""text to bots"",""value"":""""} }";
            card.Actions.Add(new AdaptiveCards.AdaptiveSubmitAction()
            {
                Title = "Submit",
                Data = JObject.Parse(submitJson)
            });

            return card;
        }
    }
}
