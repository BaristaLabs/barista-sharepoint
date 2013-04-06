namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  /// <summary>
  /// Wraps the SharePoint UsageInfo object -- obtained by SPSite.usageInfo
  /// </summary>
  [Serializable]
  public class UsageInfoInstance : ObjectInstance
  {
    private SPSite.UsageInfo m_usageInfo;

    public UsageInfoInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public UsageInfoInstance(ObjectInstance prototype, SPSite.UsageInfo usageInfo)
      : this(prototype)
    {
      this.m_usageInfo = usageInfo;
    }

    #region Properties

    [JSProperty(Name = "bandwidth")]
    public double Bandwidth
    {
      get { return m_usageInfo.Bandwidth; }
    }

    [JSProperty(Name = "discussionStorage")]
    public double DiscussionStorage
    {
      get { return m_usageInfo.DiscussionStorage; }
    }

    [JSProperty(Name = "hits")]
    public double Hits
    {
      get { return m_usageInfo.Hits; }
    }

    [JSProperty(Name = "storage")]
    public double Storage
    {
      get { return m_usageInfo.Storage; }
    }

    [JSProperty(Name = "visits")]
    public double Visits
    {
      get { return m_usageInfo.Visits; }
    }
    #endregion
  }
}
