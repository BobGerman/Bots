using AdaptiveCards;
using System;
using System.Collections.Generic;

namespace ConsultingBot.Cards
{
    public static class ProjectAssignmentConfirmationCard
    {
        public static AdaptiveCard GetCard(ProjectAssignmentCard.ProjectAssignmentCardSubmitValue value)
        {
            var card = new AdaptiveCard();
            card.Body.Add(new AdaptiveTextBlock($"Adding {value.personName} to project")
            {
                Weight = AdaptiveTextWeight.Bolder,
                Size = AdaptiveTextSize.Large,
            });

            // Display details in a fact set
            var factSet = new AdaptiveFactSet();
            factSet.Facts.Add(new AdaptiveFact("Client", value.clientName));
            factSet.Facts.Add(new AdaptiveFact("Project", value.projectName));
            factSet.Facts.Add(new AdaptiveFact("Role", value.roleChoice));
            factSet.Facts.Add(new AdaptiveFact($"{value.forecastMonth1} forecast",
                String.IsNullOrEmpty(value.forecast1) ? "0" : value.forecast1));
            factSet.Facts.Add(new AdaptiveFact($"{value.forecastMonth2} forecast",
                String.IsNullOrEmpty(value.forecast2) ? "0" : value.forecast2));
            factSet.Facts.Add(new AdaptiveFact($"{value.forecastMonth3} forecast",
                String.IsNullOrEmpty(value.forecast3) ? "0" : value.forecast3));

            card.Body.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveCards.AdaptiveColumn>()
                {
                    new AdaptiveColumn()
                    {
                        Width = "35%",
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveImage(value.clientUrl)
                        }
                    },
                    new AdaptiveColumn()
                    {
                        Width = "65%",
                        Items = new List<AdaptiveElement>()
                        {
                             factSet
                        }
                    },
                }
            });

            return card;
        }
    }
}
