namespace Barista.SharePoint
{
    using Jurassic;
    using System;
    using System.IO;
    using Microsoft.SharePoint.Utilities;
    using System.Collections.Generic;

    public class SPFileScriptSource : ScriptSource
    {
        private readonly string m_code;
        private readonly string m_codePath;
        private readonly Dictionary<string, string> m_flags;

        public SPFileScriptSource(ScriptEngine engine, string scriptUrl)
        {
            string filePath;
            string code;
            bool isHiveFile;
            if (Uri.IsWellFormedUriString(scriptUrl, UriKind.Relative))
            {
                scriptUrl = SPUtility.ConcatUrls(SPBaristaContext.Current.Web.Url, scriptUrl);
            }

            if (SPHelper.TryGetSPFileAsString(scriptUrl, out filePath, out code, out isHiveFile))
            {
                m_codePath = filePath;
                m_code = code;
                m_flags = new Dictionary<string, string>();
            }
            else
            {
                throw new JavaScriptException(engine, "Error", "Could not locate the specified script file:  " + scriptUrl);
            }
        }

        public override string Path
        {
            get { return m_codePath; }
        }

        public override IDictionary<string, string> Flags
        {
            get { return m_flags; }
        }

        public override TextReader GetReader()
        {
            return new StringReader(m_code);
        }
    }
}
