using Microsoft.Bot.Builder.AI.Luis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBot1.Services
{
    public class BotServices
    {
        public LuisRecognizer Dispatch { get; private set; }

        public BotServices()
        {

        }
    }
}
