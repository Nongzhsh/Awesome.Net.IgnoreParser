using System;
using System.IO;
using System.Linq;

namespace Awesome.Net.IgnoreParser.Extensions
{
    public static class StringExtensions
    {
        public static string Left(this string str, int len)
        {
            if(str.Length < len)
            {
                throw new ArgumentException($"{nameof(len)} 参数不能大于给定字符串的长度!");
            }

            return str.Substring(0, len);
        }

        public static string Right(this string str, int len)
        {
            if(str.Length < len)
            {
                throw new ArgumentException($"{nameof(len)} 参数不能大于给定字符串的长度!");
            }

            return str.Substring(str.Length - len, len);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string RemovePreFix(this string str, StringComparison comparisonType, params string[] preFixes)
        {
            if(str.IsNullOrEmpty())
            {
                return null;
            }

            if(preFixes == null || preFixes.Length <= 0)
            {
                return str;
            }

            foreach(var preFix in preFixes)
            {
                if(str.StartsWith(preFix, comparisonType))
                {
                    return str.Right(str.Length - preFix.Length);
                }
            }

            return str;
        }

        public static string EnsureEndsWith(this string str, char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if(str.EndsWith(c.ToString(), comparisonType))
            {
                return str;
            }

            return str + c;
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string NormalizedPath(this string path)
        {
            //path = path.Replace(":", string.Empty);
            return path.IsNullOrWhiteSpace() ? string.Empty : path.Replace('\\', '/').Trim();
        }

        public static string ToSafeFileName(this string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}