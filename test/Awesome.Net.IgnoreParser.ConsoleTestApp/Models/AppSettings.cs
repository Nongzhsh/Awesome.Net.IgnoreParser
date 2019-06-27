using Microsoft.Extensions.Logging;

namespace Awesome.Net.IgnoreParser.ConsoleTestApp.Models
{
    public class AppSettings
    {
        public string ConsoleTitle { get; set; } = "ConsoleTestApp";

        public LogLevel MinLevel { get; set; } = LogLevel.Trace;

        public IgnoreOptions IgnoreOptions { get; set; } = IgnoreOptions.Default;
    }
}