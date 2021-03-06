﻿namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Runtime.Serialization.Formatters;
  using System.Runtime.Serialization.Formatters.Binary;
  using System.Text.RegularExpressions;
  using Barista.Newtonsoft.Json;
  using System.Runtime.Serialization.Json;
  using System.Text;

  public static class DocumentStoreHelper
  {
    public static JsonSerializerSettings JsonSerializerSettings
    {
      get
      {
        return new JsonSerializerSettings
          {
          PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All,
          TypeNameHandling = TypeNameHandling.Auto,
          NullValueHandling = NullValueHandling.Ignore,
        };
      }
    }
    /// <summary>
    /// Serializes the object to json.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static string SerializeObjectToJson<T>(T value)
    {
      return JsonConvert.SerializeObject(value, Formatting.Indented, JsonSerializerSettings);
    }

    /// <summary>
    /// Using Json.Net, makes a clone of the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static T CloneObject<T>(T entity)
    {
      var data = JsonConvert.SerializeObject(entity);
      return JsonConvert.DeserializeObject<T>(data);
    }

    /// <summary>
    /// Using a Binary Formatter, makes a clone of the object.
    /// </summary>
    /// <remarks>
    /// The Object being cloned, and all nested objects, must be marked serializable.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static T CloneObjectBinary<T>(T entity)
    {
      using (var ms = new MemoryStream())
      {
        var formatter = new BinaryFormatter();
        formatter.Serialize(ms, entity);
        ms.Position = 0;

        return (T)formatter.Deserialize(ms);
      }
    }

    /// <summary>
    /// Deserializes the object from json.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    public static T DeserializeObjectFromJson<T>(string data)
    {
      if (data == null || String.IsNullOrEmpty(data.Trim()))
        return default(T);

      return JsonConvert.DeserializeObject<T>(data, JsonSerializerSettings);
    }

    /// <summary>
    /// Deserializes the object from the wcf flavor of Json.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    public static T DeserializeObjectFromWcfJson<T>(string data)
    {
      DataContractJsonSerializer dsjs = new DataContractJsonSerializer(typeof(T));
      var bits = Encoding.UTF8.GetBytes(data);
      using (MemoryStream ms = new MemoryStream(bits))
      {
        return ((T)dsjs.ReadObject(ms));
      }
    }

    private static readonly Regex PathRegex = new Regex("^((?<Segment>[^/]+)/?)+$", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

    public static bool IsValidPath(string path)
    {
      return PathRegex.IsMatch(path);
    }

    /// <summary>
    /// Gets the path segments.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static IEnumerable<string> GetPathSegments(string path)
    {
      if (string.IsNullOrEmpty(path))
        yield break;

      Match m = PathRegex.Match(path);
      if (m.Success == false)
        throw new InvalidOperationException("Specified path is not a valid path.");

      var paths = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var segment in paths)
        yield return segment;
    }

    /// <summary>
    /// Copies the stream from the input to the output.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="output">The output.</param>
    public static void CopyStream(Stream input, Stream output)
    {
      byte[] buffer = new byte[32768];
      int read;
      while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
      {
        output.Write(buffer, 0, read);
      }
    }

    /// <summary>
    /// Reads the contents of the file stream as a byte array.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns></returns>
    public static byte[] ReadToEnd(this Stream stream)
    {
      long originalPosition = stream.Position;
      stream.Position = 0;

      try
      {
        byte[] readBuffer = new byte[4096];

        int totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
        {
          totalBytesRead += bytesRead;

          if (totalBytesRead == readBuffer.Length)
          {
            int nextByte = stream.ReadByte();
            if (nextByte != -1)
            {
              byte[] temp = new byte[readBuffer.Length * 2];
              Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
              Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
              readBuffer = temp;
              totalBytesRead++;
            }
          }
        }

        byte[] buffer = readBuffer;
        if (readBuffer.Length != totalBytesRead)
        {
          buffer = new byte[totalBytesRead];
          Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
        }
        return buffer;
      }
      finally
      {
        stream.Position = originalPosition;
      }
    }
  }
}
