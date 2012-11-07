namespace Barista.Extensions
{
  using System;
  using System.IO;

  public static class StreamExtensions
  {
    public static void CopyTo(this Stream input, Stream destination)
    {
      if (destination != null)
      {
        if (input.CanRead || input.CanWrite)
        {
          if (destination.CanRead || destination.CanWrite)
          {
            if (input.CanRead)
            {
              if (destination.CanWrite)
              {
                StreamExtensions.InternalCopyTo(input, destination, 81920);
                return;
              }
              else
              {
                throw new NotSupportedException("Cannot copy: The destination is an unwritable stream");
              }
            }
            else
            {
              throw new NotSupportedException("Cannot copy: The source is a unreadable Stream");
            }
          }
          else
          {
            throw new ObjectDisposedException("destination", "The destination stream has been disposed");
          }
        }
        else
        {
          throw new ObjectDisposedException(null, "The source streem has been disposed");
        }
      }
      else
      {
        throw new ArgumentNullException("destination");
      }
    }

    /// <summary>
    /// Converts the specified stream into a byte array. Use when the stream.length value is not available.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static byte[] ToByteArray(this Stream stream)
    {
      using (MemoryStream ms = new MemoryStream())
      {
        StreamExtensions.CopyTo(stream, ms);
        ms.Flush();
        ms.Seek(0, SeekOrigin.Begin);
        return ms.ToArray();
      }
    }

    /// <summary>
    /// Reads data into a complete array, throwing an EndOfStreamException
    /// if the stream runs out of data first, or if an IOException
    /// naturally occurs.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="data">The array to read bytes into. The array
    /// will be completely filled from the stream, so an appropriate
    /// size must be given.</param>
    public static void ReadWholeArray(this Stream stream, byte[] data)
    {
      int offset = 0;
      int remaining = data.Length;
      while (remaining > 0)
      {
        int read = stream.Read(data, offset, remaining);
        if (read <= 0)
          throw new EndOfStreamException
              (String.Format("End of stream reached with {0} bytes left to read", remaining));
        remaining -= read;
        offset += read;
      }
    }

    private static void InternalCopyTo(Stream input, Stream destination, int bufferSize)
    {
      byte[] numArray = new byte[bufferSize];
      while (true)
      {
        int num = input.Read(numArray, 0, (int)numArray.Length);
        int num1 = num;
        if (num == 0)
        {
          break;
        }
        destination.Write(numArray, 0, num1);
      }
    }
  }
}
