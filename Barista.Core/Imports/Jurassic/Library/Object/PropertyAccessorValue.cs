namespace Jurassic.Library
{
  using System;

  /// <summary>
  /// Represents a the value of an accessor property.
  /// </summary>
  [Serializable]
  internal sealed class PropertyAccessorValue
  {
    private readonly FunctionInstance m_getter;
    private readonly FunctionInstance m_setter;

    /// <summary>
    /// Creates a new PropertyAccessorValue instance.
    /// </summary>
    /// <param name="getter"> The getter function, or <c>null</c> if no getter was provided. </param>
    /// <param name="setter"> The setter function, or <c>null</c> if no setter was provided. </param>
    public PropertyAccessorValue(FunctionInstance getter, FunctionInstance setter)
    {
      this.m_getter = getter;
      this.m_setter = setter;
    }

    /// <summary>
    /// Gets the function that is called when the property value is retrieved.
    /// </summary>
    public FunctionInstance Getter
    {
      get { return this.m_getter; }
    }

    /// <summary>
    /// Gets the function that is called when the property value is modified.
    /// </summary>
    public FunctionInstance Setter
    {
      get { return this.m_setter; }
    }

    /// <summary>
    /// Gets the property value by calling the getter, if one is present.
    /// </summary>
    /// <param name="thisObject"> The value of the "this" keyword inside the getter. </param>
    /// <returns> The property value returned by the getter. </returns>
    public object GetValue(ObjectInstance thisObject)
    {
      if (this.m_getter == null)
        return Undefined.Value;
      return this.m_getter.CallLateBound(thisObject);
    }

    /// <summary>
    /// Sets the property value by calling the setter, if one is present.
    /// </summary>
    /// <param name="thisObject"> The value of the "this" keyword inside the setter. </param>
    /// <param name="value"> The desired value. </param>
    public void SetValue(ObjectInstance thisObject, object value)
    {
      if (this.m_setter == null)
        return;
      this.m_setter.CallLateBound(thisObject, value);
    }
  }

}
