using System;
using System.IO;
using System.Linq;
using Awesome.Net.IgnoreParser.Extensions;
using DotNet.Globbing;

namespace Awesome.Net.IgnoreParser
{
    //https://git-scm.com/docs/gitignore
    /* https://stackoverflow.com/questions/17888695/difference-between-gitignore-rules-with-and-without-trailing-slash-like-dir-an
     * /dir   将匹配任何名称为 dir 的内容（文件、目录、链接）
     * /dir/  将只匹配一个名为 dir 的目录
     * /dir/* 将匹配 dir 目录下的任何内容，（但不匹配 dir 目录本身）
     */
    /// <summary>
    /// 可用于确定是否应忽略文件路径的规则。
    /// </summary>
    public class IgnoreRule
    {
        private readonly char[] _globWildcards = { '*', '[', '?' };
        private readonly StringComparison _stringComparison = StringComparison.Ordinal;
        private readonly Glob _glob;
        private readonly string _analyzedPattern;
        private PatternMeanings _meanings;

        /// <summary>
        /// 规则匹配选项
        /// </summary>
        public IgnoreOptions IgnoreOptions { get; }

        /// <summary>
        /// 原始 Glob OriginalPattern
        /// </summary>
        public string OriginalPattern { get; }

        /// <summary>
        /// 为否定形式
        /// </summary>
        public bool Negation { get; private set; }

        /// <summary>
        /// 模式的行号 (如果它是从文件加载的)
        /// </summary>
        public int? LineNumber { get; }

        /// <summary>
        /// .gitignore 所在目录作为当前根目录
        /// </summary>
        public string BasePath { get; }

        /// <summary>
        /// 初始化<see cref="IgnoreRule"></see>类的新实例。
        /// </summary>
        public IgnoreRule(string originalPattern, IgnoreOptions options = null, string basePath = "/", int? lineNumber = null)
        {
            if (originalPattern.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(originalPattern));
            }

            BasePath = basePath.IsNullOrWhiteSpace() ? "/" : basePath.NormalizedPath().EnsureEndsWith('/');

            IgnoreOptions = options ?? IgnoreOptions.Default;

            // 保存原始的 originalPattern
            OriginalPattern = originalPattern;

            LineNumber = lineNumber;

            var globOptions = GlobOptions.Default;
            if (!IgnoreOptions.CaseSensitive)
            {
                _stringComparison = StringComparison.OrdinalIgnoreCase;
                globOptions.Evaluation.CaseInsensitive = true;
            }

            _analyzedPattern = AnalyzePattern(originalPattern);
            _glob = Glob.Parse(_analyzedPattern, globOptions);
        }

        /// <summary>
        /// 检查文件路径是否与规则模式匹配
        /// </summary>
        public bool IsMatch(FileInfo file)
        {
            return IsMatch(file.FullName);
        }

        /// <summary>
        /// 检查文件夹路径是否与规则模式匹配
        /// </summary>
        public bool IsMatch(DirectoryInfo directory)
        {
            return IsMatch(directory.FullName);
        }

        /// <summary>
        /// 检查文件/夹路径是否与规则模式匹配
        /// </summary>
        public bool IsMatch(string fullPath)
        {
            if (fullPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(fullPath));
            }

            var path = fullPath.NormalizedPath()
                .RemovePreFix(_stringComparison, BasePath)
                .TrimEnd('/');

            if (path.IsNullOrEmpty())
            {
                return false;
            }

            // 如果 OriginalPattern 是以反斜杠(/)开头（PatternMeanings.StartsWithOfPath），
            // 意味着要匹配的路径必须跟第一个通配符之前的字符串匹配
            if (_meanings.HasFlag(PatternMeanings.StartsWithOfPath))
            {
                var firstWildcardIndex = _analyzedPattern.IndexOfAny(_globWildcards);
                var prePattern = firstWildcardIndex != -1
                    ? _analyzedPattern.Left(firstWildcardIndex)
                    : _analyzedPattern;

                if (!path.StartsWith(prePattern, _stringComparison))
                {
                    return false;
                }
            }

            /* 如果到目前为止，我们还不能通过简单的字符串匹配来确定结果， 那么只能其它方式解析 Glob 表达式了。这里使用的是：DotNet.Glob [https://github.com/dazinator/DotNet.Glob] */

            // 如果 _analyzedPattern 不再包含任何斜杠，意味着它可以匹配任何路径段
            // 如：'*.jpg' 可以匹配 'a.jpg', 'a/b.jpg', 'a/b/c.jpg'
            // 所以，path 中的每个斜杠前的字符串都应该尝试匹配一下
            if (!_analyzedPattern.Contains("/") && path.Contains("/"))
            {
                return path.Split('/').Any(segment => _glob.IsMatch(segment));
            }

            // 其它情况直接解析
            return _glob.IsMatch(path);
        }

        private string AnalyzePattern(string originalPattern)
        {
            var pattern = originalPattern;

            _meanings = PatternMeanings.ShellGlob;

            // originalPattern 以!开头
            if (pattern.StartsWith("!"))
            {
                Negation = true;
                _meanings |= PatternMeanings.Negation;
                pattern = pattern.TrimStart('!');
            }

            // originalPattern 以/开头
            if (pattern.StartsWith("/"))
            {
                _meanings |= PatternMeanings.StartsWithOfPath;
                pattern = pattern.TrimStart('/');
            }

            // originalPattern 以/结尾
            if (pattern.EndsWith("/"))
            {
                _meanings |= PatternMeanings.Directory;
                pattern = pattern.TrimEnd('/');
            }

            return pattern;
        }

        public override string ToString()
        {
            var lineNumber = LineNumber.HasValue
                ? $"#{LineNumber.Value}"
                : string.Empty;

            return $"[{OriginalPattern}] {lineNumber}";
        }
    }

    /// <summary>
    /// 表示 .gitignore 文件中每个 pattern 的含义
    /// </summary>
    [Flags]
    public enum PatternMeanings
    {
        /// <summary>
        /// pattern 为基本的 Shell Glob 形式，即不包含任何反斜杠(/)
        /// </summary>
        ShellGlob = 0,

        /// <summary>
        /// pattern 为否定形式，即(!)开头
        /// </summary>
        Negation = 1,

        /// <summary>
        /// pattern 为前路径匹配形式，即(/)开头
        /// </summary>
        StartsWithOfPath = 2,

        /// <summary>
        /// pattern 为目录匹配形式，即(/)结尾
        /// </summary>
        Directory = 4
    }
}
