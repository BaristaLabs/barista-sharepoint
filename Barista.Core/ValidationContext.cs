namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.Design;
  using System.Globalization;
  using System.Linq;
  using System.Text;

  public sealed class ValidationContext : IServiceProvider
  {
    private string _displayName;

    private Dictionary<object, object> _items;

    private string _memberName;

    private object _objectInstance;

    private IServiceContainer _serviceContainer;

    private Func<Type, object> _serviceProvider;

    public string DisplayName
    {
      get
      {
        if (string.IsNullOrEmpty(this._displayName))
        {
          this._displayName = this.GetDisplayName();
          if (string.IsNullOrEmpty(this._displayName))
          {
            this._displayName = this.MemberName;
            if (string.IsNullOrEmpty(this._displayName))
            {
              this._displayName = this.ObjectType.Name;
            }
          }
        }
        return this._displayName;
      }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          this._displayName = value;
          return;
        }
        else
        {
          throw new ArgumentNullException("value");
        }
      }
    }

    public IDictionary<object, object> Items
    {
      get
      {
        return this._items;
      }
    }

    public string MemberName
    {
      get
      {
        return this._memberName;
      }
      set
      {
        this._memberName = value;
      }
    }

    public object ObjectInstance
    {
      get
      {
        return this._objectInstance;
      }
    }

    public Type ObjectType
    {
      get
      {
        return this.ObjectInstance.GetType();
      }
    }

    public IServiceContainer ServiceContainer
    {
      get
      {
        if (this._serviceContainer == null)
        {
          this._serviceContainer = new ValidationContext.ValidationContextServiceContainer();
        }
        return this._serviceContainer;
      }
    }

    public ValidationContext(object instance)
      : this(instance, null, null)
    {
    }

    public ValidationContext(object instance, IDictionary<object, object> items)
      : this(instance, null, items)
    {
    }

    public ValidationContext(object instance, IServiceProvider serviceProvider, IDictionary<object, object> items)
    {
      Func<Type, object> func = null;
      if (instance != null)
      {
        if (serviceProvider != null)
        {
          ValidationContext validationContext = this;
          if (func == null)
          {
            func = (Type serviceType) => serviceProvider.GetService(serviceType);
          }
          validationContext.InitializeServiceProvider(func);
        }
        IServiceContainer serviceContainer = serviceProvider as IServiceContainer;
        if (serviceContainer == null)
        {
          this._serviceContainer = new ValidationContext.ValidationContextServiceContainer();
        }
        else
        {
          this._serviceContainer = new ValidationContext.ValidationContextServiceContainer(serviceContainer);
        }
        if (items == null)
        {
          this._items = new Dictionary<object, object>();
        }
        else
        {
          this._items = new Dictionary<object, object>(items);
        }
        this._objectInstance = instance;
        return;
      }
      else
      {
        throw new ArgumentNullException("instance");
      }
    }

    private string GetDisplayName()
    {
      string name = null;
      ValidationAttributeStore instance = ValidationAttributeStore.Instance;
      string str = name;
      string memberName = str;
      if (str == null)
      {
        memberName = this.MemberName;
      }
      return memberName;
    }

    public object GetService(Type serviceType)
    {
      object service = null;
      if (this._serviceContainer != null)
      {
        service = this._serviceContainer.GetService(serviceType);
      }
      if (service == null && this._serviceProvider != null)
      {
        service = this._serviceProvider(serviceType);
      }
      return service;
    }

    public void InitializeServiceProvider(Func<Type, object> serviceProvider)
    {
      this._serviceProvider = serviceProvider;
    }

    private class ValidationContextServiceContainer : IServiceContainer, IServiceProvider
    {
      private readonly object _lock;

      private IServiceContainer _parentContainer;

      private Dictionary<Type, object> _services;

      internal ValidationContextServiceContainer()
      {
        this._services = new Dictionary<Type, object>();
        this._lock = new object();
      }

      internal ValidationContextServiceContainer(IServiceContainer parentContainer)
      {
        this._services = new Dictionary<Type, object>();
        this._lock = new object();
        this._parentContainer = parentContainer;
      }

      public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
      {
        if (!promote || this._parentContainer == null)
        {
          lock (this._lock)
          {
            if (!this._services.ContainsKey(serviceType))
            {
              this._services.Add(serviceType, callback);
            }
            else
            {
              object[] objArray = new object[1];
              objArray[0] = serviceType;
              throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The Item Already Exists: {0}", objArray), "serviceType");
            }
          }
          return;
        }
        else
        {
          this._parentContainer.AddService(serviceType, callback, promote);
          return;
        }
      }

      public void AddService(Type serviceType, ServiceCreatorCallback callback)
      {
        this.AddService(serviceType, callback, true);
      }

      public void AddService(Type serviceType, object serviceInstance, bool promote)
      {
        if (!promote || this._parentContainer == null)
        {
          lock (this._lock)
          {
            if (!this._services.ContainsKey(serviceType))
            {
              this._services.Add(serviceType, serviceInstance);
            }
            else
            {
              object[] objArray = new object[1];
              objArray[0] = serviceType;
              throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The Item already exists: {0}", objArray), "serviceType");
            }
          }
          return;
        }
        else
        {
          this._parentContainer.AddService(serviceType, serviceInstance, promote);
          return;
        }
      }

      public void AddService(Type serviceType, object serviceInstance)
      {
        this.AddService(serviceType, serviceInstance, true);
      }

      public object GetService(Type serviceType)
      {
        if (serviceType != null)
        {
          object service = null;
          this._services.TryGetValue(serviceType, out service);
          if (service == null && this._parentContainer != null)
          {
            service = this._parentContainer.GetService(serviceType);
          }
          ServiceCreatorCallback serviceCreatorCallback = service as ServiceCreatorCallback;
          if (serviceCreatorCallback != null)
          {
            service = serviceCreatorCallback(this, serviceType);
          }
          return service;
        }
        else
        {
          throw new ArgumentNullException("serviceType");
        }
      }

      public void RemoveService(Type serviceType, bool promote)
      {
        lock (this._lock)
        {
          if (this._services.ContainsKey(serviceType))
          {
            this._services.Remove(serviceType);
          }
        }
        if (promote && this._parentContainer != null)
        {
          this._parentContainer.RemoveService(serviceType);
        }
      }

      public void RemoveService(Type serviceType)
      {
        this.RemoveService(serviceType, true);
      }
    }
  }

}
