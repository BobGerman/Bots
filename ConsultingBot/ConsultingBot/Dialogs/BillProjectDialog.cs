using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace ConsultingBot.Dialogs
{
    public class BillProjectDialog : CancelAndHelpDialog
    {
        public BillProjectDialog(string dialogId) : base(dialogId)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ProjectStepAsync,
                TimeWorkedAsync,
                DeliveryDateAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // Step 1: Ensure we have a project name
        // Result is the project name from LUIS or from a user prompt
        private async Task<DialogTurnResult> ProjectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var requestDetails = stepContext.Options is RequestDetails
                    ? stepContext.Options as RequestDetails
                    : new RequestDetails();

            if (requestDetails.projectName == null)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Which project do you want to add to?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(requestDetails.projectName, cancellationToken);
            }
        }

        // Step 2: Save the project name and ensure we have a duration
        // Result is the duration from LUIS or from a user prompt
        private async Task<DialogTurnResult> TimeWorkedAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var requestDetails = stepContext.Options is RequestDetails
                    ? stepContext.Options as RequestDetails
                    : new RequestDetails();

            requestDetails.projectName = (string)stepContext.Result;

            if (requestDetails.workDuration == 0)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("How many hours did you work?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync((int)requestDetails.workDuration / 60, cancellationToken);
            }
        }

        // Step 3: Save the work duration and ensure we have work date
        // Result is the work date from LUIS or from a user prompt

        private async Task<DialogTurnResult> DeliveryDateAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var requestDetails = stepContext.Options is RequestDetails
                    ? stepContext.Options as RequestDetails
                    : new RequestDetails();

            requestDetails.workDuration = (int)stepContext.Result;

            if (requestDetails.workDate == null || IsAmbiguous(requestDetails.workDate))
            {
                return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), requestDetails.workDate, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(requestDetails.workDate, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var requestDetails = stepContext.Options is RequestDetails
                    ? stepContext.Options as RequestDetails
                    : new RequestDetails();

            requestDetails.workDate = (string)stepContext.Result;

            var msg = $"Please confirm that you worked for {requestDetails.workDuration} hours on the {requestDetails.projectName} on {requestDetails.workDate}";

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(msg) }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var requestDetails = stepContext.Options is RequestDetails
                        ? stepContext.Options as RequestDetails
                        : new RequestDetails();

                return await stepContext.EndDialogAsync(requestDetails, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}
