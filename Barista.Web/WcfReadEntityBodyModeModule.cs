namespace Barista.Web
{
  using System;
  using System.Web;

  public class WcfReadEntityBodyModeModule : IHttpModule
  {
    public void Dispose() { }

    public void Init(HttpApplication context)
    {
      context.BeginRequest += BeginRequest;
    }

    private static void BeginRequest(object sender, EventArgs e)
    {
      //This will force the HttpContext.Request.ReadEntityBody to be "Classic" and will ensure compatibility..    
      var application = (sender as HttpApplication);
      if (application == null)
        return;

#pragma warning disable 168
      var inputStream = application.Request.InputStream;
#pragma warning restore 168
    }
  } 
}
