namespace Barista.SharePoint
{
    using System.Globalization;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Text;

    /// <summary>
    /// Represents a collection of utility methods.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Returns the value of the specified key in the farm property bag.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetFarmKeyValue(string key)
        {
            string val = null;
            var farm = SPFarm.Local;

            if (farm != null && farm.Properties.ContainsKey(key))
            {
                val = Convert.ToString(farm.Properties[key]);
            }
            return val;
        }

        /// <summary>
        /// Sets the value of the specified key in the farm property bag. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetFarmKeyValue(string key, string value)
        {
            var farm = SPFarm.Local;
            if (farm == null)
                return;

            if (farm.Properties.ContainsKey(key))
                farm.Properties[key] = value;
            else
                farm.Properties.Add(key, value);

            farm.Update();
        }

        /// <summary>
        /// For the specified url, returns the associated list item.
        /// </summary>
        /// <param name="absoluteListItemUrl"></param>
        /// <returns></returns>
        public static SPListItem GetListItemFromAbsoluteUrl(string absoluteListItemUrl)
        {
            SPSite sourceSite = null;
            SPWeb sourceWeb = null;

            try
            {
                SPList sourceList;
                return GetListItemFromAbsoluteUrl(absoluteListItemUrl, out sourceSite, out sourceWeb, out sourceList);
            }
            finally
            {
                if (sourceWeb != null)
                    sourceWeb.Dispose();
                if (sourceSite != null)
                    sourceSite.Dispose();
            }
        }

        public static SPListItem GetListItemFromAbsoluteUrl(string absoluteListItemUrl, out SPSite sourceSite, out SPWeb sourceWeb, out SPList sourceList)
        {
            sourceSite = new SPSite(absoluteListItemUrl, SPBaristaContext.Current.Site.UserToken);
            sourceWeb = sourceSite.OpenWeb();

            if (sourceWeb == null)
                throw new InvalidOperationException("The specified web is invalid.");

            sourceList = sourceWeb.GetList(absoluteListItemUrl);

            var query = new SPQuery
            {
                ViewAttributes = "Scope=\"Recursive\""
            };
            var queryString = "<Where>" +
                                "<Eq>" +
                                "<FieldRef Name=\"EncodedAbsUrl\"/>" +
                                "<Value Type=\"Text\">" + absoluteListItemUrl + "</Value>" +
                                "</Eq>" +
                            "</Where>";
            query.Query = queryString;
            var retrievedListItems = sourceList.GetItems(query);
            if (retrievedListItems.Count != 1)
                throw new InvalidOperationException("Unable to locate the specified file in the target list.");

            var listItem = retrievedListItems[0];
            return listItem;
        }

        public static SPUser GetSPUser(SPListItem item, string key)
        {
            var field = item.Fields[key] as SPFieldUser;

            if (field != null && item[key] != null)
            {
                var fieldValue = field.GetFieldValue(item[key].ToString()) as SPFieldUserValue;
                if (fieldValue != null)
                {
                    return fieldValue.User;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the name of a new temporary folder.
        /// </summary>
        /// <returns></returns>
        public static string GetTempFolder()
        {
            string folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            while (Directory.Exists(folder))
            {
                folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
            return folder;
        }

        /// <summary>
        /// Utility method to return a valid file name for the specified title/extension.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string CreateValidFileName(string title, string extension)
        {
            string validFileName = title.Trim();

            validFileName = Path.GetInvalidFileNameChars()
              .Aggregate(validFileName, (current, invalChar) => current.Replace(invalChar.ToString(CultureInfo.InvariantCulture), ""));
            validFileName = Path.GetInvalidPathChars()
              .Aggregate(validFileName, (current, invalChar) => current.Replace(invalChar.ToString(CultureInfo.InvariantCulture), ""));

            if (validFileName.Length > 160) //safe value threshold is 260
                validFileName = validFileName.Remove(156) + "...";

            return validFileName + "." + extension;
        }

        /// <summary>
        /// Returns a temporary file name with the specified extension.
        /// </summary>
        /// <param name="extensionWithDot"></param>
        /// <returns></returns>
        public static string GetTempFileName(string extensionWithDot)
        {
            string tempFileName;
            do
            {
                tempFileName = Path.GetTempFileName();

                var extension = Path.GetExtension(tempFileName);
                if (String.IsNullOrEmpty(extension) == false && extensionWithDot != null)
                {
                    tempFileName = tempFileName.Replace(extension, extensionWithDot);
                }
            }
            while (File.Exists(tempFileName));
            return tempFileName;
        }

        /// <summary>
        /// Encodes a string to be represented as a string literal. The format
        /// is essentially a JSON string.
        /// 
        /// The string returned includes outer quotes 
        /// Example Output: "Hello \"Rick\"!\r\nRock on"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string EncodeJsString(string s)
        {
            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        int i = c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }

        /// <summary>
        /// Executes the specified PowerShell script.
        /// </summary>
        /// <param name="scriptText"></param>
        /// <returns></returns>
        public static Collection<PSObject> ExecutePowerShellScript(string scriptText)
        {
            InitialSessionState iss = InitialSessionState.CreateDefault();
            PSSnapInException warning;
            iss.ImportPSSnapIn("Microsoft.SharePoint.PowerShell", out warning);
            Collection<PSObject> results;

            // create Powershell runspace 
            using (var runspace = RunspaceFactory.CreateRunspace(iss))
            {
                // open it 
                runspace.Open();

                try
                {
                    // create a pipeline and feed it the script text 
                    var pipeline = runspace.CreatePipeline();
                    pipeline.Commands.AddScript(scriptText);
                    pipeline.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                    try
                    {
                        // execute the script 
                        results = pipeline.Invoke();
                    }
                    catch (Exception ex)
                    {
                        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.PowerShell, "Unexpected Error while executing PowerShell Script:");
                        throw;
                    }
                }
                finally
                {
                    runspace.Close();
                }
            }

            //Return the results.
            return results;
        }
    }

    public static class EnumExtensions
    {
        public static IEnumerable<Enum> GetFlags(this Enum value)
        {
            return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
        }

        public static IEnumerable<Enum> GetIndividualFlags(this Enum value)
        {
            return GetFlags(value, GetFlagValues(value.GetType()).ToArray());
        }

        private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
        {
            var bits = Convert.ToUInt64(value);
            var results = new List<Enum>();
            for (var i = values.Length - 1; i >= 0; i--)
            {
                var mask = Convert.ToUInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;

                if ((bits & mask) != mask)
                    continue;

                results.Add(values[i]);
                bits -= mask;
            }
            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<Enum>();
        }

        private static IEnumerable<Enum> GetFlagValues(Type enumType)
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                var bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    yield return value;

                while (flag < bits)
                    flag <<= 1;

                if (flag == bits)
                    yield return value;
            }
        }
    }
}