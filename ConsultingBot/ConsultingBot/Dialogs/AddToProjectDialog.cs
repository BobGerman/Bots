using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace ConsultingBot.Dialogs
{
    public class AddToProjectDialog : CancelAndHelpDialog
    {
        public AddToProjectDialog(string dialogId) : base(dialogId)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ProjectStepAsync,
                PersonStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        // Step 1: Ensure we have a project name
        // Result is the project name from LUIS or from a user prompt
        private async Task<DialogTurnResult> ProjectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var projectIntentDetails = stepContext.Options is ProjectIntentDetails
                    ? stepContext.Options as ProjectIntentDetails
                    : new ProjectIntentDetails();

            if (string.IsNullOrEmpty(projectIntentDetails.projectName))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Which project did you work on?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(projectIntentDetails.projectName, cancellationToken);
            }
        }

        // Step 2: Save the project name and ensure we have a person name
        // Result is the person name from LUIS or from a user prompt
        private async Task<DialogTurnResult> PersonStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var projectIntentDetails = stepContext.Options is ProjectIntentDetails
                    ? stepContext.Options as ProjectIntentDetails
                    : new ProjectIntentDetails();
            projectIntentDetails.projectName = (string)stepContext.Result;

            if (string.IsNullOrEmpty(projectIntentDetails.personName))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Ok, who did you want to add to the {projectIntentDetails.projectName} project?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(projectIntentDetails.personName, cancellationToken);
            }
        }

        // Step 3: Save the person name and confirm with the user 
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var projectIntentDetails = stepContext.Options is ProjectIntentDetails
                    ?
                stepContext.Options as ProjectIntentDetails
                    :
                new ProjectIntentDetails();

            projectIntentDetails.personName = (string)stepContext.Result;

            var message = $"Please confirm, I'm adding: {projectIntentDetails.personName} to the {projectIntentDetails.projectName} project";

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(message) }, cancellationToken);
        }

        // Step 4: Display the final outcome
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var projectIntentDetails = stepContext.Options is ProjectIntentDetails
                        ?
                    stepContext.Options as ProjectIntentDetails
                        :
                    new ProjectIntentDetails();

                return await stepContext.EndDialogAsync(projectIntentDetails, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
    }
}
