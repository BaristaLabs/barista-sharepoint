namespace Barista.Framework
{
  using System;
  using System.IO;
  using System.Net;
  using System.ServiceModel.Web;
  using System.Text;
  using System.Text.RegularExpressions;
  using Barista.Extensions;

  public class MultipartContentProcessor
  {
    private static readonly Regex FilenameRegex = new Regex(@"Content-Disposition\:\s*?form-data;\s*?name=""files\[\]"";.*?filename=""(?<filename>.*?)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ContentTypeRegex = new Regex(@"Content-Type\:\s*?(?<contenttype>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public MultipartContentProcessor(Stream stream)
      : this(stream, Encoding.UTF8)
    {
      
    }

    public MultipartContentProcessor(Stream stream, Encoding encoding)
    {
      if (WebOperationContext.Current != null)
      {
        var contentType = WebOperationContext.Current.IncomingRequest.ContentType;

        if (String.IsNullOrEmpty(contentType) || contentType.StartsWith("multipart/form-data") == false)
        {
          WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.UnsupportedMediaType;
          throw new WebException("Expected multipart/form-data content type.");
        }

        var index = contentType.IndexOf("boundary=", StringComparison.InvariantCultureIgnoreCase);
        if (index == -1)
        {

        }

        var delimiter = contentType.Substring(index + 9);

        this.Parse(stream, encoding, delimiter);
      }
    }

    public bool IsValid
    {
      get;
      private set;
    }

    public string FormData
    {
      get;
      private set;
    }

    public string ContentType
    {
      get;
      private set;
    }

    public string FileName
    {
      get;
      private set;
    }

    public byte[] FileContents
    {
      get;
      private set;
    }

    public string Redirect
    {
      get;
      private set;
    }

    private static int IndexOf(byte[] searchWithin, byte[] searchFor, int startIndex)
    {
      int index = 0;
      int startPos = Array.IndexOf(searchWithin, searchFor[0], startIndex);

      if (startPos != -1)
      {
        while ((startPos + index) < searchWithin.Length)
        {
          if (searchWithin[startPos + index] == searchFor[index])
          {
            index++;
            if (index == searchFor.Length)
            {
              return startPos;
            }
          }
          else
          {
            startPos = Array.IndexOf(searchWithin, searchFor[0], startPos + index);
            if (startPos == -1)
            {
              return -1;
            }
            index = 0;
          }
        }
      }

      return -1;
    }

    private void Parse(Stream stream, Encoding encoding, string delimiter)
    {
      this.IsValid = false;

      // Read the stream into a byte array
      byte[] data = stream.ToByteArray();

      // Copy to a string for header parsing
      var content = encoding.GetString(data);

      // Look for Form Data
      var formDataIndex = content.IndexOf("form-data\r\n\r\n", StringComparison.InvariantCultureIgnoreCase);
      if (formDataIndex > -1)
      {
        var startForm = formDataIndex + "form-data\r\n\r\n".Length;
        var endForm = content.Substring(startForm).IndexOf("\r\n" + delimiter, System.StringComparison.InvariantCultureIgnoreCase);
        var formData = content.Substring(startForm, endForm);

        if (!string.IsNullOrEmpty(formData))
        {
          FormData = Uri.UnescapeDataString(formData);
        }
      }

      Regex redirectPartRegex = new Regex("^--" + delimiter + @".*?Content-Disposition\:\s*?form-data;\s*?name=""redirect"".*?\r?\n\r?\n", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
      Match redirectPartMatch = redirectPartRegex.Match(content);

      if (redirectPartMatch.Success)
      {
        int startIndex = redirectPartMatch.Index + redirectPartMatch.Length;
        Regex redirectPartEndRegex = new Regex("\r\n^--" + delimiter, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        Match redirectPartEndMatch = redirectPartEndRegex.Match(content, startIndex);

        if (redirectPartEndMatch.Success)
        {
          int endIndex = redirectPartEndMatch.Index;

          this.Redirect = content.Substring(startIndex, endIndex - startIndex);
        }
      }

      Regex contentStartRegex = new Regex("^--" + delimiter + @".*?Content-Disposition\:\s*?form-data;\s*?name=""files\[\]"";.*?\r?\n\r?\n", RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase);
      Regex contentEndRegex = new Regex("\r\n^--" + delimiter + @"--\r?\n?$", RegexOptions.Multiline | RegexOptions.Singleline);
      Match contentStartMatch = contentStartRegex.Match(content);

      // Did we find the required values?
      if (contentStartMatch.Success)
      {
        var headerContent = contentStartMatch.Value;

        // Look for filename
        Match filenameMatch = FilenameRegex.Match(headerContent);

        // Look for contentType
        Match contentTypeMatch = ContentTypeRegex.Match(headerContent);

        if (contentTypeMatch.Success)
        {
          this.ContentType = contentTypeMatch.Groups["contenttype"].Value.Trim();
        }

        if (filenameMatch.Success)
        {
          //Uri fileUri = new Uri(filenameMatch.Groups["filename"].Value.Trim(), UriKind.RelativeOrAbsolute);

          this.FileName = Path.GetFileName(filenameMatch.Groups["filename"].Value.Trim());
        }

        // Get the start & end indexes of the file contents
        int startIndex = IndexOf(data, encoding.GetBytes(contentStartMatch.Value), 0) + encoding.GetBytes(contentStartMatch.Value).Length;

        Match contentEndMatch = contentEndRegex.Match(content, startIndex);
        if (contentEndMatch.Success)
        {
          var endIndex = IndexOf(data, encoding.GetBytes(contentEndMatch.Value), startIndex);

          int contentLength = endIndex - startIndex;

          // Extract the file contents from the byte array
           byte[] fileData = new byte[contentLength];

          Buffer.BlockCopy(data, startIndex, fileData, 0, contentLength);

          this.FileContents = fileData;
        }
      }
      
      this.IsValid = true;
    }
  }
}
