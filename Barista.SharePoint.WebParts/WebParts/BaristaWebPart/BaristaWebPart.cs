namespace Barista.SharePoint.WebParts.WebParts.BaristaWebPart
{
  using Barista.SharePoint.Extensions;
  using Barista.SharePoint.Services;
  using Microsoft.SharePoint;
  using System;
  using System.ComponentModel;
  using System.Web;
  using System.Web.UI;
  using System.Web.UI.WebControls.WebParts;

  [ToolboxItemAttribute(false)]
  public class BaristaWebPart : WebPart
  {
    [WebBrowsable(true)]
    [WebDisplayName("Code to Execute")]
    [WebDescription("Contains the JavaScript which is evaluated by Barista.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue("")]
    public string Code
    {
      get;
      set;
    }

    protected override void CreateChildControls()
    {

      if (String.IsNullOrEmpty(this.Code))
        return;

      if (String.IsNullOrEmpty(this.Code.Trim()))
        return;

      BaristaHelper.EnsureExecutionInTrustedLocation();

      var codeToExecute = Tamp(this.Code);

      BaristaServiceClient client = new BaristaServiceClient(SPServiceContext.Current);

      var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
      request.Code = codeToExecute;

      request.SetExtendedPropertiesFromCurrentSPContext();

      var result = client.Eval(request);
      var resultText = System.Text.Encoding.UTF8.GetString(result.Content);

      //TODO: Based on the content type of the result, emit the contents differently.
      LiteralControl cntrl = new LiteralControl(resultText);
      Controls.Add(cntrl);
    }

    /// <summary>
    /// Tamps the ground coffee. E.g. Parses the code and makes it ready to be executed (brewed).
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    private string Tamp(string code)
    {
      //If the code looks like a uri, attempt to retrieve a code file and use the contents of that file as the code.
      if (Uri.IsWellFormedUriString(code, UriKind.RelativeOrAbsolute))
      {
        Uri codeUri;
        if (Uri.TryCreate(code, UriKind.RelativeOrAbsolute, out codeUri))
        {

          bool isHiveFile;
          String codeFromfile;
          if (SPHelper.TryGetSPFileAsString(code, out codeFromfile, out isHiveFile))
          {
            if (isHiveFile == false)
            {
              string lockDownMode = SPContext.Current.Web.GetProperty("BaristaLockdownMode") as string;
              if (String.IsNullOrEmpty(lockDownMode) == false && lockDownMode.ToLowerInvariant() == "BaristaContentLibraryOnly")
              {
                //TODO: implement this.
              }
            }

            code = codeFromfile;
          }
        }
      }

      //Replace any tokens in the code.
      code = SPHelper.ReplaceTokens(code);

      return code;
    }
  }
}
