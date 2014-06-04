namespace Barista.SharePoint.EventReceivers.BaristaRemoteItemEventReceiver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Barista.Extensions;
    using Barista.Newtonsoft.Json;
    using Barista.Newtonsoft.Json.Linq;
    using Barista.Newtonsoft.Json.Serialization;
    using Barista.SharePoint.Annotations;
    using Microsoft.SharePoint;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// List Item Events
    /// </summary>
    public class BaristaRemoteItemEventReceiver : SPItemEventReceiver
    {
        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            Post(properties);
            base.ItemAdded(properties);
        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            Post(properties);
            base.ItemUpdated(properties);
        }

        /// <summary>
        /// An item was deleted.
        /// </summary>
        public override void ItemDeleted(SPItemEventProperties properties)
        {
            Post(properties);
            base.ItemDeleted(properties);
        }

        /// <summary>
        /// An item was checked in.
        /// </summary>
        public override void ItemCheckedIn(SPItemEventProperties properties)
        {
            Post(properties);
            base.ItemCheckedIn(properties);
        }

        /// <summary>
        /// An item was checked out.
        /// </summary>
        public override void ItemCheckedOut(SPItemEventProperties properties)
        {
            Post(properties);
            base.ItemCheckedOut(properties);
        }

        /// <summary>
        /// An item was unchecked out.
        /// </summary>
        public override void ItemUncheckedOut(SPItemEventProperties properties)
        {
            Post(properties);
            base.ItemUncheckedOut(properties);
        }

        /// <summary>
        /// An attachment was added to the item.
        /// </summary>
        public override void ItemAttachmentAdded(SPItemEventProperties properties)
        {
            Post(properties);
            base.ItemAttachmentAdded(properties);
        }

        /// <summary>
        /// An attachment was removed from the item.
        /// </summary>
        public override void ItemAttachmentDeleted(SPItemEventProperties properties)
        {
            Post(properties);
            base.ItemAttachmentDeleted(properties);
        }

        /// <summary>
        /// A file was moved.
        /// </summary>
        public override void ItemFileMoved(SPItemEventProperties properties)
        {
            Post(properties);
            base.ItemFileMoved(properties);
        }

        /// <summary>
        /// A file was converted.
        /// </summary>
        public override void ItemFileConverted(SPItemEventProperties properties)
        {
            Post(properties);
            base.ItemFileConverted(properties);
        }

        /// <summary>
        /// Posts to a specified url in response to an item event.
        /// </summary>
        /// <param name="properties">The properties.</param>
        private static void Post(SPItemEventProperties properties)
        {
            using (var web = properties.OpenWeb())
            {
                var list = web.Lists[properties.ListId];
                var postBodyObject = new PostData
                {
                    Properties = properties,
                    ListItem = BaristaRemoteItemEventReceiver.GetFieldValuesAsObject(list.GetItemById(properties.ListItemId))
                };

                var request = (HttpWebRequest) WebRequest.Create(properties.ReceiverData);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.AllowAutoRedirect = true;
                request.UseDefaultCredentials = true;
                request.Proxy = WebRequest.GetSystemWebProxy();
                
                var jsonProperties = JsonConvert.SerializeObject(postBodyObject, new JsonSerializerSettings {
                        ContractResolver = new FilteredContractResolver("Web", "List", "Site", "ListItem")
                    });

                
                var data = Encoding.UTF8.GetBytes(jsonProperties);
                request.ContentLength = data.Length;


                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                }

                // now put the get response code in a new thread and immediately return
                ThreadPool.QueueUserWorkItem(x =>
                {
                    try
                    {
                        using (var objResponse = (HttpWebResponse) request.GetResponse())
                        {
                            var responseStream = new MemoryStream();
                            objResponse.GetResponseStream().CopyTo(responseStream);
                            responseStream.Seek(0, SeekOrigin.Begin);
                        }
                    }
                    catch
                    {
                        //Do Nothing.
                    }
                });
            }
        }

        private class PostData
        {
            [JsonProperty("properties")]
            public SPItemEventProperties Properties
            {
                [UsedImplicitly]
                get;
                set;
            }

            [JsonProperty("listItem")]
            public JObject ListItem
            {
                [UsedImplicitly]
                get;
                set;
            }
        }

        private class FilteredContractResolver : DefaultContractResolver
        {
            private readonly List<string> m_propertiesToIgnore = new List<string>();

            public FilteredContractResolver(params string[] properties)
            {
                m_propertiesToIgnore.AddRange(properties);
            }


            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization)
                    .Where(p => !m_propertiesToIgnore.Contains(p.PropertyName))
                    .ToList();
            }
        }

        private static readonly Regex DigitRegex = new Regex(@"^\d+;#.*$", RegexOptions.Compiled);

        public static JObject GetFieldValuesAsObject(SPListItem listItem)
        {
            if (listItem == null)
                return null;

            var result = new JObject();

            var fields = listItem.Fields;

            foreach (var field in fields.OfType<SPField>())
            {
                switch (field.Type)
                {
                    case SPFieldType.Integer:
                        {
                            int value;
                            if (listItem.TryGetSPFieldValue(field.Id, out value))
                            {
                                result.Add(field.InternalName, new JValue(value));
                            }
                        }
                        break;
                    case SPFieldType.Boolean:
                        {
                            bool value;
                            if (listItem.TryGetSPFieldValue(field.Id, out value))
                            {
                                result.Add(field.InternalName, new JValue(value));
                            }
                        }
                        break;
                    case SPFieldType.Number:
                        {
                            double value;
                            if (listItem.TryGetSPFieldValue(field.Id, out value))
                            {
                                result.Add(field.InternalName, new JValue(value));
                            }
                        }
                        break;
                    case SPFieldType.DateTime:
                        {
                            DateTime value;
                            if (listItem.TryGetSPFieldValue(field.Id, out value))
                            {
                                result.Add(field.InternalName, new JValue(value));
                            }
                        }
                        break;
                    case SPFieldType.URL:
                        {
                            string urlFieldValue;
                            if (listItem.TryGetSPFieldValue(field.Id, out urlFieldValue))
                            {
                                var urlValue = new SPFieldUrlValue(urlFieldValue);

                                var item = new JObject
                                {
                                    {"description", new JValue(urlValue.Description)},
                                    {"url", new JValue(urlValue.Url)}
                                };

                                result.Add(field.InternalName, item);
                            }
                        }
                        break;
                    case SPFieldType.User:
                        {
                            string userToken;
                            if (listItem.TryGetSPFieldValue(field.Id, out userToken))
                            {
                                var fieldUserValue = new SPFieldUserValue(listItem.ParentList.ParentWeb, userToken);
                                result.Add(field.InternalName, new JValue(fieldUserValue.User.LoginName));
                            }
                        }
                        break;
                    case SPFieldType.Lookup:
                        {
                            var fieldType = field as SPFieldLookup;
                            if (fieldType == null)
                                continue;

                            if (fieldType.AllowMultipleValues)
                            {
                                object fv;
                                if (!listItem.TryGetSPFieldValue(field.Id, out fv))
                                    continue;

                                var fieldValue = fv as SPFieldLookupValueCollection;

                                var array = new JArray();

                                if (fieldValue != null)
                                {
                                    foreach (var lookupValue in fieldValue)
                                    {
                                        var item = new JObject
                                        {
                                            {"lookupId", new JValue(lookupValue.LookupId)},
                                            {"lookupValue", new JValue(lookupValue.LookupValue)}
                                        };

                                        array.Add(item);
                                    }
                                }

                                result.Add(field.InternalName, array);
                            }
                            else
                            {
                                object fieldValue;
                                if (!listItem.TryGetSPFieldValue(field.Id, out fieldValue))
                                    continue;

                                if (fieldValue is SPFieldUrlValue)
                                {
                                    var urlValue = fieldValue as SPFieldUrlValue;
                                    var item = new JObject
                                    {
                                        {"description", new JValue(urlValue.Description)},
                                        {"url", new JValue(urlValue.Url)}
                                    };

                                    result.Add(field.InternalName, item);
                                }
                                else if (fieldValue is DateTime)
                                {
                                    var value = (DateTime)fieldValue;
                                    result.Add(field.InternalName, new JValue(value));
                                }
                                else if (fieldValue is SPFieldUserValue)
                                {
                                    var fieldUserValue = (SPFieldUserValue)fieldValue;
                                    result.Add(field.InternalName, new JValue(fieldUserValue.User.LoginName));
                                }
                                else if (fieldValue is Guid)
                                {
                                    var guidValue = (Guid)fieldValue;
                                    result.Add(field.InternalName, new JValue(guidValue));
                                }
                                else if (fieldValue is string)
                                {
                                    //Attempt to create a new SPFieldLookupValue from the string
                                    if (DigitRegex.IsMatch((string)fieldValue, 0))
                                    {
                                        try
                                        {
                                            var lookupValue = new SPFieldLookupValue((string)fieldValue);

                                            var item = new JObject
                                            {
                                                {"lookupId", new JValue(lookupValue.LookupId)},
                                                {"lookupValue", new JValue(lookupValue.LookupValue)}
                                            };
                                            result.Add(field.InternalName, item);
                                        }
                                        catch (ArgumentException)
                                        {
                                            result.Add(field.InternalName, new JValue(fieldValue));
                                        }
                                    }
                                    else
                                    {
                                        result.Add(field.InternalName, new JValue(fieldValue));
                                    }
                                }
                                else
                                {
                                    result.Add(field.InternalName, new JValue(fieldValue));
                                }
                            }
                        }
                        break;
                    default:
                        {
                            object value;
                            if (listItem.TryGetSPFieldValue(field.Id, out value))
                            {
                                var stringValue = field.GetFieldValueAsText(value);

                                JToken existing;
                                if (result.TryGetValue(field.InternalName, out existing) == false)
                                    result.Add(field.InternalName, new JValue(stringValue));
                            }
                        }
                        break;
                }
            }

            return result;
        }
    }
}