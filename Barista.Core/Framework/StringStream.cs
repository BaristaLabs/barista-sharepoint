namespace Barista.Framework
{
  using System.IO;
  using System.Text;

  /// <summary>
  /// Represents a stream that is based on an in-memory string.
  /// </summary>
  public class StringStream : MemoryStream
  {
    public StringStream(string str) :
      base(Encoding.UTF8.GetBytes(str), false) { }
  }
}