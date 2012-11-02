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

      StringDictionary headers = new StringDictionary();

      headers.Add("to", to);
      headers.Add("cc", cc);
      headers.Add("bcc", bcc);
      headers.Add("from", from);
      headers.Add("subject", subject);
      headers.Add("content-type", "text/html");

      messageBody = "<HTML><HEAD><META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html\"></HEAD><BODY>" + messageBody + "</BODY></HTML>";

      return EmailService.SendMailInternal(BaristaContext.Current.Site, subject, messageBody, true, from, to, cc, bcc);
    }

    private static bool SendMailInternal(SPSite site, string Subject, string Body, bool IsBodyHtml, string From, string To, string Cc, string Bcc)
    {
      bool mailSent = false;
      try
      {
        SmtpClient smtpClient = new SmtpClient();
        smtpClient.Host = site.WebApplication.OutboundMailServiceInstance.Server.Address;

        MailMessage mailMessage = new MailMessage(From, To, Subject, Body);
        if (!String.IsNullOrEmpty(Cc))
        {
          MailAddress CCAddress = new MailAddress(Cc);
          mailMessage.CC.Add(CCAddress);
        }
        if (!String.IsNullOrEmpty(Bcc))
        {
          MailAddress BCCAddress = new MailAddress(Bcc);
          mailMessage.Bcc.Add(BCCAddress);
        }
        mailMessage.IsBodyHtml = IsBodyHtml;
        smtpClient.Send(mailMessage);
        mailSent = true;
      }
      catch (Exception) { return mailSent; }
      return mailSent;
    }
  }
}