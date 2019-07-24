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
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // Step 1: Figure out the user's intent and run the appropriate dialog to act on it
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

                var requestDetails = stepContext.Context.Activity.Text != null
                        ?
                    await LuisConsultingProjectRecognizer.ExecuteQuery(Configuration, Logger, stepContext.Context, cancellationToken)
                        :
                    new RequestDetails();

                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Got intent of {requestDetails.intent}\nProject: {requestDetails.projectName}\nPerson: {requestDetails.personName}\nMinutes: {requestDetails.workHours}\nWhen: {requestDetails.workDate}"), cancellationToken);

                switch (requestDetails.intent)
                {
                    case Intent.AddToProject:
                        {
                            return await stepContext.BeginDialogAsync(nameof(AddToProjectDialog), requestDetails, cancellationToken);
                        }
                    case Intent.BillToProject:
                        {
                            return await stepContext.BeginDialogAsync(nameof(BillProjectDialog), requestDetails, cancellationToken);
                        }
                }

                requestDetails.intent = Intent.Unknown;
                return await stepContext.NextAsync(requestDetails, cancellationToken);
            }

        }

        // Step 2: Confirm the final outcome
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var confirmationMessage = "OK never mind!";
            var result = stepContext.Result as RequestDetails;

            // If the child dialog was cancelled or the user failed to confirm, the result will be null.
            if (result != null)
            {
                switch (result.intent)
                {
                    case Intent.AddToProject:
                        {
                            confirmationMessage = $"I'm confirming that you have added {result.personName} to the {result.projectName} project. Thank you for using ConsultingBot.";
                            break;
                        }
                    case Intent.BillToProject:
                        {
                            var timeProperty = new TimexProperty(result.workDate);
                            var deliveryDateText = timeProperty.ToNaturalLanguage(DateTime.Now);
                            confirmationMessage = $"I'm charging {result.projectName} for {result.workHours} minutes on {deliveryDateText}. Thank you for using ConsultingBot.";
                            break;
                        }
                    default:
                        {
                            confirmationMessage = "Sorry, I don't understand what you typed";
                            break;
                        }
                }
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(confirmationMessage), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
