namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.WebPartPages;

    [Serializable]
    public class SPWebPartConnectionConstructor : ClrFunction
    {
        public SPWebPartConnectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWebPartConnection", new SPWebPartConnectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWebPartConnectionInstance Construct()
        {
            return new SPWebPartConnectionInstance(this.InstancePrototype, new SPWebPartConnection());
        }
    }

    [Serializable]
    public class SPWebPartConnectionInstance : ObjectInstance
    {
        private readonly SPWebPartConnection m_webPartConnection;

        public SPWebPartConnectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWebPartConnectionInstance(ObjectInstance prototype, SPWebPartConnection webPartConnection)
            : this(prototype)
        {
            if (webPartConnection == null)
                throw new ArgumentNullException("webPartConnection");

            m_webPartConnection = webPartConnection;
        }

        public SPWebPartConnection SPWebPartConnection
        {
            get
            {
                return m_webPartConnection;
            }
        }

        [JSProperty(Name = "consumer")]
        public SPWebPartInstance Consumer
        {
            get
            {
                if (m_webPartConnection.Consumer == null || (m_webPartConnection.Consumer is WebPart) == false)
                    return null;

                return new SPWebPartInstance(this.Engine.Object.InstancePrototype, m_webPartConnection.Consumer as WebPart);
            }
        }

        //ConsumerConnectionPoint

        [JSProperty(Name = "consumerConnectionPointId")]
        public string ConsumerConnectionPointId
        {
            get
            {
                return m_webPartConnection.ConsumerConnectionPointID;
            }
            set
            {
                m_webPartConnection.ConsumerConnectionPointID = value;
            }
        }

        [JSProperty(Name = "consumerId")]
        public string ConsumerId
        {
            get
            {
                return m_webPartConnection.ConsumerID;
            }
            set
            {
                m_webPartConnection.ConsumerID = value;
            }
        }

        [JSProperty(Name = "crossPageConnectionId")]
        public string CrossPageConnectionId
        {
            get
            {
                return m_webPartConnection.CrossPageConnectionID;
            }
            set
            {
                m_webPartConnection.CrossPageConnectionID = value;
            }
        }

        [JSProperty(Name = "crossPageSchema")]
        public string CrossPageSchema
        {
            get
            {
                return m_webPartConnection.CrossPageSchema;
            }
            set
            {
                m_webPartConnection.CrossPageSchema = value;
            }
        }

        [JSProperty(Name = "id")]
        public string Id
        {
            get
            {
                return m_webPartConnection.ID;
            }
            set
            {
                m_webPartConnection.ID = value;
            }
        }

        [JSProperty(Name = "isActive")]
        public bool IsActive
        {
            get
            {
                return m_webPartConnection.IsActive;
            }
        }

        [JSProperty(Name = "isEnabled")]
        public bool IsEnabled
        {
            get
            {
                return m_webPartConnection.IsEnabled;
            }
        }

        [JSProperty(Name = "isShared")]
        public bool IsShared
        {
            get
            {
                return m_webPartConnection.IsShared;
            }
        }

        [JSProperty(Name = "isStatic")]
        public bool IsStatic
        {
            get
            {
                return m_webPartConnection.IsStatic;
            }
        }

        [JSProperty(Name = "provider")]
        public SPWebPartInstance Provider
        {
            get
            {
                if (m_webPartConnection.Provider == null || (m_webPartConnection.Provider is WebPart) == false)
                    return null;

                return new SPWebPartInstance(this.Engine.Object.InstancePrototype, m_webPartConnection.Provider as WebPart);
            }
        }

        [JSProperty(Name = "providerConnectionPointId")]
        public string ProviderConnectionPointId
        {
            get
            {
                return m_webPartConnection.ProviderConnectionPointID;
            }
            set
            {
                m_webPartConnection.ProviderConnectionPointID = value;
            }
        }

        [JSProperty(Name = "providerID")]
        public string ProviderId
        {
            get
            {
                return m_webPartConnection.ProviderID;
            }
            set
            {
                m_webPartConnection.ProviderID = value;
            }
        }

        [JSProperty(Name = "sourcePageUrl")]
        public string SourcePageUrl
        {
            get
            {
                return m_webPartConnection.SourcePageUrl;
            }
            set
            {
                m_webPartConnection.SourcePageUrl = value;
            }
        }

        [JSProperty(Name = "targetPageUrl")]
        public string TargetPageUrl
        {
            get
            {
                return m_webPartConnection.TargetPageUrl;
            }
            set
            {
                m_webPartConnection.TargetPageUrl = value;
            }
        }

        //Transformer

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_webPartConnection.ToString();
        }
    }
}
