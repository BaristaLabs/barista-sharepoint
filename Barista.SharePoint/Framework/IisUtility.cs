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
          string applicationID = HostingEnvironment.ApplicationID;
          if ((applicationID != null) && (applicationID.Length > 10))
          {
            s_currentInstanceId = applicationID.Substring(10, applicationID.IndexOf('/', 10) - 10);
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
