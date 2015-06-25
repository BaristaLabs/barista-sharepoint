namespace Barista.Jurassic.Library
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  /// <summary>
  /// Represents a container for property names and attributes.
  /// </summary>
  [Serializable]
  internal class HiddenClassSchema
  {
    // Properties
    private Dictionary<string, SchemaProperty> m_properties;

    // Transitions
    [Serializable]
    private struct TransitionInfo
    {
      public string Name;
      public PropertyAttributes Attributes;
    }
    private Dictionary<TransitionInfo, HiddenClassSchema> m_addTransitions;
    private Dictionary<string, HiddenClassSchema> m_deleteTransitions;
    private Dictionary<TransitionInfo, HiddenClassSchema> m_modifyTransitions;

    // The index of the next value.
    private readonly int m_nextValueIndex;

    // Used to recreate the properties dictionary if properties == null.
    private HiddenClassSchema m_parent;
    private TransitionInfo m_addPropertyTransitionInfo;

    /// <summary>
    /// Creates a new HiddenClassSchema instance from a modify or delete operation.
    /// </summary>
    private HiddenClassSchema(Dictionary<string, SchemaProperty> properties, int nextValueIndex)
    {
      this.m_properties = properties;
      this.m_addTransitions = null;
      this.m_deleteTransitions = null;
      this.m_modifyTransitions = null;
      this.m_nextValueIndex = nextValueIndex;
    }

    /// <summary>
    /// Creates a new HiddenClassSchema instance from an add operation.
    /// </summary>
    private HiddenClassSchema(Dictionary<string, SchemaProperty> properties, int nextValueIndex, HiddenClassSchema parent, TransitionInfo addPropertyTransitionInfo)
      : this(properties, nextValueIndex)
    {
      this.m_parent = parent;
      this.m_addPropertyTransitionInfo = addPropertyTransitionInfo;
    }

    /// <summary>
    /// Creates a hidden class schema with no properties.
    /// </summary>
    /// <returns> A hidden class schema with no properties. </returns>
    public static HiddenClassSchema CreateEmptySchema()
    {
      return new HiddenClassSchema(new Dictionary<string, SchemaProperty>(), 0);
    }

    /// <summary>
    /// Gets the number of properties defined in this schema.
    /// </summary>
    public int PropertyCount
    {
      get
      {
        if (this.m_properties == null)
          this.m_properties = CreatePropertiesDictionary();
        return this.m_properties.Count;
      }
    }

    /// <summary>
    /// Gets the index into the Values array of the next added property.
    /// </summary>
    public int NextValueIndex
    {
      get { return this.m_nextValueIndex; }
    }

    /// <summary>
    /// Enumerates the property names and values for this schema.
    /// </summary>
    /// <param name="values"> The array containing the property values. </param>
    /// <returns> An enumerable collection of property names and values. </returns>
    public IEnumerable<PropertyNameAndValue> EnumeratePropertyNamesAndValues(object[] values)
    {
      if (this.m_properties == null)
        this.m_properties = CreatePropertiesDictionary();
      this.m_parent = null;     // Prevents the properties dictionary from being stolen while an enumeration is in progress.
      return this.m_properties.Select(pair => new PropertyNameAndValue(pair.Key, new PropertyDescriptor(values[pair.Value.Index], pair.Value.Attributes)));
    }

    /// <summary>
    /// Gets the zero-based index of the property with the given name.
    /// </summary>
    /// <param name="name"> The name of the property. </param>
    /// <returns> The zero-based index of the property, or <c>-1</c> if a property with the
    /// given name does not exist. </returns>
    public int GetPropertyIndex(string name)
    {
      return GetPropertyIndexAndAttributes(name).Index;
    }

    /// <summary>
    /// Gets the zero-based index of the property with the given name and the attributes
    /// associated with the property.
    /// </summary>
    /// <param name="name"> The name of the property. </param>
    /// <returns> A structure containing the zero-based index of the property, or <c>-1</c> if a property with the
    /// given name does not exist. </returns>
    public SchemaProperty GetPropertyIndexAndAttributes(string name)
    {
      if (this.m_properties == null)
        this.m_properties = CreatePropertiesDictionary();
      SchemaProperty propertyInfo;
      if (this.m_properties.TryGetValue(name, out propertyInfo) == false)
        return SchemaProperty.Undefined;
      return propertyInfo;
    }

    /// <summary>
    /// Adds a property to the schema.
    /// </summary>
    /// <param name="name"> The name of the property to add. </param>
    /// <param name="attributes"> The property attributes. </param>
    /// <returns> A new schema with the extra property. </returns>
    public HiddenClassSchema AddProperty(string name, PropertyAttributes attributes)
    {
      // Package the name and attributes into a struct.
      var transitionInfo = new TransitionInfo { Name = name, Attributes = attributes };

      // Check if there is a transition to the schema already.
      HiddenClassSchema newSchema = null;
      if (this.m_addTransitions != null)
        this.m_addTransitions.TryGetValue(transitionInfo, out newSchema);

      if (newSchema == null)
      {
        if (this.m_parent == null)
        {
          // Create a new schema based on this one.  A complete copy must be made of the properties hashtable.
          var properties = new Dictionary<string, SchemaProperty>(this.m_properties)
            {
              {name, new SchemaProperty(this.NextValueIndex, attributes)}
            };
          newSchema = new HiddenClassSchema(properties, this.NextValueIndex + 1, this, transitionInfo);
        }
        else
        {
          // Create a new schema based on this one.  The properties hashtable is "given
          // away" so a copy does not have to be made.
          if (this.m_properties == null)
            this.m_properties = CreatePropertiesDictionary();
          this.m_properties.Add(name, new SchemaProperty(this.NextValueIndex, attributes));
          newSchema = new HiddenClassSchema(this.m_properties, this.NextValueIndex + 1, this, transitionInfo);
          this.m_properties = null;
        }


        // Add a transition to the new schema.
        if (this.m_addTransitions == null)
          this.m_addTransitions = new Dictionary<TransitionInfo, HiddenClassSchema>(1);
        this.m_addTransitions.Add(transitionInfo, newSchema);
      }

      return newSchema;
    }

    /// <summary>
    /// Deletes a property from the schema.
    /// </summary>
    /// <param name="name"> The name of the property to delete. </param>
    /// <returns> A new schema without the property. </returns>
    public HiddenClassSchema DeleteProperty(string name)
    {
      // Check if there is a transition to the schema already.
      HiddenClassSchema newSchema = null;
      if (this.m_deleteTransitions != null)
        this.m_deleteTransitions.TryGetValue(name, out newSchema);

      if (newSchema == null)
      {
        // Create a new schema based on this one.
        var properties = this.m_properties == null ? CreatePropertiesDictionary() : new Dictionary<string, SchemaProperty>(this.m_properties);
        if (properties.Remove(name) == false)
          throw new InvalidOperationException(string.Format("The property '{0}' does not exist.", name));
        newSchema = new HiddenClassSchema(properties, this.NextValueIndex);

        // Add a transition to the new schema.
        if (this.m_deleteTransitions == null)
          this.m_deleteTransitions = new Dictionary<string, HiddenClassSchema>(1);
        this.m_deleteTransitions.Add(name, newSchema);
      }

      return newSchema;
    }

    /// <summary>
    /// Modifies the attributes for a property in the schema.
    /// </summary>
    /// <param name="name"> The name of the property to modify. </param>
    /// <param name="attributes"> The new attributes. </param>
    /// <returns> A new schema with the modified property. </returns>
    public HiddenClassSchema SetPropertyAttributes(string name, PropertyAttributes attributes)
    {
      // Package the name and attributes into a struct.
      var transitionInfo = new TransitionInfo { Name = name, Attributes = attributes };

      // Check if there is a transition to the schema already.
      HiddenClassSchema newSchema = null;
      if (this.m_modifyTransitions != null)
        this.m_modifyTransitions.TryGetValue(transitionInfo, out newSchema);

      if (newSchema == null)
      {
        // Create the properties dictionary if it hasn't already been created.
        if (this.m_properties == null)
          this.m_properties = CreatePropertiesDictionary();

        // Check the attributes differ from the existing attributes.
        SchemaProperty propertyInfo;
        if (this.m_properties.TryGetValue(name, out propertyInfo) == false)
          throw new InvalidOperationException(string.Format("The property '{0}' does not exist.", name));
        if (attributes == propertyInfo.Attributes)
          return this;

        // Create a new schema based on this one.
        var properties = new Dictionary<string, SchemaProperty>(this.m_properties);
        properties[name] = new SchemaProperty(propertyInfo.Index, attributes);
        newSchema = new HiddenClassSchema(properties, this.NextValueIndex);

        // Add a transition to the new schema.
        if (this.m_modifyTransitions == null)
          this.m_modifyTransitions = new Dictionary<TransitionInfo, HiddenClassSchema>(1);
        this.m_modifyTransitions.Add(transitionInfo, newSchema);
      }

      return newSchema;
    }

    /// <summary>
    /// Creates the properties dictionary.
    /// </summary>
    private Dictionary<string, SchemaProperty> CreatePropertiesDictionary()
    {
      // Search up the tree until a schema is found with a populated properties hashtable, 
      // while keeping a list of the transitions.

      var addTransitions = new Stack<KeyValuePair<string, SchemaProperty>>();
      var node = this;
      while (node != null)
      {
        if (node.m_properties == null)
        {
          // The schema is the same as the parent schema except with the addition of a single
          // property.
          addTransitions.Push(new KeyValuePair<string, SchemaProperty>(
              node.m_addPropertyTransitionInfo.Name,
              new SchemaProperty(node.NextValueIndex - 1, node.m_addPropertyTransitionInfo.Attributes)));
        }
        else
        {
          // The schema has a populated properties hashtable - we can stop here.
          break;
        }
        node = node.m_parent;
      }
      if (node == null)
        throw new InvalidOperationException("Internal error: no route to a populated schema was found.");

      // Add the properties to the hashtable in order.
      var result = new Dictionary<string, SchemaProperty>(node.m_properties);
      while (addTransitions.Count > 0)
      {
        var keyValuePair = addTransitions.Pop();
        result.Add(keyValuePair.Key, keyValuePair.Value);
      }
      return result;
    }
  }

}
