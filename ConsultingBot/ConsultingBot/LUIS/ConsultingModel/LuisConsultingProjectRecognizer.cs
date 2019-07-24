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
    public static class LuisConsultingProjectRecognizer
    {
        public static async Task<RequestDetails> ExecuteQuery(IConfiguration configuration, ILogger logger, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var result = new RequestDetails();

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

                // Get all the possible values for each entity from the Entities JObject
                // (GetEntityValueOptions is an extension method, see below)
                var personNameValues = recognizerResult.GetPossibleEntityValues<string>("personName");
                var projectNameValues = recognizerResult.GetPossibleEntityValues<string>("projectName");
                var timeWorkedValues = recognizerResult.GetPossibleEntityValues<string>("timeWorked");
                var dateTimeValues = recognizerResult.GetPossibleEntityValues<string>("datetime");

                // Now based on the intent, fill in the result as best we can
                switch (intent)
                {
                    case "AddPersonToProject":
                        {
                            result.intent = Intent.AddToProject;
                            result.personName = personNameValues?.FirstOrDefault();
                            result.projectName = projectNameValues?.FirstOrDefault();
                            break;
                        }
                    case "BillToProject":
                        {
                            result.intent = Intent.BillToProject;
                            result.projectName = projectNameValues?.FirstOrDefault();
                            string timeUnitsToken;
                            (result.workDuration, timeUnitsToken) =
                                TryExtractTimeWorked(result, timeWorkedValues);
                            result.workDate = TryExtractDeliveryDate(result, dateTimeValues, timeUnitsToken);
                            break;
                        }
                    default:
                        {
                            result.intent = Intent.Unknown;
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

        private static string TryExtractDeliveryDate(RequestDetails result, List<string> dateTimeValues, string timeUnitsToken)
        {
            foreach (string val in dateTimeValues)
            {
                // Often the task duration is mistaken as a date value, so if we see the 
                // same time units in there, skip that value
                if (!string.IsNullOrEmpty(timeUnitsToken) && val.IndexOf(timeUnitsToken) <= 0)
                {
                    return val;
                }
            }
            return null;
        }

        private static (int,string) TryExtractTimeWorked(RequestDetails result, List<string> timeWorkedValues)
        {
            var minutes = 0;
            string timeUnitString = null;
            for (int i = 0; i < timeWorkedValues.Count; i++)
            {
                var timeUnitCount = 0;
                if (int.TryParse(timeWorkedValues[i], out timeUnitCount))
                {
                    if (i < timeWorkedValues.Count)
                    {
                        if ((new[] { "hours", "hrs", "hr", "h" })
                            .Contains(timeWorkedValues[i + 1], StringComparer.OrdinalIgnoreCase))
                        {
                            minutes = timeUnitCount * 60;
                            timeUnitString = timeWorkedValues[i + 1];
                        }
                        else if ((new[] { "minutes", "min", "mn", "m" })
                            .Contains(timeWorkedValues[i + 1], StringComparer.OrdinalIgnoreCase))
                        {
                            minutes = timeUnitCount;
                            timeUnitString = timeWorkedValues[i + 1];
                        }
                    }
                }
            }

            return (minutes, timeUnitString);
        }

        private static List<T> GetPossibleEntityValues<T>(this RecognizerResult luisResult, string entityKey, string valuePropertyName = "text")
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
