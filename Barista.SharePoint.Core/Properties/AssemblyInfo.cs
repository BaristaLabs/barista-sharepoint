#define CODE_ANALYSIS
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Barista.SharePoint.Core")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Barista.SharePoint.Core")]
[assembly: AssemblyCopyright("Copyright ©  2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d7ed67ce-e3a1-455d-95cb-9b62d8ad340d")]

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

[assembly: InternalsVisibleTo("Barista.UnitTests.CamlexNET, PublicKey=002400000480000094000000060200000024000052534131000400000100010031d062a4737096baa168e95dfab39dc20a1a022576802a2fc97a5381aa12d54c2c135856705d87e8d1068f89e2e4304aacfb1210c945ddd7745234e849a73764abcd8a95496299c44841bfad6770907993825ba014d4ac5a86510ff630c26c4ea93f7101e285da2ca27df0465625e7ecc70bbed5c394369f5f59d0ecb92c57b6")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

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

[assembly: SuppressMessage("SPCAF.Rules.SupportabilityGroup", "SPC030211:DoNotModifyFarmSettings", Justification = "SPFarm Settings required to be modified on Service Application installation")]
[assembly: SuppressMessage("SPCAF.Rules.SupportabilityGroup", "SPC030221:DoNotModifyWebAppSettings", Justification = "SPWebApplication Settings required to be modified on Service Application installation")]
[assembly: SuppressMessage("SPCAF.Rules.SupportabilityGroup", "SPC030207:DoNotWriteToFileSystem", Justification = "SP File System Access needed to retrieve hive files.")]

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