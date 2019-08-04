using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace ConsultingBot.Cards
{
    public class SampleHeroCard
    {
        public static HeroCard GetCard(string userText)
        {
            HeroCard card = new HeroCard
            {
                Title = "Meet Hathor the Cat",
                Subtitle = "Really great cat",
                Text = userText,
                Images = new List<CardImage>(),
                Buttons = new List<CardAction>(),
            };
            card.Images.Add(new CardImage { Url = "https://hrtalentlab.azurewebsites.net/cat.png" });
            card.Buttons.Add(new CardAction
            {
                Title = "Love cats",
                Type = "openUrl",
                Value = "https://icanhas.cheezburger.com/lolcats"
            });
            card.Buttons.Add(new CardAction
            {
                Title = "Hate cats",
                Type = "openUrl",
                Value = "https://www.amazon.com/101-Uses-Dead-Simon-Bond/dp/0517545160"
            });

            return card;
        }
    }
}
