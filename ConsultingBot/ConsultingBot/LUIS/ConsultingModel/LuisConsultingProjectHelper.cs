using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ConsultingBot
{
    public static class LuisConsultingProjectHelper
    {
        public static async Task<ProjectIntentDetails> ExecuteQuery(IConfiguration configuration, ILogger logger, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var result = new ProjectIntentDetails();

            try
            {
                // Create the LUIS settings from configuration.
                var luisApplication = new LuisApplication(
                    configuration["LuisAppId"],
                    configuration["LuisAPIKey"],
                    "https://" + configuration["LuisAPIHostName"]
                );
                var recognizer = new LuisRecognizer(luisApplication);

                // The actual call to LUIS
                var recognizerResult = await recognizer.RecognizeAsync(turnContext, cancellationToken);
                var (intent, score) = recognizerResult.GetTopScoringIntent();

                // Get all the possible values for each entity
                var personNameValues = recognizerResult.GetEntity<string>("personName");
                var projectNameValues = recognizerResult.GetEntity<string>("projectName");
                var timeWorkedValues = recognizerResult.GetEntity<string>("timeWorked");
                var dateTimeValues = recognizerResult.GetEntity<string>("datetime");

                // Now based on the intent, fill in the result
                switch (intent)
                {
                    case "AddPersonToProject":
                        {
                            result.intent = ProjectIntent.AddToProject;
                            result.personName = personNameValues?.FirstOrDefault();
                            result.projectName = projectNameValues?.FirstOrDefault();
                            break;
                        }
                    case "BillToProject":
                        {
                            result.intent = ProjectIntent.BillToProject;
                            result.projectName = projectNameValues?.FirstOrDefault();
                            result.taskDurationMinutes = 0;

                            string timeUnitString = null;
                            for (int i=0; i<timeWorkedValues.Count; i++)
                            {
                                var timeUnitCount = 0;
                                if (int.TryParse(timeWorkedValues[i], out timeUnitCount))
                                {
                                    if (i < timeWorkedValues.Count)
                                    {
                                        if ((new[] { "hours", "hrs", "hr", "h" })
                                            .Contains(timeWorkedValues[i+1], StringComparer.OrdinalIgnoreCase))
                                        {
                                            result.taskDurationMinutes = timeUnitCount * 60;
                                            timeUnitString = timeWorkedValues[i + 1];
                                        }
                                        else if ((new[] { "minutes", "min", "mn", "m" })
                                            .Contains(timeWorkedValues[i + 1], StringComparer.OrdinalIgnoreCase))
                                        {
                                            result.taskDurationMinutes = timeUnitCount;
                                            timeUnitString = timeWorkedValues[i + 1];
                                        }
                                    }
                                }
                            }
                            foreach (string val in dateTimeValues)
                            {
                                if (val.IndexOf(timeUnitString) <= 0)
                                {
                                    result.deliveryDate = val;
                                }
                            }
                            break;
                        }
                    default:
                        {
                            result.intent = ProjectIntent.Unknown;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                logger.LogWarning($"LUIS Exception: {e.Message} Check your LUIS configuration.");
            }

            return result;
        }

        private static List<T> GetEntity<T>(this RecognizerResult luisResult, string entityKey, string valuePropertyName = "text")
        {
            // Parsing the dynamic JObjects returned by LUIS is never easy
            // Adapted from https://pauliom.com/2018/11/06/extracting-an-entity-from-luis-in-bot-framework/
            var result = new List<T>();

            if (luisResult != null)
            {
                //// var value = (luisResult.Entities["$instance"][entityKey][0]["text"] as JValue).Value;
                var data = luisResult.Entities as IDictionary<string, JToken>;

                if (data.TryGetValue("$instance", out JToken value))
                {
                    var entities = value as IDictionary<string, JToken>;
                    if (entities.TryGetValue(entityKey, out JToken targetEntity))
                    {
                        var entityArray = targetEntity as JArray;
                        if (entityArray.Count > 0)
                        {
                            for (int i=0; i<entityArray.Count;i++)
                            {
                                var values = entityArray[i] as IDictionary<string, JToken>;
                                if (values.TryGetValue(valuePropertyName, out JToken textValue))
                                {
                                    var text = textValue as JValue;
                                    if (text != null)
                                    {
                                        result.Add((T)text.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
