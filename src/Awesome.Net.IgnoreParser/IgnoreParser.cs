using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Awesome.Net.IgnoreParser.Extensions;

namespace Awesome.Net.IgnoreParser
{
    /// <summary>
    /// 忽略解析器
    /// </summary>
    public class IgnoreParser
    {
        /// <summary>
        /// 默认解析器
        /// </summary>
        public static IgnoreParser Default = new IgnoreParser("Default");

        /// <summary>
        /// 规则匹配选项
        /// </summary>
        public IgnoreOptions Options { get; set; }

        /// <summary>
        /// 解析器名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 规则文件列表，即 <see cref="IgnoreFile"/> 集合
        /// </summary>
        public List<IgnoreFile> IgnoreFiles { get; } = new List<IgnoreFile>();

        public IgnoreParser(string name, IgnoreOptions options = null)
        {
            if (name.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Options = options;
        }

        /// <summary>
        /// 添加自定义忽略规则
        /// </summary>
        public void AddCustomIgnoreRules(string basePath, IEnumerable<string> rules)
        {
            var ruleList = rules.Distinct().ToList();
            if (ruleList.Any())
            {
                var filePath = Path.Combine(basePath, $".ignore_{Name.ToSafeFileName()}");
                var ignoreFile = new IgnoreFile(ruleList, filePath: filePath, Options);
                IgnoreFiles.Add(ignoreFile);
            }
        }

        /// <summary>
        /// 从文件夹中添加所有的 <see cref="IgnoreFile"/>
        /// </summary>
        /// <param name="basePath">指定的文件夹路径</param>
        public void AddIgnoreFiles(string basePath)
        {
            if (basePath.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(basePath));
            }
            var baseFolder = new DirectoryInfo(basePath);

            var ignoreFiles = baseFolder.GetFiles(".gitignore", SearchOption.AllDirectories);

            AddIgnoreFiles(ignoreFiles);
        }

        /// <summary>
        /// 添加 <see cref="IgnoreFile"/>
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void AddIgnoreFile(string filePath)
        {
            var ignoreFile = new IgnoreFile(filePath, Options);
            IgnoreFiles.Add(ignoreFile);
        }

        /// <summary>
        /// 添加 <see cref="IgnoreFile"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileStream">文件路径</param>
        public void AddIgnoreFile(string filePath, Stream fileStream)
        {
            using (var stream = new StreamReader(fileStream))
            {
                var ruleList = new List<string>();
                string rule;
                while (null != (rule = stream.ReadLine()))
                {
                    ruleList.Add(rule);
                }
                var ignoreFile = new IgnoreFile(ruleList, filePath: filePath, Options);
                IgnoreFiles.Add(ignoreFile);
            }
        }

        /// <summary>
        /// 添加多个 <see cref="IgnoreFile"/>
        /// </summary>
        /// <param name="files">文件列表</param>
        public void AddIgnoreFiles(IEnumerable<FileInfo> files)
        {
            AddIgnoreFiles(files.Select(x => x.FullName));
        }

        /// <summary>
        /// 添加多个 <see cref="IgnoreFile"/>
        /// </summary>
        /// <param name="files">文件列表</param>
        public void AddIgnoreFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                AddIgnoreFile(file);
            }
        }

        /// <summary>
        /// 判断是否忽略
        /// </summary>
        public bool IsIgnore(FileInfo file, Action<IgnoredDetails> ignoredAction = null)
        {
            return IsIgnore(file.FullName, ignoredAction);
        }

        /// <summary>
        /// 判断是否忽略
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="ignoredAction"></param>
        /// <returns></returns>
        public bool IsIgnore(DirectoryInfo directory, Action<IgnoredDetails> ignoredAction = null)
        {
            return IsIgnore(directory.FullName, ignoredAction);
        }

        /// <summary>
        /// 判断是否忽略文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="ignoredAction">匹配（IsIgnore）之后的 Action</param>
        public bool IsIgnore(string path, Action<IgnoredDetails> ignoredAction = null)
        {
            // 根据 .gitignore 文件所在目录去匹配
            var basePath = Path.GetDirectoryName(path).NormalizedPath();
            var ignoreFiles = IgnoreFiles
                .Where(x => basePath.StartsWith(x.BasePath))
                .OrderByDescending(x => x.BasePath)
                .ToList();

            foreach (var ignoreFile in ignoreFiles)
            {
                var isIgnore = ignoreFile.IsIgnore(path, ignoredAction);
                if (isIgnore)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}