using System;

namespace ConsultingBot
{
    public enum Intent
    {
        AddToProject,
        BillToProject,
        Unknown
    }

    public class RequestDetails
    {
        public Intent intent { get; set; }
        // Note that only some are used, depending on the intent
        public string projectName { get; set; }
        public string personName { get; set; }
        public int workDuration { get; set; }
        public string workDate { get; set; }
    }
}
