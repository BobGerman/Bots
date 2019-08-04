using AdaptiveCards;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsultingBot.Cards
{
    public class SampleCardSelectionCard
    {
        private const string defaultSampleChoiceValue = "1";
        private const string defaultUserText = "";

        public static AdaptiveCard GetCard(string sampleChoiceValue = defaultSampleChoiceValue, string userText = defaultUserText)
        {
            var card = new AdaptiveCard();

            card.Body.Add(new AdaptiveCards.AdaptiveTextBlock("Please select a card sample:")
            {
                Size = AdaptiveCards.AdaptiveTextSize.Large,
                Weight = AdaptiveCards.AdaptiveTextWeight.Bolder,
            });

            if (sampleChoiceValue == "refresh")
            {
                card.Body.Add(new AdaptiveCards.AdaptiveTextBlock($"This card has been refreshed. You previously entered: {userText}")
                {
                    Color = AdaptiveCards.AdaptiveTextColor.Attention
                });
            }

            card.Body.Add(new AdaptiveCards.AdaptiveChoiceSetInput()
            {
                Id = "sampleChoice",
                Style = AdaptiveChoiceInputStyle.Compact,
                IsMultiSelect = false,
                Value = sampleChoiceValue,
                Choices = new List<AdaptiveChoice>()
                {
                    new AdaptiveChoice()
                    {
                        Title = "Sample 1",
                        Value = "1"
                    },
                    new AdaptiveChoice()
                    {
                        Title = "Sample 2",
                        Value = "2"
                    },
                    new AdaptiveChoice()
                    {
                        Title = "Sample 3",
                        Value = "3"
                    },
                    new AdaptiveChoice()
                    {
                        Title = "Refresh this card",
                        Value = "refresh"
                    },
                }
            });

            card.Body.Add(new AdaptiveCards.AdaptiveTextInput()
            {
                Id = "userText",
                Placeholder = "Type text here...",
                Value = userText,
            });

            card.Actions.Add(new AdaptiveCards.AdaptiveSubmitAction()
            {
                Title = "Submit",
                Data = JObject.Parse(@"{ ""done"": true }"),
            });

            return card;
        }

        public static TaskModuleResponseBase GetSampleCardTaskModuleResponse(string sampleChoiceValue = defaultSampleChoiceValue, string userText = defaultUserText)
        {
            return new Microsoft.Bot.Schema.Teams.TaskModuleContinueResponse()
            {
                Type = "continue",
                Value = new TaskModuleTaskInfo()
                {
                    Title = "Select a sample",
                    Card = GetCard(sampleChoiceValue, userText).ToAttachment()
                }
            };
        }


    }
}
