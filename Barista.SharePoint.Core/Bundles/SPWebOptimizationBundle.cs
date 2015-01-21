namespace Barista.SharePoint.Bundles
{
    using System;
    using Barista.Bundles;
    using Barista.Jurassic;
    using Barista.Library;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Utilities;

    public class SPWebOptimizationBundle : WebOptimizationBundle
    {
        public override object InstallBundle(ScriptEngine engine)
        {
            var bundlerInstance = new WebOptimizationInstance(engine)
            {
                FileKeyPrefix = () => SPBaristaContext.Current.Web.Url,
                GetAbsoluteUrlFromPath = fileName =>
                {
                    string result;
                    Uri codeUri;
                    if (Uri.TryCreate(fileName, UriKind.RelativeOrAbsolute, out codeUri))
                    {
                        if (codeUri.IsAbsoluteUri)
                            result = codeUri.ToString();
                        else
                        {
                            fileName = StringHelper.ResolveParentPaths(fileName);
                            SPFile file;
                            if (SPHelper.TryGetSPFile(fileName, out file))
                            {
                                if (file.Item != null)
                                    result = (string)file.Item[SPBuiltInFieldId.EncodedAbsUrl];
                                else
                                    result = SPBaristaContext.Current.Web.Url + "/" + file.Url;
                            }
                            else
                            {
                                var error = string.Format("Bundle error: The file '{0}' doesn't exist", fileName);
                                throw new JavaScriptException(engine, "Error", error);
                            }
                        }
                    }
                    else
                    {
                        var error = string.Format("Bundle error: The location of file '{0}' could not be determined", fileName);
                        throw new JavaScriptException(engine, "Error", error);
                    }

                    return result;
                },
                GetLastModifiedDate = fileName =>
                {
                    DateTime? result;
                    Uri codeUri;
                    var url = SPUtility.ConcatUrls(SPBaristaContext.Current.Web.Url, fileName);
                    if (Uri.TryCreate(url, UriKind.Absolute, out codeUri))
                    {
                        SPFile file;
                        if (SPHelper.TryGetSPFile(codeUri.ToString(), out file))
                        {
                            result = file.TimeLastModified;
                        }
                        else
                        {
                            var error = string.Format("Bundle error: The file '{0}' doesn't exist", codeUri.ToString());
                            throw new JavaScriptException(engine, "Error", error);
                        }
                    }
                    else
                    {
                        var error = string.Format("Bundle error: The location of file '{0}' could not be determined", fileName);
                        throw new JavaScriptException(engine, "Error", error);
                    }

                    return result.Value;
                },
                ReadAllText = fileName =>
                {
                    string result;
                    Uri codeUri;
                    var url = SPUtility.ConcatUrls(SPBaristaContext.Current.Web.Url, fileName);
                    if (Uri.TryCreate(url, UriKind.Absolute, out codeUri))
                    {
                        string scriptFilePath;
                        bool isHiveFile;
                        String codeFromfile;
                        if (SPHelper.TryGetSPFileAsString(codeUri.ToString(), out scriptFilePath, out codeFromfile, out isHiveFile))
                        {
                            result = codeFromfile;
                        }
                        else
                        {
                            var error = string.Format("Bundle error: The file '{0}' doesn't exist", codeUri.ToString());
                            throw new JavaScriptException(engine, "Error", error);
                        }
                    }
                    else
                    {
                        var error = string.Format("Bundle error: The location of file '{0}' could not be determined", fileName);
                        throw new JavaScriptException(engine, "Error", error);
                    }

                    return result;
                }
            };

            return bundlerInstance;
        }
    }
}
