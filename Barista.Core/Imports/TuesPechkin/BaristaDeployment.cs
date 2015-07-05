namespace Barista.TuesPechkin
{
    using System;
    using System.IO;
    using System.Reflection;
    using Barista.Extensions;
    using ICSharpCode.SharpZipLib.Core;
    using ICSharpCode.SharpZipLib.Zip;

    [Serializable]
    public class BaristaDeployment : IDeployment
    {
         
        public static string WkHtmlToPdfSubFolderName = "wkhtmltopdf";
        private readonly string m_binDirectory;
        private string m_path;

        public BaristaDeployment(string binDirectory)
        {
            m_binDirectory = binDirectory;
        }

        public string Path
        {
            get
            {
                if (m_path != null)
                    return m_path;

                var assemblyRoot = "";

                if (!m_binDirectory.IsNullOrWhiteSpace())
                    assemblyRoot = m_binDirectory;
                else
                {
                    var codebaseuri = Assembly.GetExecutingAssembly().CodeBase;
                    Uri codebaseURI;
                    if (Uri.TryCreate(codebaseuri, UriKind.Absolute, out codebaseURI))
                        assemblyRoot = System.IO.Path.GetDirectoryName(codebaseURI.LocalPath); // (check pre-shadow copy location first)

                    if (assemblyRoot != null && !Directory.Exists(assemblyRoot))
                        assemblyRoot = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // (check loaded assembly location next)
                }

                if (assemblyRoot == null || assemblyRoot.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("Unable to locate assembly root.");

                if (Directory.Exists(System.IO.Path.Combine(assemblyRoot, WkHtmlToPdfSubFolderName)))
                    assemblyRoot = System.IO.Path.Combine(assemblyRoot, WkHtmlToPdfSubFolderName);

                var dllFileName = System.IO.Path.Combine(assemblyRoot, WkhtmltoxBindings.DLLNAME);
                if (!File.Exists(dllFileName))
                {
                    var zipFileName = System.IO.Path.Combine(assemblyRoot, "wkhtmltopdf.zip");
                    if (!File.Exists(zipFileName))
                        throw new InvalidOperationException("Unable to locate required WkHtmlToPdf files.");
                    ExtractZipFile(zipFileName, assemblyRoot);
                }

                m_path = assemblyRoot;
                return m_path;
            }
        }

        private static void ExtractZipFile(string archiveFilenameIn, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    var entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    var buffer = new byte[4096];     // 4K is optimum
                    var zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    var fullZipToPath = System.IO.Path.Combine(outFolder, entryFileName);
                    var directoryName = System.IO.Path.GetDirectoryName(fullZipToPath);
                    if (directoryName == null)
                        throw new InvalidOperationException("Unable to obtain the directory.");

                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }
    }
}
