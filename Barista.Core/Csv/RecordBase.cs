namespace Barista.Csv
{
  using Barista.Extensions;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;
  using System.Text;

  /// <summary>
  /// A base class for CSV record types.
  /// </summary>
  /// <remarks>
  /// The CSV record types <see cref="HeaderRecord"/> and <see cref="DataRecord"/> obtain common functionality by inheriting from this class.
  /// </remarks>
  [Serializable]
  public abstract class RecordBase
  {
    /// <summary>
    /// See <see cref="Values"/>.
    /// </summary>
    private readonly IList<string> m_values;

    /// <summary>
    /// The character used to separator values in the <see cref="ToString"/> implementation
    /// </summary>
    public const char ValueSeparator = (char)0x2022;

    /// <summary>
    /// Gets the value at the specified index for this CSV record.
    /// </summary>
    public string this[int index]
    {
      get
      {
        return m_values[index];
      }
      set
      {
        m_values[index] = value;
      }
    }

    /// <summary>
    /// Gets a collection of values in this CSV record.
    /// </summary>
    public IList<string> Values
    {
      get
      {
        return m_values;
      }
    }

    /// <summary>
    /// Initialises an instance of <c>RecordBase</c> with no values.
    /// </summary>
    protected RecordBase()
    {
      m_values = new List<string>();
    }

    /// <summary>
    /// Initialises an instance of the <c>RecordBase</c> class with the values specified.
    /// </summary>
    /// <param name="values">
    /// The values for the CSV record.
    /// </param>
    protected RecordBase(IEnumerable<string> values)
      : this(values, false)
    {
    }

    /// <summary>
    /// Initialises an instance of the <c>RecordBase</c> class with the values specified, optionally making the value collection read-only.
    /// </summary>
    /// <param name="values">
    /// The values for the CSV record.
    /// </param>
    /// <param name="readOnly">
    /// If <see langword="true"/>, the value collection will be read-only.
    /// </param>
    protected RecordBase(IEnumerable<string> values, bool readOnly)
    {
      var valuesList = values as IList<string> ?? values.ToList();
      valuesList.AssertNotNull("values");

      m_values = new List<string>(valuesList);

      if (readOnly)
      {
        //just use the wrapper readonly collection
        m_values = new ReadOnlyCollection<string>(m_values);
      }
    }

    /// <summary>
    /// Gets the value at the specified index, or <see langword="null"/> if the index is invalid.
    /// </summary>
    /// <param name="index">
    /// The index.
    /// </param>
    /// <returns>
    /// The value, or <see langword="null"/>.
    /// </returns>
    public string GetValueOrNull(int index)
    {
      return m_values.ElementAtOrDefault(index);
    }

    /// <summary>
    /// Determines whether this <c>RecordBase</c> is equal to <paramref name="obj"/>.
    /// </summary>
    /// <remarks>
    /// Two <c>RecordBase</c> instances are considered equal if they contain the same number of values, and each of their corresponding values are also
    /// equal.
    /// </remarks>
    /// <param name="obj">
    /// The object to compare to this <c>RecordBase</c>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is equal to this <c>RecordBase</c>, otherwise <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(obj, this))
      {
        return true;
      }

      var record = obj as RecordBase;

      if (record == null)
      {
        return false;
      }

      if (m_values.Count != record.m_values.Count)
      {
        return false;
      }

      return !m_values.Where((t, i) => t != record.m_values[i]).Any();
    }

    /// <summary>
    /// Gets a hash code for this <c>RecordBase</c>.
    /// </summary>
    /// <returns>
    /// A hash code for this <c>RecordBase</c>.
    /// </returns>
    public override int GetHashCode()
    {
      return 17 + m_values.Sum(t => t.GetHashCode());
    }

    /// <summary>
    /// Returns a <c>string</c> representation of this CSV record.
    /// </summary>
    /// <remarks>
    /// This method is provided for debugging and diagnostics only. Each value in the record is present in the returned string, with a bullet
    /// character (<c>U+2022</c>) separating them.
    /// </remarks>
    /// <returns>
    /// A <c>string</c> representation of this record.
    /// </returns>
    public sealed override string ToString()
    {
      var retVal = new StringBuilder();

      foreach (var val in m_values)
      {
        retVal.Append(val).Append(ValueSeparator);
      }

      return retVal.ToString();
    }
  }
}