using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFS.OrcaDB.Core.Extensions
{
  public static class EnumExtensions
  {
    public static bool IsSet<T>(this T value, T flags)
        where T : struct
    {
      Type type = typeof(T);

      // only works with enums
      if (!type.IsEnum) throw new ArgumentException(
          "The type parameter T must be an enum type.");

      // handle each underlying type
      Type numberType = Enum.GetUnderlyingType(type);

      if (numberType.Equals(typeof(int)))
      {
        return Box<int>(value, flags, (a, b) => (a & b) == b);
      }
      else if (numberType.Equals(typeof(sbyte)))
      {
        return Box<sbyte>(value, flags, (a, b) => (a & b) == b);
      }
      else if (numberType.Equals(typeof(byte)))
      {
        return Box<byte>(value, flags, (a, b) => (a & b) == b);
      }
      else if (numberType.Equals(typeof(short)))
      {
        return Box<short>(value, flags, (a, b) => (a & b) == b);
      }
      else if (numberType.Equals(typeof(ushort)))
      {
        return Box<ushort>(value, flags, (a, b) => (a & b) == b);
      }
      else if (numberType.Equals(typeof(uint)))
      {
        return Box<uint>(value, flags, (a, b) => (a & b) == b);
      }
      else if (numberType.Equals(typeof(long)))
      {
        return Box<long>(value, flags, (a, b) => (a & b) == b);
      }
      else if (numberType.Equals(typeof(ulong)))
      {
        return Box<ulong>(value, flags, (a, b) => (a & b) == b);
      }
      else if (numberType.Equals(typeof(char)))
      {
        return Box<char>(value, flags, (a, b) => (a & b) == b);
      }
      else
      {
        throw new ArgumentException(
            "Unknown enum underlying type " +
            numberType.Name + ".");
      }
    }

    /// Helper function for handling the value types. Boxes the
    /// params to object so that the cast can be called on them.
    private static bool Box<T>(object value, object flags,
        Func<T, T, bool> op)
    {
      return op((T)value, (T)flags);
    }
  }
}
