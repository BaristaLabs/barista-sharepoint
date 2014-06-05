namespace Barista.SharePoint.Library
{
    using System.IO;
    using System.Xml;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint.WebPartPages;

    [Serializable]
    public class SPLimitedWebPartManagerConstructor : ClrFunction
    {
        public SPLimitedWebPartManagerConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPLimitedWebPartManager", new SPLimitedWebPartManagerInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPLimitedWebPartManagerInstance Construct()
        {
            return new SPLimitedWebPartManagerInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPLimitedWebPartManagerInstance : ObjectInstance
    {
        private readonly SPLimitedWebPartManager m_limitedWebPartManager;

        public SPLimitedWebPartManagerInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPLimitedWebPartManagerInstance(ObjectInstance prototype, SPLimitedWebPartManager limitedWebPartManager)
            : this(prototype)
        {
            if (limitedWebPartManager == null)
                throw new ArgumentNullException("limitedWebPartManager");

            m_limitedWebPartManager = limitedWebPartManager;
        }

        public SPLimitedWebPartManager SPLimitedWebPartManager
        {
            get
            {
                return m_limitedWebPartManager;
            }
        }

        [JSProperty(Name = "hasPersonalizedParts")]
        public bool HasPersonalizedParts
        {
            get
            {
                return m_limitedWebPartManager.HasPersonalizedParts;
            }
        }

        [JSProperty(Name = "scope")]
        public string Scope
        {
            get
            {
                return m_limitedWebPartManager.Scope.ToString();
            }
        }

        [JSProperty(Name = "serverRelativeUrl")]
        public string ServerRelativeUrl
        {
            get
            {
                return m_limitedWebPartManager.ServerRelativeUrl;
            }
        }

        [JSProperty(Name = "webPartConnections")]
        public SPWebPartConnectionCollectionInstance WebPartConnections
        {
            get
            {
                return m_limitedWebPartManager.SPWebPartConnections == null
                    ? null
                    : new SPWebPartConnectionCollectionInstance(this.Engine.Object.InstancePrototype, m_limitedWebPartManager.SPWebPartConnections);
            }
        }

        [JSProperty(Name = "webParts")]
        public SPWebPartConnectionCollectionInstance WebParts
        {
            get
            {
                return m_limitedWebPartManager.SPWebPartConnections == null
                    ? null
                    : new SPWebPartConnectionCollectionInstance(this.Engine.Object.InstancePrototype, m_limitedWebPartManager.SPWebPartConnections);
            }
        }

        [JSFunction(Name = "addWebPart")]
        public void AddWebPart(SPWebPartInstance webPart, string zoneId, int zoneIndex)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be supplied as the first argument.");

            m_limitedWebPartManager.AddWebPart(webPart.WebPart, zoneId, zoneIndex);
        }

        //CacheInvalidate

        [JSFunction(Name = "closeWebPart")]
        public void CloseWebPart(SPWebPartInstance webPart)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be supplied as the first argument.");

            m_limitedWebPartManager.CloseWebPart(webPart.WebPart);
        }

        [JSFunction(Name = "deleteWebPart")]
        public void DeleteWebPart(SPWebPartInstance webPart)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be supplied as the first argument.");

            m_limitedWebPartManager.DeleteWebPart(webPart.WebPart);
        }

        [JSFunction(Name = "exportWebPart")]
        public Base64EncodedByteArrayInstance ExportWebPart(SPWebPartInstance webPart)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be supplied as the first argument.");

            using(var ms = new MemoryStream())
            {
                var writer = XmlWriter.Create(ms);
                m_limitedWebPartManager.ExportWebPart(webPart.WebPart, writer);
                writer.Flush();
                return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, ms.ToArray());
            }
        }

        //GetConsumerConnectionPoints
        //GetProviderConnectionPoints

        [JSFunction(Name = "getStorageKey")]
        public GuidInstance GetStorageKey(SPWebPartInstance webPart)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be supplied as the first argument.");

            var result = m_limitedWebPartManager.GetStorageKey(webPart.WebPart);
            return new GuidInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getZoneId")]
        public string GetZoneId(SPWebPartInstance webPart)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be supplied as the first argument.");

            return m_limitedWebPartManager.GetZoneID(webPart.WebPart);
        }

        [JSFunction(Name = "importWebPart")]
        public SPWebPartInstance ImportWebPart(Base64EncodedByteArrayInstance webPart)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A byte array must be supplied as the first argument.");


            using(var ms = new MemoryStream(webPart.Data))
            {
                var reader = XmlReader.Create(ms);
                string errorMessage;
                var result = m_limitedWebPartManager.ImportWebPart(reader, out errorMessage) as WebPart;

                if (errorMessage.IsNullOrWhiteSpace() == false)
                    throw new JavaScriptException(this.Engine, "Error", errorMessage);

                return result == null
                    ? null
                    : new SPWebPartInstance(this.Engine.Object.InstancePrototype, result);
            }
        }

        [JSFunction(Name = "moveWebPart")]
        public void MoveWebPart(SPWebPartInstance webPart, string zoneId, int zoneIndex)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be supplied as the first argument.");

            m_limitedWebPartManager.MoveWebPart(webPart.WebPart, zoneId, zoneIndex);
        }

        [JSFunction(Name = "openWebPart")]
        public void OpenWebPart(SPWebPartInstance webPart)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be supplied as the first argument.");

            m_limitedWebPartManager.OpenWebPart(webPart.WebPart);
        }


        [JSFunction(Name = "resetAllPersonalizationState")]
        public void ResetAllPersonalizationState()
        {
            m_limitedWebPartManager.ResetAllPersonalizationState();
        }

        [JSFunction(Name = "resetPersonalizationState")]
        public void ResetPersonalizationState(SPWebPartInstance webPart)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be supplied as the first argument.");

            m_limitedWebPartManager.ResetPersonalizationState(webPart.WebPart);
        }

        [JSFunction(Name = "saveChanges")]
        public void SaveChanges(SPWebPartInstance webPart)
        {
            if (webPart == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part must be supplied as the first argument.");

            m_limitedWebPartManager.SaveChanges(webPart.WebPart);
        }

        //ConnectWebParts

        [JSFunction(Name = "disconnectWebParts")]
        public void DisconnectWebParts(SPWebPartConnectionInstance connection)
        {
            if (connection == null)
                throw new JavaScriptException(this.Engine, "Error", "A web part connection must be supplied as the first argument.");

            m_limitedWebPartManager.SPDisconnectWebParts(connection.SPWebPartConnection);
        }

        [JSFunction(Name = "getWeb")]
        public SPWebInstance GetWeb()
        {
            return m_limitedWebPartManager.Web == null
                ? null
                : new SPWebInstance(this.Engine, m_limitedWebPartManager.Web);
        }
    }
}
