namespace Barista
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Text;
  using System.Linq;
  using System.Security.Cryptography;
  using Barista.Newtonsoft.Json;

  public class Etag : IEquatable<Etag>, IComparable<Etag>
  {
    private long m_restarts;
    private long m_changes;

    public Etag()
    {
    }

    public Etag(string str)
    {
      var etag = Parse(str);
      m_restarts = etag.m_restarts;
      m_changes = etag.m_changes;
    }

    public Etag(UuidType type, long restarts, long changes)
    {
      this.m_restarts = ((long)type << 56) | restarts;
      this.m_changes = changes;
    }

    public long Restarts
    {
      get { return m_restarts; }
    }

    public long Changes
    {
      get { return m_changes; }
    }

    public bool Equals(Etag other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return m_restarts == other.m_restarts && m_changes == other.m_changes;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((Etag)obj);
    }

    public int CompareTo(Etag other)
    {
      if (ReferenceEquals(other, null))
        return -1;
      var sub = m_restarts - other.m_restarts;
      if (Math.Abs(sub) > 0)
        return sub > 0 ? 1 : -1;
      sub = m_changes - other.m_changes;
      if (sub != 0)
        return sub > 0 ? 1 : -1;
      return 0;
    }

    private IEnumerable<byte> ToBytes()
    {
      foreach (var source in BitConverter.GetBytes(m_restarts).Reverse())
      {
        yield return source;
      }
      foreach (var source in BitConverter.GetBytes(m_changes).Reverse())
      {
        yield return source;
      }
    }

    public byte[] ToByteArray()
    {
      return ToBytes().ToArray();
    }

    public override int GetHashCode()
    {
      unchecked
      {
// ReSharper disable NonReadonlyFieldInGetHashCode
        return (m_restarts.GetHashCode() * 397) ^ m_changes.GetHashCode();
// ReSharper restore NonReadonlyFieldInGetHashCode
      }
    }

    public override string ToString()
    {
      var sb = new StringBuilder(36);
      foreach (var by in ToBytes())
      {
        sb.Append(by.ToString("X2"));
      }
      sb.Insert(8, "-")
          .Insert(13, "-")
          .Insert(18, "-")
          .Insert(23, "-");
      return sb.ToString();
    }

    #region Operators
    public static bool operator ==(Etag a, Etag b)
    {
      if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
        return true;
      if (ReferenceEquals(a, null))
        return false;
      return a.Equals(b);
    }

    public static bool operator !=(Etag a, Etag b)
    {
      return !(a == b);
    }

    public static implicit operator string(Etag etag)
    {
      if (etag == null)
        return null;
      return etag.ToString();
    }

    public static implicit operator Etag(string s)
    {
      return Parse(s);
    }
    #endregion

    #region Static Methods
    public static Etag Parse(byte[] bytes)
    {
      return new Etag
      {
        m_restarts = BitConverter.ToInt64(bytes.Take(8).Reverse().ToArray(), 0),
        m_changes = BitConverter.ToInt64(bytes.Skip(8).Reverse().ToArray(), 0)
      };
    }

    public static bool TryParse(string str, out Etag etag)
    {
      try
      {
        etag = Parse(str);
        return true;
      }
      catch (Exception)
      {
        etag = null;
        return false;
      }
    }

    public static Etag Parse(string str)
    {
      if (string.IsNullOrEmpty(str))
        throw new ArgumentException("str cannot be empty or null");
      if (str.Length != 36)
        throw new ArgumentException("str must be 36 characters");

      var buffer = new[]
			{
				byte.Parse(str.Substring(16, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(14, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(11, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(9, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(6, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(4, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(2, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(0, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(34, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(32, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(30, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(28, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(26, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(24, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(21, 2), NumberStyles.HexNumber),
				byte.Parse(str.Substring(19, 2), NumberStyles.HexNumber)
			};

      return new Etag
      {
        m_restarts = BitConverter.ToInt64(buffer, 0),
        m_changes = BitConverter.ToInt64(buffer, 8)
      };
    }

    public static Etag InvalidEtag
    {
      get
      {
        return new Etag
        {
          m_restarts = -1,
          m_changes = -1
        };
      }
    }

    public static Etag Empty
    {
      get
      {
        return new Etag
        {
          m_restarts = 0,
          m_changes = 0
        };
      }
    }

    public Etag Setup(UuidType type, long restartsNum)
    {
      return new Etag
      {
        m_restarts = ((long)type << 56) | restartsNum,
        m_changes = m_changes
      };
    }

    public Etag IncrementBy(int amount)
    {
      return new Etag
      {
        m_restarts = m_restarts,
        m_changes = m_changes + amount
      };
    }

    public static Etag Max(Etag first, Etag second)
    {
      if (first == null)
        return second;
      return first.CompareTo(second) > 0
        ? first
        : second;
    }
    #endregion

    public Etag HashWith(Etag other)
    {
      return HashWith(other.ToBytes());
    }

    public Etag HashWith(IEnumerable<byte> bytes)
    {
      var etagBytes = ToBytes().Concat(bytes).ToArray();
      using (var md5 = MD5.Create())
      {
        return Parse(md5.ComputeHash(etagBytes));
      }
    }
  }

  public class EtagJsonConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var etag = value as Etag;
      if (etag == null)
        writer.WriteNull();
      else
        writer.WriteValue(etag.ToString());
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      var s = reader.Value as string;
      return s == null ? null : Etag.Parse(s);
    }

    public override bool CanConvert(Type objectType)
    {
      return objectType == typeof(Etag);
    }
  }
}
