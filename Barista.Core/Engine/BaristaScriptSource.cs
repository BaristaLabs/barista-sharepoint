namespace Barista.Engine
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Represents an in-memory string-backed script source.
    /// </summary>
    public class BaristaScriptSource : IScriptSource
    {
        private readonly string m_code;
        private readonly string m_codePath;
        private readonly Dictionary<string, string> m_flags;

        public BaristaScriptSource(string code, string codePath)
        {
            m_code = code;
            m_codePath = codePath;
            m_flags = new Dictionary<string, string>();
        }

        public string Path
        {
            get { return m_codePath; }
        }

        public IDictionary<string, string> Flags
        {
            get { return m_flags; }
        }

        public TextReader GetReader()
        {
            return new StringReader(m_code);
        }
    }
}
