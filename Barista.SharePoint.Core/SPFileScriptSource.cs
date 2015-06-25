namespace Barista.SharePoint
{
    using Jurassic;
    using System;
    using System.IO;
    using Microsoft.SharePoint.Utilities;

    public class SPFileScriptSource : ScriptSource
    {
        private readonly string m_code;
        private readonly string m_codePath;

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
                m_code = SPHelper.ReplaceTokens(code);
            }
            else
            {
                throw new JavaScriptException(engine, "Error", "Could not locate the specified script file:  " + scriptUrl);
            }
        }

        public override TextReader GetReader()
        {
            return new StringReader(m_code);
        }

        public override string Path
        {
            get { return m_codePath; }
        }
    }
}
