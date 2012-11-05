namespace Barista.OrcaDB
{
  using System;
  using System.IO;

  public static class FileHelper
  {
    public static bool IsFileLocked(FileInfo file)
    {
      FileStream stream = null;

      try
      {
        stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
      }
      catch (IOException)
      {
        //the file is unavailable because it is:
        //still being written to
        //or being processed by another thread
        //or does not exist (has already been processed)
        return true;
      }
      finally
      {
        if (stream != null)
          stream.Close();
      }

      //file is not locked
      return false;
    }

    /// <summary>
    /// Blocks until the file is not locked any more.
    /// </summary>
    /// <param name="fullPath"></param>
    public static FileStream WaitForFile(string fullPath, int maxTries = 100)
    {
      int numTries = 0;
      while (true)
      {
        ++numTries;
        try
        {
          // Attempt to open the file exclusively.
          FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 100);
          fs.ReadByte();
          fs.Seek(0, SeekOrigin.Begin);
          return fs;
        }
        catch (Exception)
        {
          if (numTries > maxTries)
            return null;

          // Wait for the lock to be released
          System.Threading.Thread.Sleep(250);
        }
      }

      return null;
    }
  }
}
