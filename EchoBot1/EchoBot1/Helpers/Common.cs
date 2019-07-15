using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBot1.Helpers
{
    public static class Common
    {
        public static readonly List<string> BugTypes =
            new List<string>() { "Security", "Crash", "Power", "Performance", "Usability", "Serious Bug", "Other" };
    }
}
