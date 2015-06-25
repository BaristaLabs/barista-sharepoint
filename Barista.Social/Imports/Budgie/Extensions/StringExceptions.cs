namespace Barista.Social.Imports.Budgie.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  internal static class StringExceptions
  {
    /// <summary>
    /// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
    /// </summary>
    private static readonly Dictionary<String, String> UriRfc3986CharsToEscape = new Dictionary<string, string>
        {
            {"!", "%21"},
            {"*", "%2A"},
            {"'", "%27"},
            {"(", "%28"},
            {")", "%29"},
        };

    internal static Uri ToUri(this string s)
    {
      Uri u;
      if (Uri.TryCreate(s, UriKind.Absolute, out u))
      {
        return u;
      }
      return null;
    }

    /// <summary>
    /// Escapes a string according to the URI data string rules given in RFC 3986.
    /// </summary>
    /// <param name="s">The value to escape.</param>
    /// <returns>The escaped value.</returns>
    /// <remarks>
    /// The <see cref="Uri.EscapeDataString"/> method is <i>supposed</i> to take on
    /// RFC 3986 behavior if certain elements are present in a .config file.  Even if this
    /// actually worked (which in my experiments it <i>doesn't</i>), we can't rely on every
    /// host actually having this configuration element present.
    /// </remarks>
    internal static string ToRfc3986Encoded(this string s)
    {
      // Start with RFC 2396 escaping by calling the .NET method to do the work.
      // This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
      // If it does, the escaping we do that follows it will be a no-op since the
      // characters we search for to replace can't possibly exist in the string.
      var escaped = new StringBuilder(Uri.EscapeDataString(s));

      // Upgrade the escaping to RFC 3986, if necessary.
      foreach (var kv in UriRfc3986CharsToEscape)
      {
        escaped.Replace(kv.Key, kv.Value);
      }

      // Return the fully-RFC3986-escaped string.
      return escaped.ToString();
    }
  }
}
