using ConsultingData.Models;
using System;
using System.Collections.Generic;

namespace ConsultingBot
{
    public enum Intent
    {
        Unknown,
        AddToProject,
        BillToProject,
    }

    public class RequestDetails
    {
        // Unresolved values from LUIS and/or directly from prompts
        public Intent intent { get; set; } = Intent.Unknown;
        // Note that only some are used, depending on the intent
        public string projectName { get; set; } = null;
        public string personName { get; set; } = null;
        public double workHours { get; set; } = 0.0;
        public string workDate { get; set; } = null;

        // Intermediate state
        public List<ConsultingProject> possibleProjects { get; set; } = new List<ConsultingProject>();
        // Resolved values validated with data
        public ConsultingProject project { get; set; } = null;
    }
}
