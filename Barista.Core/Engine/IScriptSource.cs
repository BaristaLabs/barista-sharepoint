namespace Barista.Engine
{
    using System.IO;

    /// <summary>
    /// Represents a resource that can provide script code.  This is the base interface of all
    /// the script providers.
    /// </summary>
    public interface IScriptSource
    {
        /// <summary>
        /// Gets the path of the source file (either a path on the file system or a URL).  This
        /// can be <c>null</c> if no path is available.
        /// </summary>
        string Path
        {
            get;
        }

        /// <summary>
        /// Returns a reader that can be used to read the source code for the script.
        /// </summary>
        /// <returns> A reader that can be used to read the source code for the script, positioned
        /// at the start of the source code. </returns>
        /// <remarks> If this method is called multiple times then each reader must return the
        /// same source code. </remarks>
        TextReader GetReader();
    }
}
