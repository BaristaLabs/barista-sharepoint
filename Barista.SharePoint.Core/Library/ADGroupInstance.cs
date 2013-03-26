namespace Barista.SharePoint.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using Barista.DirectoryServices;

  [Serializable]
  public class ADGroupConstructor : ClrFunction
  {
    public ADGroupConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ADGroup", new ADGroupInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ADGroupInstance Construct()
    {
      ADGroup group = new ADGroup();

      return new ADGroupInstance(this.InstancePrototype, group);
    }

    public ADGroupInstance Construct(ADGroup group)
    {
      if (group == null)
        throw new ArgumentNullException("group");

      return new ADGroupInstance(this.InstancePrototype, group);
    }
  }

  [Serializable]
  public class ADGroupInstance : ObjectInstance
  {
    private readonly ADGroup m_group;

    public ADGroupInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ADGroupInstance(ObjectInstance prototype, ADGroup group)
      : this(prototype)
    {
      this.m_group = group;
    }

    #region Properties
    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_group.Name; }
    }

    [JSProperty(Name = "members")]
    public ArrayInstance Members
    {
      get
      {
        var result = this.Engine.Array.Construct();

        foreach (var memberLogonName in m_group.Members)
        {
          var user = ADHelper.GetADUser(memberLogonName);
          ArrayInstance.Push(result, new ADUserInstance(this.Engine.Object.InstancePrototype, user));
        }

        return result;
      }
    }
    #endregion
  }
}
