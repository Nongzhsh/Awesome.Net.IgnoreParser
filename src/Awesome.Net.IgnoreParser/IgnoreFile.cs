using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Awesome.Net.IgnoreParser.Extensions;

namespace Awesome.Net.IgnoreParser
{
    /// <summary>
    /// 表示一个 .gitignore 文件（忽略清单）
    /// </summary>
    public class IgnoreFile
    {
        private readonly List<IgnoreRule> _rules = new List<IgnoreRule>();

        //靠后的规则优先级比较高，所以要 Reverse
        private IEnumerable<IgnoreRule> UniqueRules => _rules.DistinctBy(x => x.OriginalPattern).Reverse().ToList();

        public ReadOnlyCollection<IgnoreRule> Rules => _rules.AsReadOnly();

        private string _filePath;
        public string FilePath => _filePath.IsNullOrWhiteSpace()
            ? string.Empty
            : _filePath.NormalizedPath();

        public string BasePath => Path.GetDirectoryName(FilePath).NormalizedPath();

        private IgnoreFile()
        {
        }

        public IgnoreFile(IEnumerable<string> rules, string filePath = null, IgnoreOptions options = null)
        {
            _filePath = filePath;
            AddRules(rules, options);
        }

        public IgnoreFile(string filePath, IgnoreOptions options = null)
        {
            _filePath = filePath;
            AddRules(filePath, options);
        }

        /// <summary>
        /// 将规则添加到忽略列表中。
        /// </summary>
        public void AddRule(string rule, IgnoreOptions options = null)
        {
            AddRules(new[] { rule }, options);
        }

        public void AddRules(string ignoreFilePath, IgnoreOptions options = null)
        {
            AddRules(File.ReadAllLines(ignoreFilePath), options);
        }

        public void AddRules(IEnumerable<string> rules, IgnoreOptions options = null)
        {
            var ruleLines = rules.Select((pattern, index) => new
            {
                Pattern = pattern,
                LineNumber = index + 1
            }).Where(line => line.Pattern.Length > 0 && !line.Pattern.StartsWith("#"));

            var ruleList = ruleLines.Select(line => new IgnoreRule(line.Pattern, options, BasePath, line.LineNumber));

            ruleList = ruleList.Distinct();
            _rules.AddRange(ruleList);
        }

        public bool IsIgnore(FileInfo file, Action<IgnoredDetails> ignoredAction = null)
        {
            return IsIgnore(file.FullName, ignoredAction);
        }

        public bool IsIgnore(DirectoryInfo directory, Action<IgnoredDetails> ignoredAction = null)
        {
            return IsIgnore(directory.FullName, ignoredAction);
        }

        public bool IsIgnore(string path, Action<IgnoredDetails> ignoredAction = null)
        {
            path = path.NormalizedPath();

            // 路径不以 .gitigore 文件所在路径时，则表示不能应用于当前的忽略列表，返回 false
            if(!path.StartsWith(BasePath))
            {
                return false;
            }

            return IsPathIgnore(path, ignoredAction);
        }

        public IgnoreFile Clone(string filePath = null)
        {
            var clone = new IgnoreFile()
            {
                _filePath = filePath
            };

            _rules.ForEach(r => clone.AddRule(r.OriginalPattern, r.IgnoreOptions));
            return clone;
        }

        public void RemoveRule(string rule)
        {
            _rules.RemoveAll(r => r.OriginalPattern == rule.Trim());
        }

        public override string ToString()
        {
            return FilePath ?? base.ToString();
        }

        private bool IsPathIgnore(string path, Action<IgnoredDetails> ignoredAction = null)
        {
            // https://github.com/henon/GitSharp/blob/master/GitSharp/IgnoreRules.cs
            foreach(var rule in UniqueRules)
            {
                if(rule.IsMatch(path))
                {
                    var isIgnore = !rule.Negation;
                    var details = new IgnoredDetails(path, this, rule);
                    ignoredAction?.Invoke(details);
                    return isIgnore;
                }
            }

            return false;
        }
    }
}
