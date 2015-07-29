namespace Barista.SharePoint
{
    using Barista.Configuration;
    using Barista.Extensions;
    using Barista.Newtonsoft.Json;
    using Barista.SharePoint.ServiceManagement;
    using Barista.SharePoint.Services;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Policy;
    using System.ServiceModel.Security;
    using Microsoft.SharePoint.Utilities;
    using Ninject;

    /// <summary>
    /// Contains various helper methods involving barista configuration stored in SharePoint.
    /// </summary>
    public static class BaristaHelper
    {

        public const string BaristaTrustedLocationsPropertyBagKey = "BaristaTrustedLocations";
        public const string BaristaSearchIndexDefinitionsPropertyBagKey = "BaristaSearchIndexDefinitions";
        public const string BaristaPackageApprovalsPropertyBagKey = "BaristaPackageApprovals";

        public static BaristaService GetBaristaService(SPFarm farm)
        {
            return farm.Services.GetValue<BaristaService>(BaristaService.ServiceName) ??
                   farm.Services.GetValue<BaristaService>();
        }

        public static BaristaSearchService GetBaristaSearchService(SPFarm farm)
        {
            return farm.Services.GetValue<BaristaSearchService>(BaristaSearchService.NtServiceName) ??
                   farm.Services.GetValue<BaristaSearchService>();
        }

        public static BaristaServiceInstance GetBaristaServiceInstance(SPFarm farm)
        {
            return farm.Services.GetValue<BaristaServiceInstance>(BaristaServiceInstance.ServiceInstanceName) ??
                   farm.Services.GetValue<BaristaServiceInstance>();
        }

        public static BaristaServiceProxy GetBaristaServiceProxy(SPFarm farm)
        {
            return farm.ServiceProxies.GetValue<BaristaServiceProxy>(BaristaServiceProxy.ProxyName) ??
                   farm.ServiceProxies.GetValue<BaristaServiceProxy>();
        }

        public static string GetBaristaWebServicesInstallPath()
        {
            //FIXME: In SP2013 this method is deprecated.
            return SPUtility.GetGenericSetupPath(@"WebServices\Barista");
        }

        /// <summary>
        /// Returns a JObject that contains a listing of barista packages installed on the currently executing server. 
        /// </summary>
        /// <returns></returns>
        public static JObject ListPackages()
        {
            var objResult = new JObject
            {
                {"!machineName", Environment.MachineName}
            };

            //Get all files named "package.json" and return the contents.
            foreach (var file in EnumeratePackageFiles())
            {
                using (var streamReader = file.OpenText())
                {
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        var objPackage = JObject.Load(jsonReader);
                        if (file.Directory != null)
                            objResult.Add(file.Directory.Name, objPackage);
                    }
                }
            }

            return objResult;
        }

        public static void BindBaristaBundles(IKernel kernel, string bundlesDirectory)
        {
            if (kernel == null)
                kernel = new StandardKernel();

            var bundlesDirectoryUri = new Uri("file://" + bundlesDirectory);

            kernel.Bind(x => x
                .FromAssembliesInPath(bundlesDirectory)
                .SelectAllClasses()
                .InheritedFrom<IBundle>()
                .BindAllInterfaces()
                .Configure((c, bundleType) =>
                {
                    var bundleInfo = GetBundleInfo(bundleType, bundlesDirectoryUri);
                    foreach (var info in bundleInfo.Properties())
                        c.WithMetadata(info.Name, info.Value.ToString());
                })
                );

        }

        public static JObject GetBundleInfo(Type bundleType, Uri bundlesDirectoryUri)
        {
            var result = new JObject();

            //Codebase should contain the location (in uri format) of the assembly on disk.
            var codeBase = bundleType.Assembly.CodeBase;
            if (string.IsNullOrEmpty(codeBase))
                return result;

            try
            {
                var codeBaseUri = new Uri(codeBase);
                if (!bundlesDirectoryUri.IsBaseOf(codeBaseUri))
                    return result;

                var packageId = codeBaseUri.Segments.ElementAtOrDefault(bundlesDirectoryUri.Segments.Length);
                var assemblyPath = String.Join(char.ToString(Path.DirectorySeparatorChar), codeBaseUri.Segments.Skip(bundlesDirectoryUri.Segments.Length + 1).ToArray());
                if (string.IsNullOrEmpty(packageId))
                    return result;

                packageId = packageId.TrimEnd('/');
                result.Add("baristaPackageId", packageId);

                result.Add("bundleTypeFullName", bundleType.FullName);
                result.Add("assemblyPath", assemblyPath);
                result.Add("assemblyFullName", bundleType.Assembly.FullName);
                result.Add("assemblyImageRuntimeVersion", bundleType.Assembly.ImageRuntimeVersion);

                var assemblyHash = bundleType.Assembly.Evidence.OfType<Hash>().FirstOrDefault();
                if (assemblyHash == null)
                    return result;

                result.Add("assemblyHash", Convert.ToBase64String(assemblyHash.MD5));
            }
            catch (Exception)
            {
                //Do nothing!
            }

            return result;
        }
        /// <summary>
        /// For the given package id, returns an object that describes the bundles contained in the package.
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public static JObject GetPackageInfo(string packageId)
        {
            var installPath = GetBaristaWebServicesInstallPath();
            var packagesPath = Path.Combine(installPath, "bin" + Path.DirectorySeparatorChar + "bundles");
            var packagePath = Path.Combine(packagesPath, packageId);

            var result = new JObject
            {
                {"!machineName", Environment.MachineName}
            };

            if (!Directory.Exists(packagePath))
                return result;

            var bundlesDirectoryUri = new Uri("file://" + packagesPath);
            var packageDirectory = new DirectoryInfo(packagePath);

            result.Add("packageCreationTime", packageDirectory.CreationTimeUtc);
            result.Add("packageModificationTime", packageDirectory.LastWriteTimeUtc);

            var bundleInfos = new JArray();

            var kernel = new StandardKernel();
            BindBaristaBundles(kernel, packagePath);

            var bundles = kernel.GetAll<IBundle>();
            foreach (var bundle in bundles)
            {
                bundleInfos.Add(GetBundleInfo(bundle.GetType(), bundlesDirectoryUri));
            }
            result.Add("bundles", bundleInfos);

            return result;
        }

        public static BaristaServiceApplication GetCurrentServiceApplication()
        {
            if (SPServiceContext.Current == null)
                throw new InvalidOperationException(
                    "The Current SPServiceContext is null. Please ensure execution is from from a SPServiceContextScope.");

            var baristaServiceApplicationProxy =
                SPServiceContext.Current.GetDefaultProxy(typeof(BaristaServiceApplicationProxy)) as
                    BaristaServiceApplicationProxy;

            if (baristaServiceApplicationProxy == null)
                throw new InvalidOperationException("Internal Error: Unable to obtain default Barista service application proxy.");

            var uri = baristaServiceApplicationProxy.ServiceEndpointUri;
            var serviceApplicationIdString = uri.LocalPath.Split(':').Last();
            Guid serviceApplicationId;
            if (!serviceApplicationIdString.TryParseGuid(out serviceApplicationId))
                throw new InvalidOperationException(
                    "Cannot execute Barista: Unable to determine Barista Service Application Id from current service context.");

            var baristaServiceApplication =
                SPFarm.Local.Services.GetValue<BaristaServiceApplication>(serviceApplicationId);

            return baristaServiceApplication;
        }

        /// <summary>
        /// For the given index name, obtains the directory object as defined in the service application property bag. If no directory with the given name has been defined, returns null.
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public static BaristaIndexDefinition GetBaristaIndexDefinitionFromIndexName(string indexName)
        {
            if (indexName.IsNullOrWhiteSpace())
                throw new ArgumentNullException("indexName", @"A directory name must be specified.");

            var baristaServiceApplication = GetCurrentServiceApplication();

            if (!baristaServiceApplication.Properties.ContainsKey(BaristaSearchIndexDefinitionsPropertyBagKey))
                return null;

            var indexDefinitions =
                Convert.ToString(baristaServiceApplication.Properties[BaristaSearchIndexDefinitionsPropertyBagKey]);
            if (indexDefinitions.IsNullOrWhiteSpace())
                return null;

            var indexDefinitionCollection = JsonConvert.DeserializeObject<IList<IndexDefinition>>(indexDefinitions);
            var indexDefinitionToUse =
                indexDefinitionCollection.FirstOrDefault(
                    id => string.Equals(id.IndexName, indexName, StringComparison.InvariantCultureIgnoreCase));

            if (indexDefinitionToUse == null)
                return null;

            return new BaristaIndexDefinition
            {
                IndexName = indexDefinitionToUse.IndexName,
                TypeName = indexDefinitionToUse.TypeName,
                IndexStoragePath = indexDefinitionToUse.IndexStoragePath,
                Description = indexDefinitionToUse.Description
            };
        }

        ////Lets create the Directory object from the index definition!!
        //    var directoryType = Type.GetType(indexDefinitionToUse.TypeName, false, true);
        //    if (directoryType == null)
        //        throw new InvalidOperationException(
        //          string.Format("An index definition named {0} was located, however, the type {1} could not be found.",
        //                        indexName, indexDefinitionToUse.TypeName));

        //    if (typeof(Lucene.Net.Store.Directory).IsAssignableFrom(directoryType) == false)
        //        throw new InvalidOperationException(
        //          string.Format("An index definition named {0} was located, however, the type {1} is not a directory type.",
        //                        indexName, indexDefinitionToUse.TypeName));

        //    //I know, I know, we went to all the trouble of doing DI, only to hard code the types...
        //    if (directoryType == typeof(Barista.SharePoint.Search.SPDirectory))
        //    {
        //        SPSite site = null;
        //        SPWeb web = null;

        //        //Test for the existance of the target index.
        //        try
        //        {
        //            SPFolder folder;
        //            if (SPHelper.TryGetSPFolder(indexDefinitionToUse.IndexStoragePath, out site, out web, out folder) == false)
        //                throw new InvalidOperationException(
        //                  String.Format(
        //                    "An SharePoint index definition named {0} was located, however, the target index location {1} is not valid.",
        //                    indexName, indexDefinitionToUse.IndexStoragePath));
        //        }
        //        finally
        //        {
        //            if (web != null)
        //                web.Dispose();

        //            if (site != null)
        //                site.Dispose();
        //        }

        //        return new SPDirectory(indexDefinitionToUse.IndexStoragePath);
        //    }

        //    if (directoryType == typeof(Lucene.Net.Store.SimpleFSDirectory))
        //    {
        //        var di = new DirectoryInfo(indexDefinitionToUse.IndexStoragePath);
        //        if (di.Exists == false)
        //            di.Create();

        //        return new Lucene.Net.Store.SimpleFSDirectory(di);
        //    }

        //    if (directoryType == typeof(Lucene.Net.Store.RAMDirectory))
        //    {
        //        return new Lucene.Net.Store.RAMDirectory();
        //    }

        //    //A little bit of extensibility...
        //    var directory = (Lucene.Net.Store.Directory)Activator.CreateInstance(directoryType, indexDefinitionToUse.IndexStoragePath);
        //    return directory;
        //}

        /// <summary>
        /// Checks the current context against the trusted locations as defined in the farm property bag. If the current location is not trusted, throws an exception.
        /// </summary>
        public static void EnsureExecutionInTrustedLocation()
        {
            //CA is always trusted.
            if (SPAdministrationWebApplication.Local.Id == SPContext.Current.Site.WebApplication.Id)
                return;

            var currentUri = new Uri(SPContext.Current.Web.Url.ToLowerInvariant().EnsureEndsWith("/"));
            if (SPAdministrationWebApplication.Local.AlternateUrls.Any(u => u != null && u.Uri != null && u.Uri.IsBaseOf(currentUri)))
                return;

            //Get configuration from the barista service application associated with the current context.
            var baristaServiceApplication = GetCurrentServiceApplication();

            if (!baristaServiceApplication.Properties.ContainsKey(BaristaTrustedLocationsPropertyBagKey))
                throw new SecurityAccessDeniedException(
                    "Cannot execute Barista: Barista Trusted Location settings are not defined on the service application. Please contact your farm administrator to define Barista Trusted Locations.");

            var trustedLocationsString = Convert.ToString(baristaServiceApplication.Properties[BaristaTrustedLocationsPropertyBagKey]);

            if (string.IsNullOrEmpty(trustedLocationsString))
                throw new SecurityAccessDeniedException(
                  "Cannot execute Barista: Barista Trusted Location settings was defined, but empty. Please contact your farm administrator to set Barista Trusted Locations.");

            AssertTrustedLocation(trustedLocationsString, currentUri);
        }

        private static IEnumerable<FileInfo> EnumeratePackageFiles()
        {
            var installPath = GetBaristaWebServicesInstallPath();
            var bundlePath = Path.Combine(installPath, "bin/bundles");
            var di = new DirectoryInfo(bundlePath);
            return di.EnumerateAllFiles().Where(f => string.Equals("package.json", f.Name, StringComparison.OrdinalIgnoreCase));
        }

        private static void AssertTrustedLocation(string trustedLocations, Uri currentUri)
        {
            bool trusted;

            try
            {
                var trustedLocationsJsonArray = JArray.Parse(trustedLocations);
                var trustedLocationsCollection = trustedLocationsJsonArray.OfType<JObject>().Select(trustedLocation =>
                {
                    var trustedLocationUrl =
                        new Uri(trustedLocation["Url"].ToString().ToLowerInvariant().EnsureEndsWith("/"),
                            UriKind.Absolute);

                    var trustChildren = trustedLocation["TrustChildren"].ToObject<Boolean>();

                    return new
                    {
                        trustedLocationUrl,
                        trustChildren
                    };
                });

                trusted =
                    trustedLocationsCollection.Any(
                        trustedLocation =>
                            trustedLocation.trustChildren && trustedLocation.trustedLocationUrl.IsBaseOf(currentUri) ||
                            trustedLocation.trustedLocationUrl.Equals(currentUri));
            }
            catch (Exception ex)
            {
                throw new SecurityAccessDeniedException(string.Format("Cannot execute Barista: Error parsing trusted locations ({0}). Please relay this information to your farm administrator.", ex.Message));
            }

            if (trusted == false)
                throw new SecurityAccessDeniedException(string.Format("Cannot execute Barista: The current location is not trusted ({0}). Contact your farm administrator to add the current location to the trusted Urls in the management section of the Barista service application.", currentUri));
        }
    }
}
