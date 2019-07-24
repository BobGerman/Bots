// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace ConsultingBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        protected readonly ILogger Logger;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            Configuration = configuration;
            Logger = logger;

            // Add child dialogs we may use
            AddDialog(new AddToProjectDialog(nameof(AddToProjectDialog)));
            AddDialog(new BillProjectDialog(nameof(BillProjectDialog)));
            AddDialog(new UnknownIntentDialog(nameof(UnknownIntentDialog)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                //ActStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(Configuration["LuisAppId"]) || string.IsNullOrEmpty(Configuration["LuisAPIKey"]) || string.IsNullOrEmpty(Configuration["LuisAPIHostName"]))
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file."), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Echo: {stepContext.Context.Activity.Text}"), cancellationToken);

                var projectIntentDetails = stepContext.Context.Activity.Text != null
                        ?
                    await LuisConsultingProjectRecognizer.ExecuteQuery(Configuration, Logger, stepContext.Context, cancellationToken)
                        :
                    new ProjectIntentDetails();

                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Got intent of {projectIntentDetails.intent}\nProject: {projectIntentDetails.projectName}\nPerson: {projectIntentDetails.personName}\nMinutes: {projectIntentDetails.taskDurationMinutes}\nWhen: {projectIntentDetails.deliveryDate}"), cancellationToken);

                switch (projectIntentDetails.intent)
                {
                    case ProjectIntent.AddToProject:
                        {
                            return await stepContext.BeginDialogAsync(nameof(AddToProjectDialog), projectIntentDetails, cancellationToken);
                        }
                    case ProjectIntent.BillToProject:
                        {
                            return await stepContext.BeginDialogAsync(nameof(BillProjectDialog), projectIntentDetails, cancellationToken);
                        }
                }

                return await stepContext.BeginDialogAsync(nameof(UnknownIntentDialog), projectIntentDetails, cancellationToken);
            }

        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var confirmationMessage = "Thank you for using ConsultingBot";
            var result = stepContext.Result as ProjectIntentDetails;

            // If the child dialog ("AddToProjectDialog" etc.) was cancelled or the user failed to confirm,
            // the Result here will be null.
            if (result != null)
            {
                switch (result.intent)
                {
                    case ProjectIntent.AddToProject:
                        {
                            confirmationMessage = $"I'm confirming that you have added {result.personName} to the {result.projectName} project. Thank you for using ConsultingBot.";
                            break;
                        }
                    case ProjectIntent.BillToProject:
                        {
                            var timeProperty = new TimexProperty(result.deliveryDate);
                            var deliveryDateText = timeProperty.ToNaturalLanguage(DateTime.Now);
                            confirmationMessage = $"I'm charging {result.projectName} for {result.taskDurationMinutes} minutes on {deliveryDateText}. Thank you for using ConsultingBot.";
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(confirmationMessage), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
