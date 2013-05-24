namespace Barista.SharePoint
{
  using Microsoft.SharePoint;

  using System;
  using System.Collections.Specialized;
  using System.Net.Mail;

  public static class EmailHelper
  {
    public static bool SendEmail(string to, string cc, string bcc, string from, string subject, string messageBody, bool appendFooter)
    {
      if (SPBaristaContext.Current == null)
        throw new InvalidOperationException("No SharePoint Context.");

      //Todo: Replace tokens in message body with the specified text.
      var headers = new StringDictionary
        {
          {"to", to},
          {"cc", cc},
          {"bcc", bcc},
          {"from", @from},
          {"subject", subject},
          {"content-type", "text/html"}
        };

      messageBody = "<HTML><HEAD><META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html\"></HEAD><BODY>" + messageBody + "</BODY></HTML>";

      return EmailHelper.SendMailInternal(SPBaristaContext.Current.Site, subject, messageBody, true, from, to, cc, bcc);
    }

    private static bool SendMailInternal(SPSite site, string subject, string body, bool isBodyHtml, string @from, string to, string cc, string bcc)
    {
      try
      {
        var smtpClient = new SmtpClient {
          Host = site.WebApplication.OutboundMailServiceInstance.Server.Address
        };

        var mailMessage = new MailMessage(@from, to, subject, body);
        if (!String.IsNullOrEmpty(cc))
        {
          var ccAddress = new MailAddress(cc);
          mailMessage.CC.Add(ccAddress);
        }
        if (!String.IsNullOrEmpty(bcc))
        {
          var bccAddress = new MailAddress(bcc);
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