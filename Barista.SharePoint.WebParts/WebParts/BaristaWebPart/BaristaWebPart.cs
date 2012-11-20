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
    public BaristaWebPart()
    {
      this.InstanceMode = BaristaInstanceMode.PerCall;
      this.InstanceName = null;
      this.InstanceInitializationCode = null;
      this.InstanceAbsoluteExpiration = null;
      this.InstanceSlidingExpiration = null;
    }

    [WebBrowsable(true)]
    [WebDisplayName("Instance Mode")]
    [WebDescription("Indicates the instancing mode to use. Currently Allows PerCall and Single")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue(BaristaInstanceMode.PerCall)]
    public BaristaInstanceMode InstanceMode
    {
      get;
      set;
    }

    [WebBrowsable(true)]
    [WebDisplayName("Instance Name")]
    [WebDescription("When Single instancing is used, indicates the instance name.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue("")]
    public string InstanceName
    {
      get;
      set;
    }

    [WebBrowsable(true)]
    [WebDisplayName("Instance Absolute Expiration")]
    [WebDescription("When Single instancing is used, indicates when the instance expires")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue("")]
    public DateTime? InstanceAbsoluteExpiration
    {
      get;
      set;
    }


    [WebBrowsable(true)]
    [WebDisplayName("Instance Sliding Expiration")]
    [WebDescription("When Single instancing is used, indicates when the instance expires")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue("")]
    public TimeSpan? InstanceSlidingExpiration
    {
      get;
      set;
    }

    [WebBrowsable(true)]
    [WebDisplayName("Instance Initialization Code")]
    [WebDescription("Code to execute to initialize the instance.")]
    [Personalizable(PersonalizationScope.Shared)]
    [Category("Settings")]
    [DefaultValue("")]
    public string InstanceInitializationCode
    {
      get;
      set;
    }

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

      string codePath;
      var codeToExecute = Tamp(this.Code, out codePath);

      BaristaServiceClient client = new BaristaServiceClient(SPServiceContext.Current);

      var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
      request.Code = codeToExecute;
      request.CodePath = codePath;

      if (String.IsNullOrEmpty(this.InstanceInitializationCode) == false)
      {
        string filePath;
        request.InstanceInitializationCode = Tamp(this.InstanceInitializationCode, out filePath);
        request.InstanceInitializationCodePath = filePath;
      }

      request.InstanceMode = this.InstanceMode;
      request.InstanceName = this.InstanceName;
      request.InstanceAbsoluteExpiration = this.InstanceAbsoluteExpiration;
      request.InstanceSlidingExpiration = this.InstanceSlidingExpiration;

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
    private string Tamp(string code, out string codePath)
    {
      codePath = String.Empty;

      //If the code looks like a uri, attempt to retrieve a code file and use the contents of that file as the code.
      if (Uri.IsWellFormedUriString(code, UriKind.RelativeOrAbsolute))
      {
        Uri codeUri;
        if (Uri.TryCreate(code, UriKind.RelativeOrAbsolute, out codeUri))
        {
          string filePath;
          bool isHiveFile;
          String codeFromfile;
          if (SPHelper.TryGetSPFileAsString(code, out filePath, out codeFromfile, out isHiveFile))
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
            codePath = filePath;
          }
        }
      }

      //Replace any tokens in the code.
      code = SPHelper.ReplaceTokens(code);

      return code;
    }
  }
}
