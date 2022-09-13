namespace Barista.SharePoint.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Security.Permissions;
  using System.Web.UI.WebControls.WebParts;
  using System.Xml;

  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Utilities;
  using Microsoft.SharePoint.WebPartPages;

  using WebPart = System.Web.UI.WebControls.WebParts.WebPart;

  /// <summary> 
  /// Used to solve the problem of issues with document types deploying without the usage of the document set welcome page. 
  /// For more information, see http://code.msdn.microsoft.com/Custom-Document-Set-eb3fbcfd. 
  /// </summary> 
  [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
  [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
  public abstract class CustomDocumentSetFeatureReceiver : SPFeatureReceiver
  {
    #region Public Properties

    /// <summary> 
    /// Gets the feature scope of the calling feature. 
    /// </summary> 
    /// <value>The scope.</value> 
    public SPFeatureScope Scope { get; private set; }

    /// <summary> 
    /// Gets the <see cref = "SPSite" /> that contains the feature. 
    /// </summary> 
    public SPSite Site { get; private set; }

    /// <summary> 
    /// Gets the <see cref = "SPWeb" /> that contains the feature. 
    /// </summary> 
    public SPWeb Web { get; private set; }

    #endregion

    #region Properties

    /// <summary> 
    /// Gets the content types to provision. 
    /// </summary> 
    protected abstract IEnumerable<SPContentTypeId> ContentTypeIds { get; }

    /// <summary> 
    /// Gets or sets a value indicating whether to ensure web parts by removing and adding them on each occation. 
    /// </summary> 
    /// <value> 
    ///   <c>true</c> to ensure web parts; otherwise, <c>false</c>. 
    /// </value> 
    protected bool EnsureWebParts { get; set; }

    /// <summary> 
    /// Gets the feature receiver properties. 
    /// </summary> 
    /// <value> 
    /// The feature receiver properties. 
    /// </value> 
    protected SPFeatureReceiverProperties Properties { get; private set; }

    #endregion

    #region Public Methods

    /// <summary> 
    /// Adds the event handler to the content type. 
    /// </summary> 
    /// <param name="contentType">The content type.</param> 
    /// <param name="name">The name of the event.</param> 
    /// <param name="receiver">The type reference of the event receiver class.</param> 
    /// <param name="type">The event receiver type.</param> 
    /// <param name="sequence">The sequence.</param> 
    /// <param name="sync">The synchronization type.</param> 
    public void AddEventHandler(
        SPContentType contentType,
        string name,
        Type receiver,
        SPEventReceiverType type,
        int sequence,
        SPEventReceiverSynchronization sync)
    {
      // Guard 
      if (receiver == null)
      {
        throw new ArgumentNullException("receiver");
      }

            AddEventHandler(contentType, name, receiver.Assembly.FullName, receiver.FullName, type, sequence, sync);
    }

    /// <summary> 
    /// Adds the event handler to the content type. 
    /// </summary> 
    /// <param name="contentType">The content type.</param> 
    /// <param name="name">The name of the event.</param> 
    /// <param name="assembly">The assembly containin the event receiver.</param> 
    /// <param name="className">Name of the event receiver class.</param> 
    /// <param name="type">The event receiver type.</param> 
    /// <param name="sequence">The sequence.</param> 
    /// <param name="sync">The synchronization type.</param> 
    public void AddEventHandler(
        SPContentType contentType,
        string name,
        string assembly,
        string className,
        SPEventReceiverType type,
        int sequence,
        SPEventReceiverSynchronization sync)
    {
      // Guard 
      if (contentType == null)
      {
        throw new ArgumentNullException("contentType");
      }

      SPEventReceiverDefinition definition = GetEventHandler(contentType.EventReceivers, name, type);
      if (definition == null)
      {
        contentType.EventReceivers.Add(type, assembly, className);
        definition = GetEventHandler(contentType.EventReceivers, className, type);
      }

      definition.Name = name;
      definition.SequenceNumber = sequence;
      definition.Synchronization = sync;
      definition.Update();
    }

    /// <summary> 
    /// Occurs on feature activation. 
    /// </summary> 
    /// <param name="properties"> 
    /// An <see cref="T:Microsoft.SharePoint.SPFeatureReceiverProperties"/> object that represents the properties of the event. 
    /// </param> 
    public override void FeatureActivated(SPFeatureReceiverProperties properties)
    {
            EnsureContext(properties);
            foreach (SPContentTypeId id in ContentTypeIds)
      {
                ProvisionDocumentSet(id);
      }
    }

    #endregion

    #region Methods

    /// <summary> 
    /// Adds the web part to the page from a definition. 
    /// </summary> 
    /// <param name="manager">The web part manager.</param> 
    /// <param name="definition">The web part definition.</param> 
    /// <param name="zone">The web part zone.</param> 
    /// <param name="index">The zone index.</param> 
    protected void AddWebPart(SPLimitedWebPartManager manager, string definition, string zone, int index)
    {
      // Guard 
      if (manager == null)
      {
        throw new ArgumentNullException("manager");
      }

            if (Web == null)
      {
        throw new InvalidOperationException("You must call EnsureContext method before calling this method.");
      }

            string data = Web.GetFileAsString(definition);
      if (data != null)
      {
        WebPart webPart;
                using (var reader = new StringReader(data))
        {
          string errorMessage;
                    var xmlTextReader = new XmlTextReader(reader);
          webPart = manager.ImportWebPart(xmlTextReader, out errorMessage);
          if (webPart == null)
          {
            throw new WebPartPageUserException(errorMessage);
          }
        }

        manager.AddWebPart(webPart, zone, index);
      }
    }

    /// <summary> 
    /// Gets the welcome page web part manager. 
    /// </summary> 
    /// <param name="file">The file to return a <see cref="SPLimitedWebPartManager"/> instance for.</param> 
    /// <returns>The web part manager of the welcome page.</returns> 
    protected SPLimitedWebPartManager GetWelcomePageWebPartmanager(SPFile file)
    {
      // Guard 
      if (file == null)
      {
        throw new ArgumentNullException("file");
      }

            return Web.GetLimitedWebPartManager(file.Url, PersonalizationScope.Shared);
    }

    /// <summary> 
    /// Provisions the web parts. 
    /// </summary> 
    /// <param name="manager">The manager.</param> 
    protected virtual void ProvisionWebParts(SPLimitedWebPartManager manager)
    {
      // Guard 
      if (manager == null)
      {
        throw new ArgumentNullException("manager");
      }
    }

    /// <summary> 
    /// Gets a specific event handler. 
    /// </summary> 
    /// <param name="sperdcol">The event receiver definitions.</param> 
    /// <param name="className">The event receiver class name.</param> 
    /// <param name="type">The event type.</param> 
    /// <returns>The existing event handler or <c>null</c> if the event handler was not found.</returns> 
    private static SPEventReceiverDefinition GetEventHandler(
        SPEventReceiverDefinitionCollection sperdcol, string className, SPEventReceiverType type)
    {
      if (sperdcol == null)
      {
        throw new ArgumentNullException("sperdcol");
      }

      for (int i = 0; i < sperdcol.Count; i++)
      {
        SPEventReceiverDefinition definition = sperdcol[i];
        if ((definition.Type == type) && (definition.Class == className))
        {
          return definition;
        }
      }

      return null;
    }

    /// <summary> 
    /// Gets the document set welcome page for the specified content type. 
    /// </summary> 
    /// <param name="contentType">The content type.</param> 
    /// <returns>An <see cref="SPFile"/> object representing the document set welcome page.</returns> 
    private static SPFile GetWelcomePage(SPContentType contentType)
    {
      // Guard 
      if (contentType == null)
      {
        throw new ArgumentNullException("contentType");
      }

      if (contentType.ResourceFolder.Files != null)
      {
        var files =
            contentType.ResourceFolder.Files.Cast<SPFile>().Where(f => f.Name == "docsethomepage.aspx");
        var spFiles = files as IList<SPFile> ?? files.ToList();
        return !spFiles.Any() ? null : spFiles.First();
      }

      return null;
    }

    /// <summary> 
    /// Provisions the welcome page. 
    /// </summary> 
    /// <param name="contentType">The content type.</param> 
    /// <returns>The welcome page.</returns> 
    private static SPFile ProvisionWelcomePage(SPContentType contentType)
    {
      // Guard 
      if (contentType == null)
      {
        throw new ArgumentNullException("contentType");
      }

      SPFile file = GetWelcomePage(contentType);
      if (file != null)
      {
        return file;
      }

      byte[] buffer =
          File.ReadAllBytes(SPUtility.GetVersionedGenericSetupPath(@"Template\Features\DocumentSet\docsethomepage.aspx", SPUtility.ContextCompatibilityLevel));
      SPFolder resourceFolder = contentType.ResourceFolder;
      return resourceFolder.Files.Add("docsethomepage.aspx", buffer, true);
    }

    /// <summary> 
    /// Gets a value indicating whether the document sets are fully deployed. 
    /// </summary> 
    /// <param name="contentType">The content type.</param> 
    /// <returns><c>true</c> if deployed; otherwise, <c>false</c>.</returns> 
    private bool Deployed(SPContentType contentType)
    {
      SPFile welcomePage = GetWelcomePage(contentType);
      if (welcomePage == null)
      {
        return false;
      }

            if (EnsureWebParts)
      {
        return false;
      }

            return GetWelcomePageWebPartmanager(GetWelcomePage(contentType)).WebParts.Count > 1;
    }

    /// <summary> 
    /// Sets the <see cref="Web"/> and <see cref="Site"/> objects using the <paramref name="properties"/> object. 
    /// </summary> 
    /// <param name="properties">Represents the properties of a Feature installation, uninstallation, activation or deactivation event.</param> 
    private void EnsureContext(SPFeatureReceiverProperties properties)
    {
      // Guard 
      if (properties == null)
      {
        throw new ArgumentNullException("properties");
      }

            Properties = properties;
            Scope = properties.Definition.Scope;
            switch (Scope)
      {
        case SPFeatureScope.Site:
                    SetContextSite(properties.Feature.Parent as SPSite);
          break;
        case SPFeatureScope.Web:
                    SetContextWeb(properties.Feature.Parent as SPWeb);
          break;
      }
    }

    /// <summary> 
    /// Provisions the document set. 
    /// </summary> 
    /// <param name="contentTypeId">The conten type id.</param> 
    private void ProvisionDocumentSet(SPContentTypeId contentTypeId)
    {
            var bestMatchContentTypeId = Site.RootWeb.ContentTypes.BestMatch(contentTypeId);
            var contentType = Site.RootWeb.ContentTypes[bestMatchContentTypeId];
            if (Deployed(contentType))
      {
        return;
      }

            ProvisionDocumentSet(contentType);
    }

    /// <summary> 
    /// Provisions the document set. 
    /// </summary> 
    /// <param name="contentType">Type of the content.</param> 
    private void ProvisionDocumentSet(SPContentType contentType)
    {
            ProvisionEventHandler(contentType);
      SPFile file = ProvisionWelcomePage(contentType);
            using (SPLimitedWebPartManager manager = GetWelcomePageWebPartmanager(file))
      {
                if (EnsureWebParts)
        {
          if (manager.WebParts != null)
          {
            List<WebPart> webParts = new List<WebPart>(manager.WebParts.Cast<WebPart>());
            foreach (WebPart webPart in webParts)
            {
              manager.DeleteWebPart(webPart);
            }
          }
        }

                ProvisionWebParts(manager);
      }
    }

    /// <summary> 
    /// Provisions the event handler. 
    /// </summary> 
    /// <param name="contentType">The content type.</param> 
    private void ProvisionEventHandler(SPContentType contentType)
    {
            AddEventHandler(
          contentType,
          "DocumentSet ItemUpdated",
          "Microsoft.Office.DocumentManagement, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c",
          "Microsoft.Office.DocumentManagement.DocumentSets.DocumentSetEventReceiver",
          SPEventReceiverType.ItemUpdated,
          100,
          SPEventReceiverSynchronization.Synchronous);

            AddEventHandler(
          contentType,
          "DocumentSet ItemAdded",
          "Microsoft.Office.DocumentManagement, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c",
          "Microsoft.Office.DocumentManagement.DocumentSets.DocumentSetItemsEventReceiver",
          SPEventReceiverType.ItemAdded,
          100,
          SPEventReceiverSynchronization.Synchronous);
    }

    /// <summary> 
    /// Sets the <see cref="Web"/> and <see cref="Site"/> objects. 
    /// </summary> 
    /// <param name="site">The <see cref="SPSite"/> that is the current site collection of the context.</param> 
    private void SetContextSite(SPSite site)
    {
      // Guard 
      if (site == null)
      {
        throw new ArgumentNullException("site");
      }

            Site = site;
            Web = site.RootWeb;
    }

    /// <summary> 
    /// Sets the <see cref="Web"/> and <see cref="Site"/> objects. 
    /// </summary> 
    /// <param name="web">The <see cref="SPWeb"/> that is the current site of the context.</param> 
    private void SetContextWeb(SPWeb web)
    {
      // Guard 
      if (web == null)
      {
        throw new ArgumentNullException("web");
      }

            Web = web;
            Site = web.Site;
    }

    #endregion
  } 
}
