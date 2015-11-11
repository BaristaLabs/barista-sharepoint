namespace Barista.Jurassic
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a script file.
    /// </summary>
    public class FileScriptSource : ScriptSource
    {
        private readonly string m_path;
        private readonly Encoding m_encoding;
        private readonly Dictionary<string, string> m_flags;

        /// <summary>
        /// Creates a new FileScriptSource instance.
        /// </summary>
        /// <param name="path"> The path of the script file. </param>
        /// <param name="encoding"> The character encoding to use if the file lacks a byte order
        /// mark (BOM).  If this parameter is omitted, the file is assumed to be UTF8. </param>
        public FileScriptSource(string path, Encoding encoding)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            this.m_path = path;
            this.m_encoding = encoding ?? Encoding.UTF8;
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
            return new StreamReader(this.Path, this.m_encoding, true);
        }
    }
}
