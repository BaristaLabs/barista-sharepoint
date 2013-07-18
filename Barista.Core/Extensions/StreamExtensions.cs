namespace Barista.Extensions
{
  using System;
  using System.IO;

  public static class StreamExtensions
  {
    public static void CopyTo(this Stream input, Stream destination)
    {
      if (destination == null)
        throw new ArgumentNullException("destination");

      if (!input.CanRead && !input.CanWrite)
        throw new ObjectDisposedException(null, "The source streem has been disposed");

      if (!destination.CanRead && !destination.CanWrite)
        throw new ObjectDisposedException("destination", "The destination stream has been disposed");

      if (!input.CanRead)
        throw new NotSupportedException("Cannot copy: The source is a unreadable Stream");

      if (!destination.CanWrite)
        throw new NotSupportedException("Cannot copy: The destination is an unwritable stream");

      StreamExtensions.InternalCopyTo(input, destination, 81920);
    }

    /// <summary>
    /// Converts the specified stream into a byte array. Use when the stream.length value is not available.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static byte[] ToByteArray(this Stream stream)
    {
      using (var ms = new MemoryStream())
      {
        stream.CopyTo(ms);
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

    /// <summary>
    /// Reads data from a stream until the end is reached. The
    /// data is returned as a byte array. An IOException is
    /// thrown if any of the underlying IO calls fail.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="initialLength">The initial buffer length. 0 to use default initial length.</param>
    [Obsolete]
    public static byte[] ReadFully(this Stream stream, int initialLength)
    {
      // If we've been passed an unhelpful initial length, just
      // use 32K.
      if (initialLength < 1)
      {
        initialLength = 32768;
      }

      byte[] buffer = new byte[initialLength];
      int read = 0;

      int chunk;
      while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
      {
        read += chunk;

        // If we've reached the end of our buffer, check to see if there's
        // any more information
        if (read == buffer.Length)
        {
          int nextByte = stream.ReadByte();

          // End of stream? If so, we're done
          if (nextByte == -1)
          {
            return buffer;
          }

          // Nope. Resize the buffer, put in the byte we've just
          // read, and continue
          byte[] newBuffer = new byte[buffer.Length * 2];
          Array.Copy(buffer, newBuffer, buffer.Length);
          newBuffer[read] = (byte)nextByte;
          buffer = newBuffer;
          read++;
        }
      }
      // Buffer is now too big. Shrink it.
      byte[] ret = new byte[read];
      Array.Copy(buffer, ret, read);
      return ret;
    }

    private static void InternalCopyTo(Stream input, Stream destination, int bufferSize)
    {
      byte[] numArray = new byte[bufferSize];
      while (true)
      {
        int num = input.Read(numArray, 0, numArray.Length);
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
