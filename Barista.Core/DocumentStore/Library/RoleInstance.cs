namespace Barista.DocumentStore.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Linq;

  [Serializable]
  public class RoleInstance : ObjectInstance
  {
    private readonly Role m_role;

    public RoleInstance(ScriptEngine engine, Role role)
      : base(engine)
    {
      if (role == null)
        throw new ArgumentNullException("role");

      m_role = role;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_role.Name; }
      set { m_role.Name = value; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_role.Description; }
      set { m_role.Description = value; }
    }

    [JSProperty(Name = "order")]
    public int Order
    {
      get { return m_role.Order; }
      set { m_role.Order = value; }
    }

    [JSProperty(Name = "basePermissions")]
    [JSDoc("ternPropertyType", "[string]")]
    public ArrayInstance BasePermissions
    {
      get
      {
// ReSharper disable CoVariantArrayConversion
        var result = this.Engine.Array.Construct(m_role.BasePermissions.Select(bp => bp).ToArray());
// ReSharper restore CoVariantArrayConversion
        return result;
      }
      set
      {
        m_role.BasePermissions = value.ElementValues.OfType<string>().ToList();
      }
    }
  }
}
