namespace Barista.Extensions
{
  using System;

  public static class EnumExtensions
  {
    public static bool IsSet<T>(this T value, T flags)
        where T : struct
    {
      var type = typeof(T);

      // only works with enums
      if (!type.IsEnum) throw new ArgumentException(
          "The type parameter T must be an enum type.");

      // handle each underlying type
      var numberType = Enum.GetUnderlyingType(type);

      if (numberType == typeof(int))
      {
        return Box<int>(value, flags, (a, b) => (a & b) == b);
      }

      if (numberType == typeof(sbyte))
      {
        return Box<sbyte>(value, flags, (a, b) => (a & b) == b);
      }

      if (numberType == typeof(byte))
      {
        return Box<byte>(value, flags, (a, b) => (a & b) == b);
      }

      if (numberType == typeof(short))
      {
        return Box<short>(value, flags, (a, b) => (a & b) == b);
      }

      if (numberType == typeof(ushort))
      {
        return Box<ushort>(value, flags, (a, b) => (a & b) == b);
      }

      if (numberType == typeof(uint))
      {
        return Box<uint>(value, flags, (a, b) => (a & b) == b);
      }
      if (numberType == typeof(long))
      {
        return Box<long>(value, flags, (a, b) => (a & b) == b);
      }

      if (numberType == typeof(ulong))
      {
        return Box<ulong>(value, flags, (a, b) => (a & b) == b);
      }

      if (numberType == typeof(char))
      {
        return Box<char>(value, flags, (a, b) => (a & b) == b);
      }

      throw new ArgumentException(
        "Unknown enum underlying type " +
        numberType.Name + ".");
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
