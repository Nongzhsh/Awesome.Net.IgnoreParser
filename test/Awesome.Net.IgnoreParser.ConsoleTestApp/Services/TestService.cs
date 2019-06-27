using System;
using System.IO;
using System.Linq;
using Awesome.Net.IgnoreParser.ConsoleTestApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Awesome.Net.IgnoreParser.ConsoleTestApp.Services
{
    public class TestService : ITestService
    {
        private readonly ILogger<TestService> _logger;
        private readonly IOptions<AppSettings> _options;

        public TestService(ILogger<TestService> logger,
            IOptions<AppSettings> options)
        {
            _logger = logger;
            _options = options;
        }

        public void Run()
        {
            var workingPath = Directory.GetCurrentDirectory();
            var codePath = workingPath.Remove(workingPath.LastIndexOf("\\test\\"));
            var source = new DirectoryInfo(codePath);

            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var target = new DirectoryInfo($"{desktop}\\Temp");
            
            var parser = IgnoreParser.Default;
            parser.Options = _options.Value.IgnoreOptions;
            parser.AddIgnoreFiles(codePath);

            CopyWithIgnores(source, target, parser);
        }

        private void CopyWithIgnores(DirectoryInfo source, DirectoryInfo target, IgnoreParser parser)
        {
            Directory.CreateDirectory(target.FullName);

            foreach(var file in source.GetFiles().Where(f => !parser.IsIgnore(f, LogIgnore)))
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }

            foreach(var subDir in source.GetDirectories().Where(d => !parser.IsIgnore(d, LogIgnore)))
            {
                CopyWithIgnores(subDir, target.CreateSubdirectory(subDir.Name), parser);
            }
        }

        private void LogIgnore(IgnoredDetails log)
        {
            _logger.LogInformation(log.ToString());
        }
    }
}