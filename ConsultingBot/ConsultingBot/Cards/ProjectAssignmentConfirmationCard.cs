using AdaptiveCards;
using ConsultingBot.InvokeActivityHandlers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsultingBot.Cards
{
    public static class ProjectAssignmentConfirmationCard
    {
        public static AdaptiveCard GetCard(ProjectAssignmentCard.ProjectAssignmentCardSubmitValue value)
        {
            var card = new AdaptiveCard();
            var factSet = new AdaptiveFactSet();
            factSet.Facts.Add(new AdaptiveFact("Client", value.clientName));
            factSet.Facts.Add(new AdaptiveFact("Project", value.projectName));
            factSet.Facts.Add(new AdaptiveFact("Consultant", value.personName));
            factSet.Facts.Add(new AdaptiveFact("Role", value.roleChoice));
            factSet.Facts.Add(new AdaptiveFact($"Forecast for {value.forecastMonth1}",
                String.IsNullOrEmpty(value.forecast1) ? "0" : value.forecast1));
            factSet.Facts.Add(new AdaptiveFact($"Forecast for {value.forecastMonth2}",
                String.IsNullOrEmpty(value.forecast2) ? "0" : value.forecast2));
            factSet.Facts.Add(new AdaptiveFact($"Forecast for {value.forecastMonth3}",
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
