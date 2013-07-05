namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.IO;
  using System.Collections.Generic;
  using System.Linq;
  using Barista.Extensions;
  using Microsoft.WindowsAPICodePack.Shell;

  /// <summary>
  /// Represents a Document Store implementation that uses the file system as the persistance mechanism.
  /// </summary>
  /// <remarks>
  /// See http://technet.microsoft.com/en-us/library/ee176615.aspx for list of extended file properties..
  /// </remarks>
  public partial class FSDocumentStore : IDocumentStore
  {
    private readonly DirectoryInfo m_root;

    public FSDocumentStore(string path)
    {
      if (path.IsValidPath() == false)
        throw new ArgumentException(@"The specified path has invalid characters: " + path, "path");

      if (Directory.Exists(path) == false)
        Directory.CreateDirectory(path);

      m_root = new DirectoryInfo(path);
    }
  }
}
