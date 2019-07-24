using System;

namespace ConsultingBot
{
    public enum ProjectIntent
    {
        AddToProject,
        BillToProject,
        Unknown
    }

    public class ProjectIntentDetails
    {
        public ProjectIntent intent { get; set; }
        // Note that only some are used, depending on the intent
        public string projectName { get; set; }
        public string personName { get; set; }
        public int taskDurationMinutes { get; set; }
        public string deliveryDate { get; set; }
    }
}
