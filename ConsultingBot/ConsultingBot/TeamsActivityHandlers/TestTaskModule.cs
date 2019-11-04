using ConsultingBot.Cards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace ConsultingBot.TeamsActivityHandlers
{
    public class TestTaskModule
    {
        private TestCard testCard = new TestCard();

        // Called when a Task Module Action on a card is clicked and the task module needs to be rendered
        public async Task<TaskModuleResponse> HandleTaskModuleFetchAsync(ITurnContext turnContext, TaskModuleRequest query)
        {
            return new TaskModuleResponse
            {
                Task = this.TaskModuleResponseTask(query, false),
            };
        }

        // Called when a task module from a card is submitted
        public async Task<TaskModuleResponse> HandleTaskModuleSubmitAsync(ITurnContext turnContext, TaskModuleRequest query)
        {
            bool done = false;
            if (query.Data != null)
            {
                var data = JObject.FromObject(query.Data);
                done = (bool)data["done"];
            }

            return new TaskModuleResponse
            {
                Task = this.TaskModuleResponseTask(query, done),
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
