namespace Barista.SharePoint
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Management.Automation;
  using Microsoft.SharePoint.PowerShell;
  using Microsoft.SharePoint;
  using Barista.SharePoint.Services;

  [Cmdlet("Invoke", "BaristaService", SupportsShouldProcess = true)]
  public class InvokeBaristaService : SPCmdlet
  {
    private string m_code;
    private bool m_isEval = false;
    private bool m_isExec = false;

    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public SPServiceContextPipeBind ServiceContext;

    [Parameter(ParameterSetName = "Eval", Mandatory = false)]
    public string Eval
    {
      get
      {
        return m_code;
      }
      set
      {
        m_isEval = true;
        m_code = value;
      }
    }

    [Parameter(ParameterSetName = "Exec", Mandatory = false)]
    public string Exec
    {
      get
      {
        return m_code;
      }
      set
      {
        m_isExec = true;
        m_code = value;
      }
    }

    protected override void InternalProcessRecord()
    {
      // get the specified service context
      SPServiceContext serviceContext = ServiceContext.Read();
      if (serviceContext == null)
      {
        WriteError(new InvalidOperationException("Invalid service context."), ErrorCategory.ResourceExists, serviceContext);
        return;
      }

      if (m_isEval && m_isExec)
      {
        WriteError(new InvalidOperationException("Can be exec or eval, but not both."), ErrorCategory.InvalidArgument, null);
        return;
      }

      if (m_isEval == false && m_isExec == false)
      {
        WriteError(new InvalidOperationException("Must specify either eval or exec"), ErrorCategory.InvalidArgument, null);
        return;
      }

      BaristaServiceClient client = new BaristaServiceClient(serviceContext);

      var request = new BrewRequest() {
        Code = m_code,
      };

      BrewResponse response = null;

      if (m_isEval)
      {
        response = client.Eval(request);

        if (response.ContentType.StartsWith("application/json", StringComparison.InvariantCultureIgnoreCase) ||
            response.ContentType.StartsWith("application/xml", StringComparison.InvariantCultureIgnoreCase) ||
            response.ContentType.StartsWith("application/javascript", StringComparison.InvariantCultureIgnoreCase) ||
            response.ContentType.StartsWith("text", StringComparison.InvariantCultureIgnoreCase))
          WriteResult(System.Text.Encoding.UTF8.GetString(response.Content));
        else
          WriteObject(response.Content);
      }
      else
      {
        client.Exec(request);
      }
    }
  }
}
