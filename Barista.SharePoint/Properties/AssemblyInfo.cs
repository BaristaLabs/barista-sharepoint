#define CODE_ANALYSIS
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Barista.SharePoint")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Barista.SharePoint")]
[assembly: AssemblyCopyright("Copyright ©  2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("77bc7214-5d7d-472c-88c5-7d022d93c231")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: SuppressMessage("SPCAF.Rules.CorrectnessGroup", "SPC010212:DoNotCallSPSiteCatchAccessDeniedException", Justification = "Providing an accessor for underlying method.")]
[assembly: SuppressMessage("SPCAF.Rules.CorrectnessGroup", "SPC010214:DoNotUseSPWebProperties", Justification = "Providing an accessor for underlying method.")]

[assembly: SuppressMessage("SPCAF.Rules.MemoryDisposalGroup", "SPC110201:DisposeCreatedSPSiteObject", Justification = "SPSite in these instances disposed by Barista framework after request.")]
[assembly: SuppressMessage("SPCAF.Rules.MemoryDisposalGroup", "SPC110203:DisposeSPWebCreatedByOpenWeb", Justification = "SPWeb in these instances disposed by Barista framework after request.")]
[assembly: SuppressMessage("SPCAF.Rules.MemoryDisposalGroup", "SPC110205:DisposeSPWebCreatedBySPLimitedWebPartManager", Justification = "SPLimitedWebPartManager disposed by Barista framework after request.")]
[assembly: SuppressMessage("SPCAF.Rules.MemoryDisposalGroup", "SPC110211:DisposeSPSiteCreatedBySPSiteCollectionAdd", Justification = "SPSiteCollectoin.Add result in these instances disposed by Barista framework after request.")]
[assembly: SuppressMessage("SPCAF.Rules.MemoryDisposalGroup", "SPC110213:DisposeSPWebCreatedBySPWebCollectionAdd", Justification = "SPWebCollection.Add result in these instances disposed by Barista framework after request.")]
[assembly: SuppressMessage("SPCAF.Rules.MemoryDisposalGroup", "SPC110212:DisposeSPSiteCreatedBySPSiteCollectionIndex", Justification = "SPSite objects in these instances disposed by Barista framework after request.")]
[assembly: SuppressMessage("SPCAF.Rules.MemoryDisposalGroup", "SPC110214:DisposeSPWebCreatedBySPWebCollectionIndex", Justification = "SPWeb objects in these instances disposed by Barista framework after request.")]

[assembly: SuppressMessage("SPCAF.Rules.SecurityGroup", "SPC020202:AvoidCallToAllowUnsafeUpdatesOnSPWeb", Justification = "Unsafe Updates required as calls may not necessarily be associated with a POST, but still valid.")]
[assembly: SuppressMessage("SPCAF.Rules.SecurityGroup", "SPC020203:AvoidCallToAllowUnsafeUpdatesOnSPSite", Justification = "Unsafe Updates required as calls may not necessarily be associated with a POST, but still valid.")]
[assembly: SuppressMessage("SPCAF.Rules.SecurityGroup", "SPC020205:DoNotSetSPListAllowEveryoneViewItemsToTrue", Justification = "Providing an accessor for underlying method.")]
[assembly: SuppressMessage("SPCAF.Rules.SecurityGroup", "SPC020204:DoNotCallWindowsIdentityImpersonate", Justification = "The intent of this class is to facilitate impersonation for credentials defined in a secure store application.")]

[assembly: SuppressMessage("SPCAF.Rules.SupportabilityGroup", "SPC030207:DoNotWriteToFileSystem", Justification = "SP File System Access needed to retrieve hive files.")]
[assembly: SuppressMessage("SPCAF.Rules.SupportabilityGroup", "SPC030211:DoNotModifyFarmSettings", Justification = "SPFarm Settings required to be modified on Service Application installation")]
[assembly: SuppressMessage("SPCAF.Rules.SupportabilityGroup", "SPC030221:DoNotModifyWebAppSettings", Justification = "SPWebApplication Settings required to be modified on Service Application installation")]

[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC050236:DoNotCallSPItemEventPropertiesOpenWeb", Justification = "False best practice: OpenWeb() required to initialize SPContext within current scope.")]
[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC052101:DoNotActivateFeaturesViaAPI", Justification = "False best practice: SharePoint is a runtime system, features may be activated and deactivated through the UI and by particular business events, the API calls are there to faciliate this in a manner that powershell or feature stapling cannot accomplish.")]
[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC050237:DoNotUseFieldCollectionByIndex", Justification = "False best practice: Accessing field collection by index is appropriate when using a GUID.")]
[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC050238:DoNotUseSPWebGetListFromUrl", Justification = "False best practice: When the url is not associated with a web part page, GetListFromWebPartPageUrl fails.")]
[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC050222:DoNotUseSPListItems", Justification = "Providing an accessor for underlying method.")]
[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC050223:DoNotUseListItemsGetItemById", Justification = "Providing an accessor for underlying method.")]
[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC050224:DoNotUseListItemsByIndex", Justification = "Providing an accessor for underlying method.")]
[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC050226:DoNotUseSPListItemVersionDelete", Justification = "Providing an accessor for underlying method.")]
[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC050227:DoNotUseSPFileVersionDelete", Justification = "Providing an accessor for underlying method.")]
[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC050233:UseRowLimitInQueries", Justification = "Providing an accessor for underlying method.")]
[assembly: SuppressMessage("SPCAF.Rules.BestPracticesGroup", "SPC055201:ConsiderBestMatchForContentTypesRetrieval", Justification = "Providing an accessor for underlying method.")]