using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Newtonsoft.Json.Linq;

namespace ConsultingBot
{
    public static class LuisConsultingProjectRecognizer
    {
        public static async Task<ConsultingRequestDetails> ExecuteQuery(IConfiguration configuration, ILogger logger, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var result = new ConsultingRequestDetails();

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
                            (result.workHours, timeUnitsToken) =
                                TryExtractTimeWorked(timeWorkedValues);
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

        private static string TryExtractDeliveryDate(ConsultingRequestDetails result, List<string> dateTimeValues, string timeUnitsToken)
        {
            string timex = null;

            foreach (string val in dateTimeValues)
            {
                // Often the task duration is mistaken as a date value, so if we see the 
                // same time units in there, skip that value
                if (!string.IsNullOrEmpty(timeUnitsToken) && val.IndexOf(timeUnitsToken) <= 0)
                {
                    // OK looks like we have the right string, use the DateTime Recognizer to resolve it
                    var culture = Culture.English;
                    var r = DateTimeRecognizer.RecognizeDateTime(val, culture);
                    if (r.Count > 0 && r.First().TypeName.StartsWith("datetimeV2"))
                    {
                        var first = r.First();
                        var resolutionValues = (IList<Dictionary<string, string>>)first.Resolution["values"];
                        timex = resolutionValues[0]["timex"];
                    }
                }
            }
            return timex;
        }

        private static (double,string) TryExtractTimeWorked(List<string> timeWorkedValues)
        {
            var result = 0.0;
            string timeUnitString = null;
            for (int i = 0; i < timeWorkedValues.Count; i++)
            {
                var hours = 0.0;
                if (double.TryParse(timeWorkedValues[i], out hours))
                {
                    if (i < timeWorkedValues.Count)
                    {
                        if ((new[] { "hours", "hrs", "hr", "h" })
                            .Contains(timeWorkedValues[i + 1], StringComparer.OrdinalIgnoreCase))
                        {
                            result = hours;
                            timeUnitString = timeWorkedValues[i + 1];
                        }
                        else if ((new[] { "minutes", "min", "mn", "m" })
                            .Contains(timeWorkedValues[i + 1], StringComparer.OrdinalIgnoreCase))
                        {
                            result = hours / 60.0;
                            timeUnitString = timeWorkedValues[i + 1];
                        }
                    }
                }
            }

            return (result, timeUnitString);
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
