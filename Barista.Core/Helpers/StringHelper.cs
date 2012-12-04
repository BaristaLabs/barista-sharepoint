namespace Barista
{
  using System;
  using System.Text;
  using System.Security.Cryptography;

  public static class StringHelper
  {
    // stackoverflow.com/questions/1613896/truncate-string-on-whole-words-in-net-c
    public static string TruncateAtWord(string input, int length)
    {
      if (input == null || input.Length < length)
        return input;
      int iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);
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
      if (String.IsNullOrEmpty(fileName))
        return "";

      string mime = "application/octet-stream";
      var extension = System.IO.Path.GetExtension(fileName);
      if (extension != null)
      {
        string ext = extension.ToLower();
        Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
        if (rk != null && rk.GetValue("Content Type") != null)
          mime = rk.GetValue("Content Type").ToString();
      }
      return mime;
    }

    /// <summary>
    /// Creates an MD5 Hash of the specified string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string CreateMD5Hash(string input)
    {
      // Use input string to calculate MD5 hash
      MD5 md5 = MD5.Create();
      byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
      byte[] hashBytes = md5.ComputeHash(inputBytes);

      // Convert the byte array to hexadecimal string
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < hashBytes.Length; i++)
      {
        sb.Append(hashBytes[i].ToString("X2"));
        // To force the hex string to lower-case letters instead of
        // upper-case, use he following line instead:
        // sb.Append(hashBytes[i].ToString("x2")); 
      }
      return sb.ToString();
    }
  }
}
