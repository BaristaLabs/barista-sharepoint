namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint;
  using System;

  [Serializable]
  public class SPRoleAssignmentCollectionConstructor : ClrFunction
  {
    public SPRoleAssignmentCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPRoleAssignmentCollection", new SPRoleAssignmentCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPRoleAssignmentCollectionInstance Construct()
    {
      return new SPRoleAssignmentCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPRoleAssignmentCollectionInstance : ObjectInstance
  {
    private readonly SPRoleAssignmentCollection m_roleAssignmentCollection;

    public SPRoleAssignmentCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPRoleAssignmentCollectionInstance(ObjectInstance prototype, SPRoleAssignmentCollection roleAssignmentCollection)
      : this(prototype)
    {
      if (roleAssignmentCollection == null)
        throw new ArgumentNullException("roleAssignmentCollection");

      m_roleAssignmentCollection = roleAssignmentCollection;
    }

    public SPRoleAssignmentCollection SPRoleAssignmentCollection
    {
      get { return m_roleAssignmentCollection; }
    }

    [JSProperty(Name = "count")]
    public object Count
    {
      get
      {
          try
          {
              return m_roleAssignmentCollection.Count;
          }
          catch(Exception)
          {
              return Undefined.Value;
          }
      }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_roleAssignmentCollection.Id);
      }
    }

    [JSFunction(Name = "addPrincipal")]
    public void Add(SPPrincipalInstance principal)
    {
      if (principal == null)
        return;

      m_roleAssignmentCollection.Add(principal.SPPrincipal);
    }

    [JSFunction(Name = "addRoleAssignment")]
    public void Add(SPRoleAssignmentInstance roleAssignment)
    {
      if (roleAssignment == null)
        return;

      m_roleAssignmentCollection.Add(roleAssignment.SPRoleAssignment);
    }

    [JSFunction(Name = "addToCurrentScopeOnly")]
    public void AddToCurrentScopeOnly(SPRoleAssignmentInstance roleAssignment)
    {
      if (roleAssignment == null)
        return;

      m_roleAssignmentCollection.AddToCurrentScopeOnly(roleAssignment.SPRoleAssignment);
    }

    [JSFunction(Name = "getAssignmentByPrincipal")]
    public SPRoleAssignmentInstance GetAssignmentByPrincipal(SPPrincipalInstance principal)
    {
      if (principal == null)
        return null;

      var result = m_roleAssignmentCollection.GetAssignmentByPrincipal(principal.SPPrincipal);
      return new SPRoleAssignmentInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "getParentSecurableObject")]
    public SPSecurableObjectInstance GetParentSecurableObject()
    {
      return new SPSecurableObjectInstance(this.Engine)
      {
        SecurableObject = m_roleAssignmentCollection.ParentSecurableObject
      };
    }

    [JSFunction(Name = "getRoleAssignmentByIndex")]
    public SPRoleAssignmentInstance GetRoleAssignmentById(int index)
    {
      var result = m_roleAssignmentCollection[index];
      return new SPRoleAssignmentInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "getXml")]
    public string GetXml()
    {
      return m_roleAssignmentCollection.Xml;
    }

    [JSFunction(Name = "removePrincipal")]
    public void RemovePrincipal(SPPrincipalInstance principal)
    {
      if (principal == null)
        return;

      m_roleAssignmentCollection.Remove(principal.SPPrincipal);
    }

    [JSFunction(Name = "removeByIndex")]
    public void RemoveByIndex(int index)
    {
      m_roleAssignmentCollection.Remove(index);
    }

    [JSFunction(Name = "removeById")]
    public void RemoveById(int id)
    {
      m_roleAssignmentCollection.RemoveById(id);
    }

    [JSFunction(Name = "removeFromCurrentScopeOnly")]
    public void RemoveFromCurrentScopeOnly(SPPrincipalInstance principal)
    {
      if (principal == null)
        return;

      m_roleAssignmentCollection.RemoveFromCurrentScopeOnly(principal.SPPrincipal);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var roleAssignment in m_roleAssignmentCollection.OfType<SPRoleAssignment>())
      {
        ArrayInstance.Push(result, new SPRoleAssignmentInstance(this.Engine.Object.InstancePrototype, roleAssignment));
      }
      return result;
    }
  }
}
