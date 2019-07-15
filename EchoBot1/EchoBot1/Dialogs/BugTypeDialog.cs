using EchoBot1.Helpers;
using EchoBot1.Services;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBot1.Dialogs
{
    public class BugTypeDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;

        public BugTypeDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base (dialogId)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new ArgumentNullException(nameof(botServices));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(BugTypeDialog)}.mainFlow", waterfallSteps));

            InitialDialogId = $"{nameof(BugTypeDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var luisResult = result.Properties["luisResult"] as LuisResult;
            var entities = luisResult.Entities;

            if (entities.Count > 0)
            {
                foreach (var entity in entities)
                {
                    if (Common.BugTypes.Any(s => s.Equals(entity.Entity, StringComparison.OrdinalIgnoreCase)))
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Yes, {entity.Entity} is a bug type"));
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"No, {entity.Entity} is not a bug type"));
                    }
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Not sure what you think might be a bug type"));
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
