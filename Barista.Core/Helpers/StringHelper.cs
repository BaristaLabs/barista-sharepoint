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
      var iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);
      return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
    }

    public static string ByteArrayToString(byte[] ba)
    {
      var hex = new StringBuilder(ba.Length * 2);
      foreach (var b in ba)
        hex.AppendFormat("{0:x2}", b);
      return hex.ToString();
    }

    public static byte[] StringToByteArray(String hex)
    {
      var numberChars = hex.Length;
      var bytes = new byte[numberChars / 2];
      for (var i = 0; i < numberChars; i += 2)
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

      var mime = "application/octet-stream";
      var extension = System.IO.Path.GetExtension(fileName);
      if (extension != null)
      {
        var ext = extension.ToLower();
        var rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
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
      var md5 = MD5.Create();
      var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
      var hashBytes = md5.ComputeHash(inputBytes);

      // Convert the byte array to hexadecimal string
      var sb = new StringBuilder();
      for (var i = 0; i < hashBytes.Length; i++)
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
