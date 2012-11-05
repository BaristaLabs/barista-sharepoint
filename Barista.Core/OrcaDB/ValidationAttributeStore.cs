namespace OFS.OrcaDB.Core
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.ComponentModel.DataAnnotations;
  using System.Globalization;
  using System.Linq;

  internal class ValidationAttributeStore
  {
    private static ValidationAttributeStore _singleton;

    private Dictionary<Type, ValidationAttributeStore.TypeStoreItem> _typeStoreItems;

    internal static ValidationAttributeStore Instance
    {
      get
      {
        return ValidationAttributeStore._singleton;
      }
    }

    static ValidationAttributeStore()
    {
      ValidationAttributeStore._singleton = new ValidationAttributeStore();
    }

    public ValidationAttributeStore()
    {
      this._typeStoreItems = new Dictionary<Type, ValidationAttributeStore.TypeStoreItem>();
    }

    private static void EnsureValidationContext(ValidationContext validationContext)
    {
      if (validationContext != null)
      {
        return;
      }
      else
      {
        throw new ArgumentNullException("validationContext");
      }
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
      ValidationAttributeStore.TypeStoreItem typeStoreItem;
      if (type != null)
      {
        lock (this._typeStoreItems)
        {
          ValidationAttributeStore.TypeStoreItem typeStoreItem1 = null;
          if (!this._typeStoreItems.TryGetValue(type, out typeStoreItem1))
          {
            IEnumerable<Attribute> attributes = TypeDescriptor.GetAttributes(type).Cast<Attribute>();
            typeStoreItem1 = new ValidationAttributeStore.TypeStoreItem(type, attributes);
            this._typeStoreItems[type] = typeStoreItem1;
          }
          typeStoreItem = typeStoreItem1;
        }
        return typeStoreItem;
      }
      else
      {
        throw new ArgumentNullException("type");
      }
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
      ValidationAttributeStore.PropertyStoreItem propertyStoreItem = null;
      return typeStoreItem.TryGetPropertyStoreItem(validationContext.MemberName, out propertyStoreItem);
    }

    private class PropertyStoreItem : ValidationAttributeStore.StoreItem
    {
      private Type _propertyType;

      internal Type PropertyType
      {
        get
        {
          return this._propertyType;
        }
      }

      internal PropertyStoreItem(Type propertyType, IEnumerable<Attribute> attributes)
        : base(attributes)
      {
        this._propertyType = propertyType;
      }
    }

    private abstract class StoreItem
    {
      private static IEnumerable<ValidationAttribute> _emptyValidationAttributeEnumerable;

      private IEnumerable<ValidationAttribute> _validationAttributes;

      internal IEnumerable<ValidationAttribute> ValidationAttributes
      {
        get
        {
          return this._validationAttributes;
        }
      }

      static StoreItem()
      {
        ValidationAttributeStore.StoreItem._emptyValidationAttributeEnumerable = new ValidationAttribute[0];
      }

      internal StoreItem(IEnumerable<Attribute> attributes)
      {
        this._validationAttributes = attributes.OfType<ValidationAttribute>();
      }
    }

    private class TypeStoreItem : ValidationAttributeStore.StoreItem
    {
      private Dictionary<string, ValidationAttributeStore.PropertyStoreItem> _propertyStoreItems;

      private object _syncRoot;

      private Type _type;

      internal TypeStoreItem(Type type, IEnumerable<Attribute> attributes)
        : base(attributes)
      {
        this._syncRoot = new object();
        this._type = type;
      }

      private Dictionary<string, ValidationAttributeStore.PropertyStoreItem> CreatePropertyStoreItems()
      {
        Dictionary<string, ValidationAttributeStore.PropertyStoreItem> strs = new Dictionary<string, ValidationAttributeStore.PropertyStoreItem>();
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this._type);
        foreach (PropertyDescriptor property in properties)
        {
          ValidationAttributeStore.PropertyStoreItem propertyStoreItem = new ValidationAttributeStore.PropertyStoreItem(property.PropertyType, ValidationAttributeStore.TypeStoreItem.GetExplicitAttributes(property).Cast<Attribute>());
          strs[property.Name] = propertyStoreItem;
        }
        return strs;
      }

      public static AttributeCollection GetExplicitAttributes(PropertyDescriptor propertyDescriptor)
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
        if (flag)
        {
          return new AttributeCollection(attributes.ToArray());
        }
        else
        {
          return propertyDescriptor.Attributes;
        }
      }

      internal ValidationAttributeStore.PropertyStoreItem GetPropertyStoreItem(string propertyName)
      {
        ValidationAttributeStore.PropertyStoreItem propertyStoreItem = null;
        if (this.TryGetPropertyStoreItem(propertyName, out propertyStoreItem))
        {
          return propertyStoreItem;
        }
        else
        {
          string message = String.Format("Unknown Property: {0} on {1}", propertyName, this._type.Name);
          throw new ArgumentException(message, "propertyName");
        }
      }

      internal bool TryGetPropertyStoreItem(string propertyName, out ValidationAttributeStore.PropertyStoreItem item)
      {
        if (!string.IsNullOrEmpty(propertyName))
        {
          if (this._propertyStoreItems == null)
          {
            lock (this._syncRoot)
            {
              if (this._propertyStoreItems == null)
              {
                this._propertyStoreItems = this.CreatePropertyStoreItems();
              }
            }
          }
          if (this._propertyStoreItems.TryGetValue(propertyName, out item))
          {
            return true;
          }
          else
          {
            return false;
          }
        }
        else
        {
          throw new ArgumentNullException("propertyName");
        }
      }
    }
  }
}
