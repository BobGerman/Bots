using ConsultingData.Models;
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
        public Intent intent { get; set; } = Intent.Unknown;

        // LUIS entity values
        public string projectName { get; set; } = null;
        public string personName { get; set; } = null;
        public double workHours { get; set; } = 0.0;
        public string workDate { get; set; } = null;

        // Possible projects matching project name
        public List<ConsultingProject> possibleProjects { get; set; } = new List<ConsultingProject>();

        // Resolved value validated with data
        public ConsultingProject project { get; set; } = null;
    }
}
