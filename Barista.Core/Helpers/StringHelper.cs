namespace Barista
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.IO;

  public static class StringHelper
  {
    // stackoverflow.com/questions/1613896/truncate-string-on-whole-words-in-net-c
    public static string TruncateAtWord(string input, int length)
    {
      if (input == null || input.Length < length)
        return input;
      int iNextSpace = input.LastIndexOf(" ", length);
      return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
    }

    public static string ByteArrayToString(byte[] ba)
    {
      StringBuilder hex = new StringBuilder(ba.Length * 2);
      foreach (byte b in ba)
        hex.AppendFormat("{0:x2}", b);
      return hex.ToString();
    }

    public static byte[] StringToByteArray(String hex)
    {
      int numberChars = hex.Length;
      byte[] bytes = new byte[numberChars / 2];
      for (int i = 0; i < numberChars; i += 2)
        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
      return bytes;
    }

    /// <summary>
    /// Gets the name of the MIME type for the specified file type.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static string GetMimeTypeFromFileName(string fileName)
    {
      string mime = "application/octet-stream";
      if (String.IsNullOrEmpty(fileName) == false)
      {
        string ext = System.IO.Path.GetExtension(fileName).ToLower();
        Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
        if (rk != null && rk.GetValue("Content Type") != null)
          mime = rk.GetValue("Content Type").ToString();
      }
      return mime;
    }
  }
}
