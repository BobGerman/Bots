using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsultingData.Models;
using ConsultingData.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace ConsultingBot.Dialogs
{
    public class AddToProjectDialog : CancelAndHelpDialog
    {
        public AddToProjectDialog(string dialogId) : base(dialogId)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt) + "projectName", ProjectNameValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ProjectStepAsync,
                ProjectDisambiguationStepAsync,
                PersonStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        #region Waterfall steps

        // Step 1: Ensure we have a project name
        // Result is the project name from LUIS or from a user prompt
        private async Task<DialogTurnResult> ProjectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var requestDetails = stepContext.Options is RequestDetails
                    ? stepContext.Options as RequestDetails
                    : new RequestDetails();

            List<ConsultingProject> result = await ResolveProject(requestDetails.projectName);

            if (result == null || result.Count < 1)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt) + "projectName", new PromptOptions
                {
                    Prompt = MessageFactory.Text("Which project do you want to add to?"),
                    RetryPrompt = MessageFactory.Text("Sorry I didn't get that, what project was it?"),
                }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(result, cancellationToken);
            }
        }

        // Step 2: Project Disambiguation step
        // Result is one or more ConsultingProject objects
        private async Task<DialogTurnResult> ProjectDisambiguationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var requestDetails = stepContext.Options is RequestDetails
                    ? stepContext.Options as RequestDetails
                    : new RequestDetails();

            List<ConsultingProject> result = (List<ConsultingProject>)stepContext.Result;
            requestDetails.possibleProjects = result;

            if (result.Count > 1)
            {
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("Which of these projects did you mean?"),
                    Choices = result.Select(project => new Choice()
                    {
                        Value = $"{project.Client.Name} - {project.Name}",
                        Synonyms = new List<string>() { project.ProjectId.ToString() }
                    }).ToList()
                }, cancellationToken);
            }
            else
            {
                var project = result.First();
                var foundChoice = new FoundChoice()
                {
                    Value = $"{project.Client.Name} - {project.Name}",
                    Index = 0,
                };
                return await stepContext.NextAsync(foundChoice);
            }

        }

        // Step 3: Save the project info and ensure we have a person name
        // Result is the person name from LUIS or from a user prompt
        private async Task<DialogTurnResult> PersonStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var requestDetails = stepContext.Options is RequestDetails
                    ? stepContext.Options as RequestDetails
                    : new RequestDetails();

            var choice = (FoundChoice)stepContext.Result;
            var project = requestDetails.possibleProjects.ToArray()[choice.Index];
            requestDetails.projectName = project.Name;
            requestDetails.project = project;

            if (string.IsNullOrEmpty(requestDetails.personName))
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Ok, who did you want to add to the {requestDetails.projectName} project?") }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(requestDetails.personName, cancellationToken);
            }
        }

        // Step 4: Save the person name and confirm with the user 
        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var requestDetails = stepContext.Options is RequestDetails
                    ?
                stepContext.Options as RequestDetails
                    :
                new RequestDetails();

            requestDetails.personName = (string)stepContext.Result;

            var message = $"Please confirm, I'm adding: {requestDetails.personName} to the {requestDetails.projectName} project";

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text(message) }, cancellationToken);
        }

        // Step 5: Display the final outcome
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var requestDetails = stepContext.Options is RequestDetails
                        ?
                    stepContext.Options as RequestDetails
                        :
                    new RequestDetails();

                return await stepContext.EndDialogAsync(requestDetails, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
        #endregion

        #region Project name resolution
        private async Task<List<ConsultingProject>> ResolveProject(string keyword)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                ConsultingDataService dataService = new ConsultingDataService();
                var possibleResults = await dataService.GetProjects(keyword);
                if (possibleResults.Count > 0)
                {
                    // We have a single result, return it
                    return possibleResults;
                }
            }
            return null;
        }

        private async Task<bool> ProjectNameValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            ConsultingDataService dataService = new ConsultingDataService();
            var keyword = promptContext.Recognized.Value;
            var projects = await ResolveProject(keyword);
            return projects != null && projects.Count > 0;
        }
        #endregion
    }
}
