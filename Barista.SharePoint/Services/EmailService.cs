namespace Barista.SharePoint.Services
{
  using Barista.Framework;

  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Client.Services;

  using System;
  using System.Collections.Specialized;
  using System.Net.Mail;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;

  [BasicHttpBindingServiceMetadataExchangeEndpoint]
  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public sealed class EmailService
  {
    [OperationContract]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    public bool SendEmail(string to, string cc, string bcc, string from, string subject, string messageBody, bool appendFooter)
    {
      if (BaristaContext.Current == null)
        throw new InvalidOperationException("No SharePoint Context.");

      //Todo: Replace tokens in message body with the specified text.
      StringDictionary headers = new StringDictionary
        {
          {"to", to},
          {"cc", cc},
          {"bcc", bcc},
          {"from", @from},
          {"subject", subject},
          {"content-type", "text/html"}
        };

      messageBody = "<HTML><HEAD><META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html\"></HEAD><BODY>" + messageBody + "</BODY></HTML>";

      return EmailService.SendMailInternal(BaristaContext.Current.Site, subject, messageBody, true, from, to, cc, bcc);
    }

    private static bool SendMailInternal(SPSite site, string subject, string body, bool isBodyHtml, string @from, string to, string cc, string bcc)
    {
      try
      {
        SmtpClient smtpClient = new SmtpClient {
          Host = site.WebApplication.OutboundMailServiceInstance.Server.Address
        };

        MailMessage mailMessage = new MailMessage(@from, to, subject, body);
        if (!String.IsNullOrEmpty(cc))
        {
          MailAddress ccAddress = new MailAddress(cc);
          mailMessage.CC.Add(ccAddress);
        }
        if (!String.IsNullOrEmpty(bcc))
        {
          MailAddress bccAddress = new MailAddress(bcc);
          mailMessage.Bcc.Add(bccAddress);
        }
        mailMessage.IsBodyHtml = isBodyHtml;
        smtpClient.Send(mailMessage);
      }
      catch (Exception)
      {
        return false;
      }

      return true;
    }
  }
}