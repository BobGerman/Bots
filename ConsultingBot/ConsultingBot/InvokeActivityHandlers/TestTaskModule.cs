using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace ConsultingBot.Cards
{
    public class TestTaskModule
    {
        private TestCard testCard = new TestCard();

        public async Task<InvokeResponse> HandleInvokeAsync(ITurnContext turnContext)
        {
            ITeamsContext teamsContext = turnContext.TurnState.Get<ITeamsContext>();

            if (teamsContext.IsRequestTaskModuleFetch())
            {
                return await this.HandleTaskModuleFetchAsync(turnContext, teamsContext.GetTaskModuleRequestData()).ConfigureAwait(false);
            }

            if (teamsContext.IsRequestTaskModuleSubmit())
            {
                return await this.HandleTaskModuleSubmitAsync(turnContext, teamsContext.GetTaskModuleRequestData()).ConfigureAwait(false);
            }

            return await Task.FromResult<InvokeResponse>(null);
        }

        // Called when a Task Module Action on a card is clicked and the task module needs to be rendered
        private async Task<InvokeResponse> HandleTaskModuleFetchAsync(ITurnContext turnContext, TaskModuleRequest query)
        {
            return new InvokeResponse
            {
                Status = 200,
                Body = new TaskModuleResponse
                {
                    Task = this.TaskModuleResponseTask(query, false),
                },
            };
        }

        // Called when a task module from a card is submitted
        private async Task<InvokeResponse> HandleTaskModuleSubmitAsync(ITurnContext turnContext, TaskModuleRequest query)
        {
            bool done = false;
            if (query.Data != null)
            {
                var data = JObject.FromObject(query.Data);
                done = (bool)data["done"];
            }

            return new InvokeResponse
            {
                Status = 200,
                Body = new TaskModuleResponse
                {
                    Task = this.TaskModuleResponseTask(query, done),
                },
            };
        }

        // Called when fetching the contents of the task module
        public TaskModuleResponseBase TaskModuleResponseTask(TaskModuleRequest query, bool done)
        {
            if (done)
            {
                return new TaskModuleMessageResponse()
                {
                    Type = "message",
                    Value = "Thanks for your inputs!",
                };
            }
            else
            {
                string textValue = null;
                if (query.Data != null)
                {
                    var data = JObject.FromObject(query.Data);
                    textValue = data["userText"]?.ToString();
                }

                return new TaskModuleContinueResponse()
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Title = "More Page",
                        Card = testCard.GetCard(query, textValue),
                    },
                };
            }
        }

    }
}
