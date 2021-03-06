﻿namespace Barista.Extensions
{
    using System.IO;
    using System.Linq;
    using Barista.Helpers;
    using Jurassic;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// String extension class
    /// </summary>
    public static class StringExtensions
    {
        public static string EnsureStartsWith(this string str, string value)
        {
            if (str.StartsWith(value))
                return str;
            return str + value;
        }

        public static string EnsureEndsWith(this string str, string value)
        {
            if (str.EndsWith(value))
                return str;
            return str + value;
        }

        public static bool IsValidFileName(this string fileName)
        {
            return !fileName.IsNullOrWhiteSpace() &&
                    fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        public static bool IsValidPath(this string path)
        {
            return !path.IsNullOrWhiteSpace() &&
                    path.IndexOfAny(Path.GetInvalidPathChars()) < 0;
        }

        /// <summary>
        /// Gets a dynamic dictionary back from a Uri query string
        /// </summary>
        /// <param name="queryString">The query string to extract values from</param>
        /// <returns>A dynamic dictionary containing the query string values</returns>
        public static IDictionary<string, string> AsQueryDictionary(this string queryString)
        {
            var coll = HttpUtility.ParseQueryString(queryString);
            var ret = new Dictionary<string, string>();

            var found = 0;
            foreach (var key in coll.AllKeys.Where(key => key != null))
            {
                if (ret.ContainsKey(key))
                    continue;

                ret.Add(key, coll[key]);

                found++;

                if (found >= 1000) //FIXME: Why 1000?
                {
                    break;
                }
            }

            return ret;
        }

        public static string ReplaceFirstOccurence(this string inputstring, string searchText, string replacementText)
        {
            var index = inputstring.IndexOf(searchText, StringComparison.Ordinal);
            if (index == -1)
                return inputstring;

            return inputstring.Substring(0, index) + replacementText + inputstring.Substring(index + searchText.Length);
        }

        public static string ReplaceFirstOccurenceIgnoreCase(this string inputstring, string searchText, string replacementText)
        {
            var index = inputstring.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase);
            if (index == -1)
                return inputstring;

            return inputstring.Substring(0, index) + replacementText + inputstring.Substring(index + searchText.Length);
        }

        internal static List<Dictionary<string, string>> ConvertToDictionaryList(this YamlSequenceNode yamlNode)
        {
            return yamlNode.OfType<YamlMappingNode>().Select(item => ConvertToDictionary(item)).ToList();
        }

        internal static Dictionary<string, string> ConvertToDictionary(this YamlMappingNode yamlNode)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var key in yamlNode.Children.Keys)
            {
                dic[key.ToString()] = yamlNode.Children[key].ToString();
            }
            return dic;
        }

        public static void ThrowIfNull(this object obj, string exceptionMessage)
        {
            if (obj == null)
                throw new ArgumentNullException(exceptionMessage);
        }

        /// <summary>Indicates whether a specified string is null, empty, or consists only of white-space characters.</summary>
        /// <returns>true if the <paramref name="value" /> parameter is null or <see cref="F:System.String.Empty" />, or if <paramref name="value" /> consists exclusively of white-space characters. </returns>
        /// <param name="value">The string to test.</param>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (value == null)
                return true;

            var num = 0;
            while (num < value.Length)
            {
                if (char.IsWhiteSpace(value[num]))
                {
                    num++;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Concatenates the members of a constructed <see cref="T:System.Collections.Generic.IEnumerable`1" /> collection of type <see cref="T:System.String" />, using the specified separator between each member.</summary>
        /// <returns>A string that consists of the members of <paramref name="values" /> delimited by the <paramref name="separator" /> string. If <paramref name="values" /> has no members, the method returns <see cref="F:System.String.Empty" />.</returns>
        /// <param name="separator">The string to use as a separator.</param>
        /// <param name="values">A collection that contains the strings to concatenate.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="values" /> is null. </exception>
        public static string Join(this IEnumerable<string> values, string separator)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            if (separator == null)
                separator = string.Empty;

            var enumerator = values.GetEnumerator();
            string stringAndRelease;
            using (enumerator)
            {
                if (enumerator.MoveNext())
                {
                    var stringBuilder = new StringBuilder(16);
                    if (enumerator.Current != null)
                    {
                        stringBuilder.Append(enumerator.Current);
                    }
                    while (enumerator.MoveNext())
                    {
                        stringBuilder.Append(separator);
                        if (enumerator.Current == null)
                        {
                            continue;
                        }
                        stringBuilder.Append(enumerator.Current);
                    }
                    stringAndRelease = stringBuilder.ToString();
                }
                else
                {
                    stringAndRelease = string.Empty;
                }
            }
            return stringAndRelease;
        }

        /// <summary>
        /// Convert string to int32.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static int ToInt32(this string source)
        {
            return source.ToInt32(0);
        }

        /// <summary>
        /// Convert string to int32.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int ToInt32(this string source, int defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            int value;

            if (!int.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to long.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static long ToLong(this string source)
        {
            return source.ToLong(0);
        }

        /// <summary>
        /// Convert string to long.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static long ToLong(this string source, long defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            long value;

            if (!long.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to short.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static short ToShort(this string source)
        {
            return source.ToShort(0);
        }

        /// <summary>
        /// Convert string to short.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static short ToShort(this string source, short defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            short value;

            if (!short.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to decimal.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static decimal ToDecimal(this string source)
        {
            return source.ToDecimal(0);
        }

        /// <summary>
        /// Convert string to decimal.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static decimal ToDecimal(this string source, decimal defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            decimal value;

            if (!decimal.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to date time.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string source)
        {
            return source.ToDateTime(DateTime.MinValue);
        }

        /// <summary>
        /// Convert string to date time.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string source, DateTime defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            DateTime value;

            if (!DateTime.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Convert string to boolean.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static bool ToBoolean(this string source)
        {
            return source.ToBoolean(false);
        }

        /// <summary>
        /// Convert string tp boolean.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns></returns>
        public static bool ToBoolean(this string source, bool defaultValue)
        {
            if (string.IsNullOrEmpty(source))
                return defaultValue;

            bool value;

            if (!bool.TryParse(source, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>
        /// Truncates the string to a specified length and replace the truncated to a ...
        /// </summary>
        /// <param name="text">string that will be truncated</param>
        /// <param name="maxLength">total length of characters to maintain before the truncate happens</param>
        /// <returns>truncated string</returns>
        public static string Truncate(this string text, int maxLength)
        {
            // replaces the truncated string to a ...
            const string suffix = "...";
            string truncatedString = text;

            if (maxLength <= 0) return truncatedString;
            int strLength = maxLength - suffix.Length;

            if (strLength <= 0) return truncatedString;

            if (text == null || text.Length <= maxLength) return truncatedString;

            truncatedString = text.Substring(0, strLength);
            truncatedString = truncatedString.TrimEnd();
            truncatedString += suffix;
            return truncatedString;
        }

        /// <summary>Converts the string representation of a GUID to the equivalent <see cref="T:System.Guid" /> structure. </summary>
        /// <returns>true if the parse operation was successful; otherwise, false.</returns>
        /// <param name="input">The GUID to convert.</param>
        /// <param name="result">The structure that will contain the parsed value.</param>
        public static bool TryParseGuid(this string input, out Guid result)
        {
            //TODO: regex this badboy.
            try
            {
                result = new Guid(input);
                return true;
            }
            catch
            {
                result = default(Guid);
                return false;
            }
        }

        /// <summary>
        /// Tries to parse string to enum type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static bool TryParseEnum<T>(this string value, bool ignoreCase, out T enumValue)
            where T : struct
        {
            try
            {
                enumValue = (T)System.Enum.Parse(typeof(T), value, ignoreCase);
                return true;
            }
            catch
            {
                enumValue = default(T);
                return false;
            }
        }

        /// <summary>
        /// Tries to parse string to enum type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="defaultValue"></param>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static bool TryParseEnum<T>(this object value, bool ignoreCase, T defaultValue, out T enumValue)
            where T : struct
        {
            if (value == null || value == Undefined.Value || value == Null.Value)
            {
                enumValue = defaultValue;
                return true;
            }

            if (value is SByte)
            {
                var val = (SByte)value;
                enumValue = (T)System.Enum.ToObject(typeof(T), val);
                return true;
            }

            if (value is Int16)
            {
                var val = (Int16)value;
                enumValue = (T)System.Enum.ToObject(typeof(T), val);
                return true;
            }

            if (value is Int32)
            {
                var val = (Int32)value;
                enumValue = (T)System.Enum.ToObject(typeof(T), val);
                return true;
            }

            if (value is Int64)
            {
                var val = (Int64)value;
                enumValue = (T)System.Enum.ToObject(typeof(T), val);
                return true;
            }

            if (value is Byte)
            {
                var val = (Byte)value;
                enumValue = (T)System.Enum.ToObject(typeof(T), val);
                return true;
            }

            if (value is UInt16)
            {
                var val = (UInt16)value;
                enumValue = (T)System.Enum.ToObject(typeof(T), val);
                return true;
            }

            if (value is UInt32)
            {
                var val = (UInt32)value;
                enumValue = (T)System.Enum.ToObject(typeof(T), val);
                return true;
            }

            if (value is UInt64)
            {
                var val = (UInt64)value;
                enumValue = (T)System.Enum.ToObject(typeof(T), val);
                return true;
            }

            var stringValue = value.ToString();

            try
            {
                enumValue = (T)System.Enum.Parse(typeof(T), stringValue, ignoreCase);
                return true;
            }
            catch
            {
                enumValue = default(T);
                return false;
            }
        }

        /// <summary>
        /// Concatenates two urls.
        /// </summary>
        /// <param name="firstPart"></param>
        /// <param name="secondPart"></param>
        /// <returns></returns>
        public static string ConcatUrls(this string firstPart, string secondPart)
        {
            if (firstPart == null)
                return secondPart;

            if (secondPart == null)
                return firstPart;

            const string str = "/";
            if (!firstPart.EndsWith(str, StringComparison.OrdinalIgnoreCase))
            {
                return !secondPart.StartsWith(str, StringComparison.OrdinalIgnoreCase)
                  ? string.Concat(firstPart, str, secondPart)
                  : string.Concat(firstPart, secondPart);
            }

            if (secondPart.StartsWith(str, StringComparison.OrdinalIgnoreCase))
            {
                firstPart = firstPart.TrimEnd(str.ToCharArray());
            }
            return string.Concat(firstPart, secondPart);
        }
    }
}
