namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.SharePoint;
  using System;
  using Barista.Extensions;

  [Serializable]
  public class SPModerationInformationConstructor : ClrFunction
  {
    public SPModerationInformationConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPModerationInformation", new SPModerationInformationInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPModerationInformationInstance Construct()
    {
      return new SPModerationInformationInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPModerationInformationInstance : ObjectInstance
  {
    private readonly SPModerationInformation m_moderationInformation;

    public SPModerationInformationInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPModerationInformationInstance(ObjectInstance prototype, SPModerationInformation moderationInformation)
      : this(prototype)
    {
      if (moderationInformation == null)
        throw new ArgumentNullException("moderationInformation");

      m_moderationInformation = moderationInformation;
    }

    public SPModerationInformation SPModerationInformation
    {
      get { return m_moderationInformation; }
    }

    [JSProperty(Name="comment")]
    public string Comment
    {
      get { return m_moderationInformation.Comment; }
      set { m_moderationInformation.Comment = value; }
    }

    [JSProperty(Name = "status")]
    [JSDoc("Gets or sets the moderation status. Possible values are: Approved, Denied, Draft, Pending, Scheduled")]
    public string Status
    {
      get { return m_moderationInformation.Status.ToString(); }
      set
      {
        SPModerationStatusType status;
        if (value.TryParseEnum(true, out status))
          m_moderationInformation.Status = status;
      }
    }
  }
}
