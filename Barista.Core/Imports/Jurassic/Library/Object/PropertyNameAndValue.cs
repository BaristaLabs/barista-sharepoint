namespace Barista.Jurassic.Library
{
  /// <summary>
  /// Represents a property name and value.
  /// </summary>
  public sealed class PropertyNameAndValue
  {
    private readonly string m_name;
    private PropertyDescriptor m_descriptor;

    public PropertyNameAndValue(string name, PropertyDescriptor descriptor)
    {
      this.m_name = name;
      this.m_descriptor = descriptor;
    }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string Name
    {
      get { return this.m_name; }
    }

    /// <summary>
    /// Gets the value of the property.
    /// </summary>
    public object Value
    {
      get { return this.m_descriptor.Value; }
    }

    /// <summary>
    /// Gets the property attributes.  These attributes describe how the property can
    /// be modified.
    /// </summary>
    public PropertyAttributes Attributes
    {
      get { return this.m_descriptor.Attributes; }
    }

    /// <summary>
    /// Gets a boolean value indicating whether the property value can be set.
    /// </summary>
    public bool IsWritable
    {
      get { return this.m_descriptor.IsWritable; }
    }

    /// <summary>
    /// Gets a boolean value indicating whether the property value will be included during an
    /// enumeration.
    /// </summary>
    public bool IsEnumerable
    {
      get { return this.m_descriptor.IsEnumerable; }
    }

    /// <summary>
    /// Gets a boolean value indicating whether the property can be deleted.
    /// </summary>
    public bool IsConfigurable
    {
      get { return this.m_descriptor.IsConfigurable; }
    }
  }
}
