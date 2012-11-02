namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  public class SPContentTypeIdConstructor : ClrFunction
  {
    public SPContentTypeIdConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPContentTypeId", new SPContentTypeInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPContentTypeIdInstance Construct(string contentTypeId)
    {
      var spContentTypeId = new SPContentTypeId(contentTypeId);

      return new SPContentTypeIdInstance(this.InstancePrototype, spContentTypeId);
    }

    public SPContentTypeIdInstance Construct(SPContentTypeId contentTypeId)
    {
      if (contentTypeId == null)
        throw new ArgumentNullException("contentTypeId");

      return new SPContentTypeIdInstance(this.InstancePrototype, contentTypeId);
    }
  }

  public class SPContentTypeIdInstance : ObjectInstance
  {
    private SPContentTypeId m_contentTypeId;

    public SPContentTypeIdInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPContentTypeIdInstance(ObjectInstance prototype, SPContentTypeId contentTypeId)
      : this(prototype)
    {
      this.m_contentTypeId = contentTypeId;
    }

    internal SPContentTypeId ContentTypeId
    {
      get { return m_contentTypeId; }
    }

    #region Functions
    [JSFunction(Name = "getParent")]
    public SPContentTypeIdInstance GetParent()
    {
      return new SPContentTypeIdInstance(this.Engine.Object.InstancePrototype, m_contentTypeId.Parent);
    }

    [JSFunction(Name = "isParentOf")]
    public bool IsParentOf(object id)
    {
      if (id is string)
        return m_contentTypeId.IsParentOf(new SPContentTypeId(id as string));
      else if (id is SPContentTypeIdInstance)
        return m_contentTypeId.IsParentOf((id as SPContentTypeIdInstance).m_contentTypeId);
      else
        return false;
    }

    [JSFunction(Name = "isChildOf")]
    public bool IsChildOf(object id)
    {
      if (id is string)
        return m_contentTypeId.IsChildOf(new SPContentTypeId(id as string));
      else if (id is SPContentTypeIdInstance)
        return m_contentTypeId.IsChildOf((id as SPContentTypeIdInstance).m_contentTypeId);
      else
        return false;
    }
    
    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return m_contentTypeId.ToString();
    }
    #endregion
  }
}
