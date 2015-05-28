namespace Barista.Engine
{
    using System.IO;

    /// <summary>
    /// Represents an in-memory string-backed script source.
    /// </summary>
    public class BaristaScriptSource : IScriptSource
    {
        private readonly string m_code;
        private readonly string m_codePath;

        public BaristaScriptSource(string code, string codePath)
        {
            m_code = code;
            m_codePath = codePath;
        }

        public string Path
        {
            get { return m_codePath; }
        }

        public TextReader GetReader()
        {
            return new StringReader(m_code);
        }
    }
}
