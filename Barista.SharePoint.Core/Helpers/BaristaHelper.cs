namespace Barista.SharePoint
{
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Newtonsoft.Json.Linq;
  using System;
  using System.Linq;

  public static class BaristaHelper
  {
    /// <summary>
    /// Checks the current context against the trusted locations.
    /// </summary>
    public static void EnsureExecutionInTrustedLocation()
    {
      var currentUri = new Uri(SPContext.Current.Web.Url);

      //CA is always trusted.
      if (SPAdministrationWebApplication.Local.AlternateUrls.Any(u => u != null && u.Uri != null && u.Uri.IsBaseOf(currentUri)))
        return;

      var trusted = false;
      var trustedLocations = Utilities.GetFarmKeyValue("BaristaTrustedLocations");

      if (String.IsNullOrEmpty(trustedLocations) == false)
      {
        var trustedLocationsCollection = JArray.Parse(trustedLocations);
        foreach (var trustedLocation in trustedLocationsCollection.OfType<JObject>())
        {
          var trustedLocationUrl = new Uri(trustedLocation["Url"].ToString(), UriKind.Absolute);
          var trustChildren = trustedLocation["TrustChildren"].ToObject<Boolean>();

          if (trustChildren)
          {
            if (trustedLocationUrl.IsBaseOf(currentUri))
            {
              trusted = true;
              break;
            }
          }
          else
          {
            if (trustedLocationUrl == currentUri)
            {
              trusted = true;
              break;
            }
          }
        }
      }

      if (trusted == false)
        throw new InvalidOperationException("Cannot execute Barista: Current location is not trusted. Add the current location to the trusted Urls in the management section of the Barista service application.");
    }
  }
}
