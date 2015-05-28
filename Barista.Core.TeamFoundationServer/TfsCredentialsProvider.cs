namespace Barista.SharePoint
{
  using System;
  using System.Net;
  using Microsoft.TeamFoundation.Client;
  using System.Security.Principal;
  using System.Linq;
  using System.Threading;
  using Microsoft.IdentityModel.Claims;
  using Microsoft.IdentityModel.WindowsTokenService;

  public class TfsCredentialsProvider : ICredentialsProvider, IDisposable
  {
    private WindowsImpersonationContext m_ctxt = null;

    public ICredentials GetCredentials(Uri uri, ICredentials failedCredentials)
    {
      //If we're running under Claims authentication, impersonate the thread user
      //by calling the Claims to Windows Token Service and call the remote site using
      //the impersonated credentials. NOTE: The Claims to Windows Token Service must be running.
      
      if (Thread.CurrentPrincipal.Identity is ClaimsIdentity)
      {
        IClaimsIdentity identity = (ClaimsIdentity)System.Threading.Thread.CurrentPrincipal.Identity;
        var firstClaim = identity.Claims.FirstOrDefault(c => c.ClaimType == ClaimTypes.Upn);

        if (firstClaim == null)
          throw new InvalidOperationException("No UPN claim found");

        string upn = firstClaim.Value;

        if (String.IsNullOrEmpty(upn))
          throw new InvalidOperationException("A UPN claim was found, however, the value was empty.");

        var currentIdentity = S4UClient.UpnLogon(upn);
        m_ctxt = currentIdentity.Impersonate();
      }

      return CredentialCache.DefaultNetworkCredentials;
    }

    public void NotifyCredentialsAuthenticated(Uri uri)
    {
      if (m_ctxt == null)
        return;

      m_ctxt.Dispose();
      m_ctxt = null;
    }

    public void Dispose()
    {
      if (m_ctxt != null)
      {
        m_ctxt.Dispose();
        m_ctxt = null;
      }
    }
  }
}
