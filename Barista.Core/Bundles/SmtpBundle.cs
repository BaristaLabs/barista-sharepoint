namespace Barista.Bundles
{
    using Barista.Smtp;
    using Jurassic;
    using System;

    [Serializable]
    public class SmtpBundle : IBundle
    {
        public bool IsSystemBundle
        {
            get
            {
                return true;
            }
        }

        public string BundleName
        {
            get
            {
                return "Smtp";
            }
        }

        public string BundleDescription
        {
            get
            {
                return "Includes functionality to interact with Smtp severs.";
            }
        }

        public object InstallBundle(ScriptEngine engine)
        {
            engine.SetGlobalValue("SmtpClient", new SmtpClientConstructor(engine));
            engine.SetGlobalValue("MailAddress", new MailAddressConstructor(engine));
            engine.SetGlobalValue("SmtpAttachment", new AttachmentConstructor(engine));
            engine.SetGlobalValue("SmtpLinkedResource", new LinkedResourceConstructor(engine));
            engine.SetGlobalValue("MailMessage", new MailMessageConstructor(engine));
            engine.SetGlobalValue("AlternateView", new AlternateViewConstructor(engine));

            return Undefined.Value;
        }
    }
}
