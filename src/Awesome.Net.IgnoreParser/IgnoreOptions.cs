namespace Awesome.Net.IgnoreParser
{
    /// <summary>
    /// 规则匹配选项
    /// </summary>
    public class IgnoreOptions
    {
        public static IgnoreOptions Default = new IgnoreOptions();

        /// <summary>
        /// 区分大小写
        /// </summary>
        public bool CaseSensitive { get; set; } = true;
    }
}
