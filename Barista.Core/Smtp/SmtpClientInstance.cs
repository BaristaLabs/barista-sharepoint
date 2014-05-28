namespace Barista.Smtp
{
    using System.ComponentModel;
    using System.Net.Mail;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using TypeConverter = Barista.Jurassic.TypeConverter;

    [Serializable]
    public class SmtpClientConstructor : ClrFunction
    {
        public SmtpClientConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SmtpClient", new SmtpClientInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SmtpClientInstance Construct(object host, object port)
        {
            if (host == Undefined.Value && port == Undefined.Value)
                return new SmtpClientInstance(this.InstancePrototype, new SmtpClient());

            return port == Undefined.Value
                ? new SmtpClientInstance(this.InstancePrototype, new SmtpClient(TypeConverter.ToString(host)))
                : new SmtpClientInstance(this.InstancePrototype, new SmtpClient(TypeConverter.ToString(host), TypeConverter.ToInteger(port)));
        }
    }

    [Serializable]
    public class SmtpClientInstance : ObjectInstance
    {
        private readonly SmtpClient m_smtpClient;

        public SmtpClientInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SmtpClientInstance(ObjectInstance prototype, SmtpClient smtpClient)
            : this(prototype)
        {
            if (smtpClient == null)
                throw new ArgumentNullException("smtpClient");
            
            m_smtpClient = smtpClient;
            m_smtpClient.SendCompleted += SendCompleted;
        }

        public SmtpClient SmtpClient
        {
            get { return m_smtpClient; }
        }

        [JSProperty(Name = "credentials")]
        public NetworkCredentialInstance Credentials
        {
            get
            {
                if (m_smtpClient.Credentials == null)
                    return null;

                var creds = m_smtpClient.Credentials.GetCredential(m_smtpClient.Host, m_smtpClient.Port, "Basic");

                return creds == null
                    ? null
                    : new NetworkCredentialInstance(this.Engine.Object.InstancePrototype, creds);
            }
            set {
                m_smtpClient.Credentials = value == null
                    ? null
                    : value.NetworkCredential;
            }
        }

        [JSProperty(Name = "deliveryMethod")]
        [JSDoc("Specifies how outgoing email messages will be handled. Possible values are 'Network', 'SpecifiedPickupDirectory', 'PickupDirectoryFromIis'")]
        public string DeliveryMethod
        {
            get
            {
                return m_smtpClient.DeliveryMethod.ToString();
            }
            set
            {
                SmtpDeliveryMethod method;
                if (value.TryParseEnum(true, out method))
                    m_smtpClient.DeliveryMethod = method;
            }
        }

        [JSProperty(Name = "enableSsl")]
        [JSDoc("Specify whether the SmtpClient uses SSL to encrypt the connection.")]
        public bool EnableSsl
        {
            get
            {
                return m_smtpClient.EnableSsl;
            }
            set
            {
                m_smtpClient.EnableSsl = value;
            }
        }

        [JSProperty(Name = "host")]
        [JSDoc("Gets or sets the name or IP address of the host used for SMTP transactions.")]
        public string Host
        {
            get
            {
                return m_smtpClient.Host;
            }
            set
            {
                m_smtpClient.Host = value;
            }
        }

        [JSProperty(Name = "pickupDirectoryLocation")]
        [JSDoc("Gets or sets the folder where applications save mail messages to be processed by the local SMTP server.")]
        public string PickupDirectoryLocation
        {
            get
            {
                return m_smtpClient.PickupDirectoryLocation;
            }
            set
            {
                m_smtpClient.PickupDirectoryLocation = value;
            }
        }

        [JSProperty(Name = "port")]
        [JSDoc("Gets or sets the port used for SMTP transactions.")]
        public int Port
        {
            get
            {
                return m_smtpClient.Port;
            }
            set
            {
                m_smtpClient.Port = value;
            }
        }

        [JSProperty(Name = "servicePoint")]
        [JSDoc("Gets the network connection used to transmit the e-mail message.")]
        public int ServicePoint
        {
            get
            {
                return m_smtpClient.Port;
            }
        }

        [JSProperty(Name = "targetName")]
        [JSDoc("Gets or sets the Service Provider Name (SPN) to use for authentication when using extended protection.")]
        public string TargetName
        {
            get
            {
                return m_smtpClient.TargetName;
            }
            set
            {
                m_smtpClient.TargetName = value;
            }
        }

        [JSProperty(Name = "timeout")]
        [JSDoc("Gets or sets a value that specifies the amount of time after which a synchronous send call times out.")]
        public int Timeout
        {
            get
            {
                return m_smtpClient.Timeout;
            }
            set
            {
                m_smtpClient.Timeout = value;
            }
        }

        [JSProperty(Name = "useDefaultCredentials")]
        [JSDoc("Gets or sets a boolean value that controls whether default credentials are sent with requests. The default is false.")]
        public bool UseDefaultCredentials
        {
            get
            {
                return m_smtpClient.UseDefaultCredentials;
            }
            set
            {
                m_smtpClient.UseDefaultCredentials = value;
            }
        }

        [JSProperty(Name = "sendCompletedCallback")]
        [JSDoc("Function that is called when an asynchronous e-mail send operation completes.")]
        public FunctionInstance SendCompletedCallback
        {
            get;
            set;
        }

        [JSFunction(Name = "send")]
        public void Send(string from, string recipients, string subject, string body)
        {
            m_smtpClient.Send(from, recipients, subject, body);
        }

        [JSFunction(Name = "sendAsync")]
        public void SendAsync(string from, string recipients, string subject, string body, object userToken)
        {
            m_smtpClient.SendAsync(from, recipients, subject, body, userToken);
        }

        [JSFunction(Name = "sendMailMessage")]
        public void SendMailMessage(MailMessageInstance mailMessage)
        {
            if (mailMessage == null)
                throw new JavaScriptException(this.Engine, "Error", "A MailMessage object must be specified.");

            var mess = mailMessage.MailMessage;

            m_smtpClient.Send(mess);
        }

        [JSFunction(Name = "sendMailMessageAsync")]
        public void SendMailMessageAsync(MailMessageInstance mailMessage, object userToken)
        {
            if (mailMessage == null)
                throw new JavaScriptException(this.Engine, "Error", "A MailMessage object must be specified.");

            var mess = mailMessage.MailMessage;

            m_smtpClient.SendAsync(mess, userToken);
        }

        [JSFunction(Name = "sendAsyncCancel")]
        public void SendAsyncCancel()
        {
            m_smtpClient.SendAsyncCancel();
        }

        private void SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (this.SendCompletedCallback != null)
                SendCompletedCallback.Call(m_smtpClient, e.UserState, e.Cancelled);
        }
    }
}
