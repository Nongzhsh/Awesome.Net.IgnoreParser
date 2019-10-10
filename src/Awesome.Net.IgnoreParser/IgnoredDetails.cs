using System;

namespace Awesome.Net.IgnoreParser
{
    /// <summary>
    /// 匹配详情
    /// </summary>
    public class IgnoredDetails
    {
        /// <summary>
        /// 匹配的路径
        /// </summary>
        public string MatchedPath { get; }

        /// <summary>
        /// 所应用的忽略文件
        /// </summary>
        public IgnoreFile IgnoreFile { get; }

        /// <summary>
        /// 所应用的规则
        /// </summary>
        public IgnoreRule Rule { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="matchedPath"></param>
        /// <param name="ignoreFile"></param>
        /// <param name="rule"></param>
        public IgnoredDetails(string matchedPath, IgnoreFile ignoreFile, IgnoreRule rule)
        {
            MatchedPath = matchedPath;
            IgnoreFile = ignoreFile;
            Rule = rule;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var newLine = Environment.NewLine;
            var state = Rule.Negation ? "Included" : "Ignored";
            var content = $"{newLine}[{state}] {MatchedPath}{newLine}{Rule} ({IgnoreFile})";

            return content;
        }
    }
}
