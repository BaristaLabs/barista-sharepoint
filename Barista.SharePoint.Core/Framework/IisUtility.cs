namespace Barista.SharePoint.Framework
{
  using System.Web.Hosting;

  public static class IisUtility
  {
    private static string s_currentInstanceId;

    public static string CurrentInstanceId
    {
      get
      {
        if (s_currentInstanceId == null)
        {
          string applicationId = HostingEnvironment.ApplicationID;
          if ((applicationId != null) && (applicationId.Length > 10))
          {
            s_currentInstanceId = applicationId.Substring(10, applicationId.IndexOf('/', 10) - 10);
          }
          else
          {
            s_currentInstanceId = "-1";
          }
        }
        return s_currentInstanceId;
      }
    }
  }
}
