namespace Barista.TeamFoundationServer.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.TeamFoundation.WorkItemTracking.Client;
  using System;

  [Serializable]
  public class ProjectConstructor : ClrFunction
  {
    public ProjectConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Project", new ProjectInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ProjectInstance Construct()
    {
      return new ProjectInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ProjectInstance : ObjectInstance
  {
    private readonly Project m_project;

    public ProjectInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ProjectInstance(ObjectInstance prototype, Project project)
      : this(prototype)
    {
      if (project == null)
        throw new ArgumentNullException("project");

      m_project = project;
    }

    public Project Project
    {
      get { return m_project; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_project.Name; }
    }

    [JSProperty(Name = "guid")]
    public GuidInstance Guid
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_project.Guid); }
    }

    [JSProperty(Name = "uri")]
    public UriInstance Uri
    {
      get { return new UriInstance(this.Engine.Object.InstancePrototype, m_project.Uri); }
    }
  }
}
