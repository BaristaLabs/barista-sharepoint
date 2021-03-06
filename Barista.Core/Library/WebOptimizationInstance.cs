﻿namespace Barista.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using EdgeJs;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;

    [Serializable]
    public class WebOptimizationInstance : ObjectInstance
    {
        private static readonly ConcurrentDictionary<string, Tuple<DateTime, string>> FileModifiedDates = new ConcurrentDictionary<string, Tuple<DateTime, string>>();

        public WebOptimizationInstance(ScriptEngine engine)
            : base(engine)
        {
            PopulateFunctions();
        }

        public Func<string> FileKeyPrefix
        {
            get;
            set;
        }

        public Func<string, string> GetAbsoluteUrlFromPath
        {
            get;
            set;
        }

        public Func<string, DateTime> GetLastModifiedDate
        {
            get;
            set;
        }

        public Func<string, string> ReadAllText
        {
            get;
            set;
        }

        [JSFunction(Name = "clearCache")]
        [JSDoc("Clears all cached files.")]
        public void ClearCache()
        {
            FileModifiedDates.Clear();
        }

        [JSFunction(Name = "hasBundleChangedSince")]
        [JSDoc("Using a xml bundle definition, determines if the contents of the bundle have changed since the specfied date.")]
        public bool HasBundleChangedSince(string bundleDefinitionXml, object date)
        {
            if (date == Undefined.Value || date == Null.Value || date == null)
                throw new JavaScriptException(Engine, "Error", "A date must be supplied as the second argument.");

            DateTime dateToCompare;
            if (date is DateTime)
                dateToCompare = (DateTime)date;
            else if (date is DateInstance)
                dateToCompare = (date as DateInstance).Value;
            else
                dateToCompare = (new DateInstance(Engine.Object.InstancePrototype, TypeConverter.ToString(date))).Value;

            var doc = new XmlDocument();
            doc.LoadXml(bundleDefinitionXml);

            var files = ParseBundleDefinition(doc);

            if (files == null || files.Count == 0)
                return false;

            UpdateFileCache(files);

            foreach (var file in files.Keys)
            {
                if (!FileModifiedDates.ContainsKey(FileKeyPrefix() + file))
                    continue;

                var contents = FileModifiedDates[FileKeyPrefix() + file];
                if (contents.Item1.ToLocalTime() > dateToCompare.ToLocalTime())
                    return true;
            }

            return false;
        }

        [JSFunction(Name = "bundle")]
        [JSDoc("Using a xml bundle definition, combines the specified files and returns the bundle as an object.")]
        public object Bundle(string bundleDefinitionXml, string fileName, object update, object minify)
        {
            if (String.IsNullOrEmpty(fileName))
                fileName = "bundle.txt";

            var bUpdate = JurassicHelper.GetTypedArgumentValue(Engine, update, true);
            var bMinify = JurassicHelper.GetTypedArgumentValue(Engine, minify, false);

            var doc = new XmlDocument();

            doc.LoadXml(bundleDefinitionXml);
            var bundleText = GenerateBundleFromBundleDefinition(fileName, doc, bUpdate, bMinify);

            var bytes = new Base64EncodedByteArrayInstance(Engine.Object.InstancePrototype, Encoding.UTF8.GetBytes(bundleText))
            {
                FileName = fileName,
                MimeType = StringHelper.GetMimeTypeFromFileName(fileName),
            };

            var result = Engine.Object.Construct();
            result.SetPropertyValue("lastModified", JurassicHelper.ToDateInstance(Engine, FileModifiedDates.Values.Max(v => v.Item1)), false);
            result.SetPropertyValue("data", bytes, false);
            return result;
        }

        [JSFunction(Name = "gzip")]
        public Base64EncodedByteArrayInstance GZip(object obj, object fileName, object mimeType)
        {
            byte[] data;
            if (obj is Base64EncodedByteArrayInstance)
            {
                var b = obj as Base64EncodedByteArrayInstance;
                if (fileName == Undefined.Value || fileName == Null.Value || fileName == null)
                    fileName = b.FileName;


                if (mimeType == Undefined.Value || mimeType == Null.Value || mimeType == null)
                    mimeType = b.MimeType;

                data = b.Data;
            }
            else
            {
                data = Encoding.UTF8.GetBytes(TypeConverter.ToString(obj));
            }

            Base64EncodedByteArrayInstance result;
            using (var inStream = new MemoryStream(data))
            {
                using (var outStream = new MemoryStream())
                {
                    using (var compress = new GZipStream(outStream, CompressionMode.Compress))
                    {
                        // Copy the source file into the compression stream.
                        byte[] buffer = new byte[4096];
                        int numRead;
                        while ((numRead = inStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            compress.Write(buffer, 0, numRead);
                        }
                    }
                    result = new Base64EncodedByteArrayInstance(Engine.Object.InstancePrototype, outStream.ToArray());

                    if (fileName != Undefined.Value && fileName != Null.Value && fileName != null)
                        result.FileName = TypeConverter.ToString(fileName);

                    if (mimeType != Undefined.Value && mimeType != Null.Value && mimeType != null)
                        result.MimeType = TypeConverter.ToString(mimeType);
                }
            }

            return result;
        }

        private static async Task<object> MinifyCssAsync(string css)
        {
            var cssoFunc = Edge.Func(@"
var csso = require(""csso"");
return function(data, callback) {
    var result = csso.minify(data, {fromString: true});
    callback(null, result);
};");

            var res = await cssoFunc(css);
            return res;
        }

        [JSFunction(Name = "minifyCss")]
        [JSDoc("Returns a minified representation of the css string passed as the first argument.")]
        public string MinifyCss(string css)
        {
            var cssTask = Task.Run<object>(() => MinifyCssAsync(css));
            cssTask.Wait();

            if (cssTask.Result == null)
                return String.Empty;

            return cssTask.Result.ToString();
        }

        private static async Task<object> MinifyJsAsync(string javascript)
        {
            var uglifyFunc = Edge.Func(@"
var UglifyJS = require(""uglify-js"");
return function(data, callback) {
    var result = UglifyJS.minify(data, {fromString: true});
    callback(null, result);
};");

            var res = await uglifyFunc(javascript);
            return res;
        }

        [JSFunction(Name = "minifyJs")]
        [JSDoc("Returns a minified representation of the javascript string passed as the first argument.")]
        public string MinifyJs(string javascript)
        {
            var uglifyTask = Task.Run<object>(() => MinifyJsAsync(javascript));
            uglifyTask.Wait();

            if (uglifyTask.Result == null)
                return String.Empty;

            dynamic result = uglifyTask.Result;
            return result.code.ToString();
        }

        [JSFunction(Name = "replaceRelativeUrlsWithAbsoluteInCss")]
        [JSDoc("Returns the specified CSS file with the included url values to be absolute urls.")]
        public string ReplaceRelativeUrlsWithAbsoluteInCss(string css, string cssFilePath)
        {
            var pattern = new Regex(@"url\s*\(\s*([""']?)([^:?]+?)(?<frag>#.+?)?(?<qs>\?.+?)?\1\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var matches = pattern.Matches(css);

            // Ignore the content if no match 
            if (matches.Count <= 0)
                return css;

            foreach (Match match in matches)
            {
                // this is a path that is relative to the CSS file
                var relativeToCss = match.Groups[2].Value;
                // combine the relative path to the cssAbsolute
                var absoluteToUrl = cssFilePath.TrimEnd('/') + "/" + relativeToCss;

                // make this server absolute
                var absUrl = GetAbsoluteUrlFromPath(absoluteToUrl);

                var quote = match.Groups[1].Value;
                var fragment = match.Groups["frag"].Value;
                var queryString = match.Groups["qs"].Value;
                var replace = String.Format("url({0}{1}{2}{3}{0})", quote, absUrl, fragment, queryString);
                css = css.Replace(match.Groups[0].Value, replace);
            }

            return css;
        }

        private IDictionary<string, string> ParseBundleDefinition(XmlNode doc)
        {
            var result = new Dictionary<string, string>();

            var bundleNode = doc.SelectSingleNode("//bundle");

            if (bundleNode == null || bundleNode.Attributes == null)
                return result;

            XmlNode outputAttr = bundleNode.Attributes["output"];

            if (outputAttr != null && (outputAttr.InnerText.Contains("/") || outputAttr.InnerText.Contains("\\")))
                throw new JavaScriptException(Engine, "Error", "The 'output' attribute is for file names only - not paths");

            var nodes = doc.SelectNodes("//file");

            if (nodes == null)
                return result;

            foreach (XmlNode node in nodes)
            {
                if (!result.ContainsKey(node.InnerText))
                    result.Add(node.InnerText, node.InnerText);
            }

            return result;
        }

        private void UpdateFileCache(IDictionary<string, string> files)
        {
            foreach (var file in files.Keys)
            {
                Tuple<DateTime, string> contents;
                if (FileModifiedDates.ContainsKey(FileKeyPrefix() + file))
                {
                    var fileContents = FileModifiedDates[FileKeyPrefix() + file];
                    var lastModified = GetLastModifiedDate(file);
                    if (lastModified > fileContents.Item1)
                    {
                        var text = ReadAllText(file);
                        contents = new Tuple<DateTime, string>(lastModified, text);
                        FileModifiedDates[FileKeyPrefix() + file] = contents;
                    }
                }
                else
                {
                    var lastModified = GetLastModifiedDate(file);
                    var text = ReadAllText(file);
                    contents = new Tuple<DateTime, string>(lastModified, text);
                    FileModifiedDates.TryAdd(FileKeyPrefix() + file, contents);
                }
            }
        }

        private string GenerateBundleFromBundleDefinition(string filePath, XmlDocument doc, bool update, bool minify)
        {
            var files = ParseBundleDefinition(doc);

            if (files == null || files.Count == 0)
                return String.Empty;

            if (update)
                UpdateFileCache(files);

            var extension = Path.GetExtension(filePath);
            if (String.IsNullOrEmpty(extension))
                extension = "txt";

            var sb = new StringBuilder();

            foreach (var file in files.Keys)
            {
                if (FileModifiedDates.ContainsKey(FileKeyPrefix() + file))
                {
                    var contents = FileModifiedDates[FileKeyPrefix() + file];

                    if (extension.Equals(".js", StringComparison.OrdinalIgnoreCase))
                        sb.AppendLine("///#source 1 1 " + FileKeyPrefix() + files[file]);
                    else if (extension.Equals(".css", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine("/* source 1 1 " + FileKeyPrefix() + files[file] + "*/");

                        //make paths absolute in css files.
                        var relCssPath = file.Substring(0, file.LastIndexOf('/'));
                        try
                        {
                            contents = new Tuple<DateTime, string>(contents.Item1,
                              ReplaceRelativeUrlsWithAbsoluteInCss(contents.Item2, relCssPath));
                        }
                        catch (Exception ex)
                        {
                            throw new JavaScriptException(Engine, "Error", "Error processing " + file + " " + ex.Message);
                        }
                    }

                    var source = contents.Item2;
                    if (minify)
                    {
                        try
                        {
                            if (extension.Equals(".js", StringComparison.OrdinalIgnoreCase))
                                source = MinifyJs(source);
                            else if (extension.Equals(".css", StringComparison.OrdinalIgnoreCase))
                                source = MinifyCss(source);
                        }
                        catch (Exception ex)
                        {
                            throw new JavaScriptException(Engine, "Error", "Error occurred while minifying file " + file + " " + ex.Message);
                        }
                    }
                    sb.AppendLine(source);
                }
            }

            return sb.ToString();
        }
    }
}
