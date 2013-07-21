namespace Barista.Jurassic
{
  using System;
  using System.Text;

  /// <summary>
  /// Represents a string that supports efficient concatenation.  This class is used instead of
  /// <see cref="System.String"/> when two strings are concatenated together using the addition
  /// operator (+) or the concat() function.  Use of this class avoids the creation of useless
  /// intermediary strings and by doing so speeds up string concatenation dramatically
  /// (this change improved sunspider/string-validate-input.js by almost 20x).
  /// </summary>
  [Serializable]
  public sealed class ConcatenatedString
  {
    private StringBuilder m_builder;
    private int m_length;
    private string m_cachedString;

    /// <summary>
    /// Creates a new ConcatenatedString instance from the given string.
    /// </summary>
    /// <param name="initialValue"> The initial contents of the concatenated string. </param>
    public ConcatenatedString(string initialValue)
    {
      if (initialValue == null)
        throw new ArgumentNullException("initialValue");
      this.m_builder = new StringBuilder(initialValue);
      this.m_length = initialValue.Length;
      this.m_cachedString = initialValue;
    }

    /// <summary>
    /// Creates a new ConcatenatedString instance by concatenating the given strings.
    /// </summary>
    /// <param name="left"> The left-most string to concatenate. </param>
    /// <param name="right">The right-most string to concatenate</param>
    public ConcatenatedString(string left, string right)
    {
      if (left == null)
        throw new ArgumentNullException("left");
      if (right == null)
        throw new ArgumentNullException("right");
      this.m_builder = new StringBuilder(left, left.Length + right.Length);
      this.m_builder.Append(right);
      this.m_length = left.Length + right.Length;
    }

    /// <summary>
    /// Creates a new ConcatenatedString instance by concatenating the given strings.
    /// </summary>
    /// <param name="strings"> The strings to concatenate to form the initial value for this
    /// object. The array must not be altered after passing it to this constructor. </param>
    public ConcatenatedString(string[] strings)
    {
      if (strings == null)
        throw new ArgumentNullException("strings");

      // Calculate the total length.
      foreach (string str in strings)
        this.m_length += str == null ? 0 : str.Length;

      // Append the strings.
      this.m_builder = new StringBuilder(this.m_length);
      foreach (string str in strings)
        if (str != null)
          this.m_builder.Append(str);
    }

    private ConcatenatedString(StringBuilder builder)
    {
      this.m_builder = builder;
      this.m_length = builder.Length;
    }

    /// <summary>
    /// Gets the length, in characters, of the ConcatenatedString object.
    /// </summary>
    public int Length
    {
      get { return this.m_length; }
    }

    /// <summary>
    /// Returns a new ConcatenatedString instance containing the concatenation of this ConcatenatedString
    /// and the given object (converted to a string).
    /// </summary>
    /// <param name="obj"> The object to append to this string. </param>
    /// <returns> A new ConcatenatedString instance representing the concatenated string. </returns>
    public ConcatenatedString Concatenate(object obj)
    {
      if (obj is string)
        return Concatenate((string)obj);
      if (obj is ConcatenatedString)
        return Concatenate((ConcatenatedString)obj);
      return Concatenate(TypeConverter.ToString(obj));
    }

    /// <summary>
    /// Returns a new ConcatenatedString instance containing the concatenation of this ConcatenatedString
    /// and the given string.
    /// </summary>
    /// <param name="str"> The string to append. </param>
    /// <returns> A new ConcatenatedString instance representing the concatenated string. </returns>
    public ConcatenatedString Concatenate(string str)
    {
      if (this.m_length == this.m_builder.Length)
      {
        this.m_builder.Append(str);
        return new ConcatenatedString(this.m_builder);
      }

      var builder = new StringBuilder(this.m_builder.ToString(0, this.m_length), this.Length + str.Length);
      builder.Append(str);
      return new ConcatenatedString(builder);
    }

    /// <summary>
    /// Returns a new ConcatenatedString instance containing the concatenation of this ConcatenatedString
    /// and the given string.
    /// </summary>
    /// <param name="str"> The string to append. </param>
    /// <returns> A new ConcatenatedString instance representing the concatenated string. </returns>
    public ConcatenatedString Concatenate(ConcatenatedString str)
    {
      return Concatenate(str.ToString());
    }

    /// <summary>
    /// Appends the given object (converted to a string) to the end of this object.
    /// </summary>
    /// <param name="obj"> The object to append. </param>
    public void Append(object obj)
    {
      if (obj is string)
        Append((string)obj);
      else if (obj is ConcatenatedString)
        Append((ConcatenatedString)obj);
      else
        Append(TypeConverter.ToString(obj));
    }

    /// <summary>
    /// Appends the given string to the end of this object.
    /// </summary>
    /// <param name="str"> The string to append. </param>
    public void Append(string str)
    {
      if (this.m_length == this.m_builder.Length)
      {
        this.m_builder.Append(str);
        this.m_length = this.m_builder.Length;
        this.m_cachedString = null;
      }
      else
      {
        var builder = new StringBuilder(this.m_builder.ToString(0, this.m_length), this.Length + str.Length);
        builder.Append(str);
        this.m_builder = builder;
        this.m_length = builder.Length;
        this.m_cachedString = null;
      }
    }

    /// <summary>
    /// Appends the given string to the end of this object.
    /// </summary>
    /// <param name="str"> The string to append. </param>
    public void Append(ConcatenatedString str)
    {
      Append(str.ToString());
    }

    /// <summary>
    /// Returns a string representing the current object.
    /// </summary>
    /// <returns> A string representing the current object. </returns>
    public override string ToString()
    {
      if (this.m_cachedString == null)
        this.m_cachedString = this.m_builder.ToString(0, this.m_length);
      return this.m_cachedString;
    }
  }

}
