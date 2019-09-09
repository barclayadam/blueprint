using System;

namespace Blueprint.Core.Tracing
{
    public class VersionInfo
    {
        public string AppName { get; set; }

        public string Version { get; set; }

        public string Commit { get; set; }

        public DateTime Date { get; set; }
    }
}