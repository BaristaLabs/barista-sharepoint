namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.ComponentModel.DataAnnotations;
  using System.Linq;

  internal class ValidationAttributeStore
  {
    private static ValidationAttributeStore s_singleton;

    private readonly Dictionary<Type, ValidationAttributeStore.TypeStoreItem> m_typeStoreItems;

    internal static ValidationAttributeStore Instance
    {
      get
      {
        return ValidationAttributeStore.s_singleton;
      }
    }

    static ValidationAttributeStore()
    {
      ValidationAttributeStore.s_singleton = new ValidationAttributeStore();
    }

    public ValidationAttributeStore()
    {
      this.m_typeStoreItems = new Dictionary<Type, ValidationAttributeStore.TypeStoreItem>();
    }

    private static void EnsureValidationContext(ValidationContext validationContext)
    {
      if (validationContext == null)
        throw new ArgumentNullException("validationContext");
      
    }

    internal Type GetPropertyType(ValidationContext validationContext)
    {
      ValidationAttributeStore.EnsureValidationContext(validationContext);
      ValidationAttributeStore.TypeStoreItem typeStoreItem = this.GetTypeStoreItem(validationContext.ObjectType);
      ValidationAttributeStore.PropertyStoreItem propertyStoreItem = typeStoreItem.GetPropertyStoreItem(validationContext.MemberName);
      return propertyStoreItem.PropertyType;
    }

    internal IEnumerable<ValidationAttribute> GetPropertyValidationAttributes(ValidationContext validationContext)
    {
      ValidationAttributeStore.EnsureValidationContext(validationContext);
      ValidationAttributeStore.TypeStoreItem typeStoreItem = this.GetTypeStoreItem(validationContext.ObjectType);
      ValidationAttributeStore.PropertyStoreItem propertyStoreItem = typeStoreItem.GetPropertyStoreItem(validationContext.MemberName);
      return propertyStoreItem.ValidationAttributes;
    }

    private ValidationAttributeStore.TypeStoreItem GetTypeStoreItem(Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      ValidationAttributeStore.TypeStoreItem typeStoreItem;
      lock (this.m_typeStoreItems)
      {
        ValidationAttributeStore.TypeStoreItem typeStoreItem1;
        if (!this.m_typeStoreItems.TryGetValue(type, out typeStoreItem1))
        {
          IEnumerable<Attribute> attributes = TypeDescriptor.GetAttributes(type).Cast<Attribute>();
          typeStoreItem1 = new ValidationAttributeStore.TypeStoreItem(type, attributes);
          this.m_typeStoreItems[type] = typeStoreItem1;
        }
        typeStoreItem = typeStoreItem1;
      }
      return typeStoreItem;
    }

    internal IEnumerable<ValidationAttribute> GetTypeValidationAttributes(ValidationContext validationContext)
    {
      ValidationAttributeStore.EnsureValidationContext(validationContext);
      ValidationAttributeStore.TypeStoreItem typeStoreItem = this.GetTypeStoreItem(validationContext.ObjectType);
      return typeStoreItem.ValidationAttributes;
    }

    internal bool IsPropertyContext(ValidationContext validationContext)
    {
      ValidationAttributeStore.EnsureValidationContext(validationContext);
      ValidationAttributeStore.TypeStoreItem typeStoreItem = this.GetTypeStoreItem(validationContext.ObjectType);
      ValidationAttributeStore.PropertyStoreItem propertyStoreItem;
      return typeStoreItem.TryGetPropertyStoreItem(validationContext.MemberName, out propertyStoreItem);
    }

    private class PropertyStoreItem : ValidationAttributeStore.StoreItem
    {
      private readonly Type m_propertyType;

      internal Type PropertyType
      {
        get
        {
          return this.m_propertyType;
        }
      }

      internal PropertyStoreItem(Type propertyType, IEnumerable<Attribute> attributes)
        : base(attributes)
      {
        this.m_propertyType = propertyType;
      }
    }

    private abstract class StoreItem
    {
      private readonly IEnumerable<ValidationAttribute> m_validationAttributes;

      internal IEnumerable<ValidationAttribute> ValidationAttributes
      {
        get
        {
          return this.m_validationAttributes;
        }
      }

      protected StoreItem(IEnumerable<Attribute> attributes)
      {
        this.m_validationAttributes = attributes.OfType<ValidationAttribute>();
      }
    }

    private class TypeStoreItem : ValidationAttributeStore.StoreItem
    {
      private volatile Dictionary<string, ValidationAttributeStore.PropertyStoreItem> m_propertyStoreItems;

      private readonly object m_syncRoot;

      private readonly Type m_type;

      internal TypeStoreItem(Type type, IEnumerable<Attribute> attributes)
        : base(attributes)
      {
        this.m_syncRoot = new object();
        this.m_type = type;
      }

      private Dictionary<string, ValidationAttributeStore.PropertyStoreItem> CreatePropertyStoreItems()
      {
        Dictionary<string, ValidationAttributeStore.PropertyStoreItem> strs = new Dictionary<string, ValidationAttributeStore.PropertyStoreItem>();
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this.m_type);
        foreach (PropertyDescriptor property in properties)
        {
          ValidationAttributeStore.PropertyStoreItem propertyStoreItem = new ValidationAttributeStore.PropertyStoreItem(property.PropertyType, ValidationAttributeStore.TypeStoreItem.GetExplicitAttributes(property).Cast<Attribute>());
          strs[property.Name] = propertyStoreItem;
        }
        return strs;
      }

      private static AttributeCollection GetExplicitAttributes(PropertyDescriptor propertyDescriptor)
      {
        List<Attribute> attributes = new List<Attribute>(propertyDescriptor.Attributes.Cast<Attribute>());
        IEnumerable<Attribute> attributes1 = TypeDescriptor.GetAttributes(propertyDescriptor.PropertyType).Cast<Attribute>();
        bool flag = false;
        foreach (Attribute attribute in attributes1)
        {
          for (int i = attributes.Count - 1; i >= 0; i--)
          {
            if (object.ReferenceEquals(attribute, attributes[i]))
            {
              attributes.RemoveAt(i);
              flag = true;
            }
          }
        }
        return flag 
          ? new AttributeCollection(attributes.ToArray())
          : propertyDescriptor.Attributes;
      }

      internal ValidationAttributeStore.PropertyStoreItem GetPropertyStoreItem(string propertyName)
      {
        ValidationAttributeStore.PropertyStoreItem propertyStoreItem;
        if (this.TryGetPropertyStoreItem(propertyName, out propertyStoreItem))
        {
          return propertyStoreItem;
        }

        var message = String.Format("Unknown Property: {0} on {1}", propertyName, this.m_type.Name);
        throw new ArgumentException(message, "propertyName");
      }

      internal bool TryGetPropertyStoreItem(string propertyName, out ValidationAttributeStore.PropertyStoreItem item)
      {
        if (!string.IsNullOrEmpty(propertyName))
        {
          if (this.m_propertyStoreItems == null)
          {
            lock (this.m_syncRoot)
            {
              if (this.m_propertyStoreItems == null)
              {
                this.m_propertyStoreItems = this.CreatePropertyStoreItems();
              }
            }
          }
          return this.m_propertyStoreItems.TryGetValue(propertyName, out item);
        }

        throw new ArgumentNullException("propertyName");
      }
    }
  }
}
