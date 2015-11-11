namespace Barista.Jurassic
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Represents a string containing script code.
    /// </summary>
    public class StringScriptSource : ScriptSource
    {
        private readonly string m_code;
        private readonly string m_path;
        private readonly Dictionary<string, string> m_flags;

        /// <summary>
        /// Creates a new StringScriptSource instance.
        /// </summary>
        /// <param name="code"> The script code. </param>
        public StringScriptSource(string code)
            : this(code, null)
        {
        }

        /// <summary>
        /// Creates a new StringScriptSource instance.
        /// </summary>
        /// <param name="code"> The script code. </param>
        /// <param name="path"> The path of the file the script code was retrieved from. </param>
        public StringScriptSource(string code, string path)
        {
            if (code == null)
                throw new ArgumentNullException("code");
            this.m_code = code;
            this.m_path = path;
            this.m_flags = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the path of the source file (either a path on the file system or a URL).  This
        /// can be <c>null</c> if no path is available.
        /// </summary>
        public override string Path
        {
            get { return this.m_path; }
        }

        public override System.Collections.Generic.IDictionary<string, string> Flags
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns a reader that can be used to read the source code for the script.
        /// </summary>
        /// <returns> A reader that can be used to read the source code for the script, positioned
        /// at the start of the source code. </returns>
        /// <remarks> If this method is called multiple times then each reader must return the
        /// same source code. </remarks>
        public override TextReader GetReader()
        {
            return new StringReader(this.m_code);
        }
    }
}
