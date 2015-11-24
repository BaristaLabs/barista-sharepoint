namespace Barista.Library
{
    using System.IO;
    using System.Linq;
    using Barista.Csv;
    using Barista.Extensions;
    using Jurassic;
    using Jurassic.Library;
    using Barista.Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Text;

    [Serializable]
    public class DocumentInstance : ObjectInstance
    {
        public DocumentInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        /// <summary>
        /// Gets or sets the Winnovative HtmlToPdf Converter License key that is used for Html to Pdf conversions.
        /// </summary>
        /// <remarks>
        /// If a key is not specified, any generated pdfs will contain a trial watermark.
        /// </remarks>
        public string WinnovativeHtmlToPdfConverterLicenseKey
        {
            get;
            set;
        }

        [JSFunction(Name = "html2Pdf")]
        public Base64EncodedByteArrayInstance Html2Pdf(string html, object pdfAttachments)
        {
            IEnumerable<PdfAttachmentInstance> pdfAttachmentInstances = null;
            if (pdfAttachments != null && pdfAttachments != Undefined.Value && pdfAttachments != Null.Value &&
                pdfAttachments is ArrayInstance)
                pdfAttachmentInstances = (pdfAttachments as ArrayInstance).ElementValues.OfType<PdfAttachmentInstance>();

            IList<PdfAttachment> attachments = null;
            if (pdfAttachmentInstances != null)
            {
                attachments = pdfAttachmentInstances.Select(p => new PdfAttachment
                  {
                      FileName = p.FileName,
                      FileDisplayName = p.FileDisplayName,
                      Description = p.Description,
                      Data = p.Data != null ? p.Data.Data : null,
                  })
                  .ToList();
            }
            var result = HtmlConverterHelper.ConvertHtmlToPdf(new List<string> { html }, attachments, this.WinnovativeHtmlToPdfConverterLicenseKey);
            var byteResult = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, result)
              {
                  MimeType = "application/pdf"
              };
            return byteResult;
        }

        [JSFunction(Name = "concatenatePdfDocs")]
        public Base64EncodedByteArrayInstance ConcatenatePdfDocuments(string html, object pdfsToAppend)
        {
            IEnumerable<Base64EncodedByteArrayInstance> pdfsToAppendInstances = null;
            if (pdfsToAppend != null && pdfsToAppend != Undefined.Value && pdfsToAppend != Null.Value &&
                pdfsToAppend is ArrayInstance)
                pdfsToAppendInstances = (pdfsToAppend as ArrayInstance).ElementValues.OfType<Base64EncodedByteArrayInstance>();

            IList<byte[]> pdfsToAppendData = null;
            if (pdfsToAppendInstances != null)
            {
                pdfsToAppendData = pdfsToAppendInstances.Select(p => p.Data)
                  .ToList();
            }
            var result = HtmlConverterHelper.MergeFiles(html, pdfsToAppendData, this.WinnovativeHtmlToPdfConverterLicenseKey);
            var byteResult = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, result)
            {
                MimeType = "application/pdf"
            };
            return byteResult;
        }

        [JSFunction(Name = "csv2Json")]
        public object Csv2Json(object csv, object csvOptions)
        {
            if (csv == null || csv == Null.Value || csv == Undefined.Value)
                return null;

            string csvString;
            if (csv is Base64EncodedByteArrayInstance)
                csvString = (csv as Base64EncodedByteArrayInstance).ToUtf8String();
            else
                csvString = TypeConverter.ToString(csv);

            var options = new CsvOptions(this.Engine.Object.InstancePrototype);

            if (csvOptions != null && csvOptions != Undefined.Value && csvOptions != Null.Value)
                options = JurassicHelper.Coerce<CsvOptions>(this.Engine, csvOptions);

            using (var csvReader = CsvReader.FromCsvString(csvString))
            {
                csvReader.PreserveLeadingWhiteSpace = options.PreserveLeadingWhiteSpace;
                csvReader.PreserveTrailingWhiteSpace = options.PreserveTrailingWhiteSpace;
                csvReader.ValueDelimiter = options.ValueDelimiter[0];
                csvReader.ValueSeparator = options.ValueSeparator[0];

                HeaderRecord headerRecord = null;
                if (options.HasHeader)
                    headerRecord = csvReader.ReadHeaderRecord();

                var result = new List<ObjectInstance>();
                DataRecord currentRecord;
                while ((currentRecord = csvReader.ReadDataRecord()) != null)
                {
                    ObjectInstance currentObject;
                    if (headerRecord == null)
                    {
                        // ReSharper disable CoVariantArrayConversion
                        currentObject = this.Engine.Array.Construct(currentRecord.Values.ToArray());
                        // ReSharper restore CoVariantArrayConversion
                    }
                    else
                    {
                        currentObject = this.Engine.Object.Construct();
                        foreach (var key in headerRecord.Values)
                        {
                            currentObject.SetPropertyValue(key, currentRecord[key], false);
                        }
                    }
                    result.Add(currentObject);
                }

                // ReSharper disable CoVariantArrayConversion
                return this.Engine.Array.Construct(result.ToArray());
                // ReSharper restore CoVariantArrayConversion
            }
        }

        [JSFunction(Name = "json2Csv")]
        public object Json2Csv(object array, object csvOptions)
        {
            if (array == null || array == Null.Value || array == Undefined.Value || (array is ArrayInstance) == false || (array as ArrayInstance).Length == 0)
                return Null.Value;

            var options = new CsvOptions(this.Engine.Object.InstancePrototype);

            if (csvOptions != null && csvOptions != Undefined.Value && csvOptions != Null.Value)
                options = JurassicHelper.Coerce<CsvOptions>(this.Engine, csvOptions);

            var jsonArray = array as ArrayInstance;

            HeaderRecord headerRecord = null;
            if (options.HasHeader)
            {
                var firstRecord = jsonArray[0] as ObjectInstance;
                if (firstRecord == null)
                    return Null.Value;

                var keys = firstRecord.Properties
                  .Select(property => property.Name)
                  .ToList();

                headerRecord = new HeaderRecord(keys);
            }

            using (var ms = new MemoryStream())
            {
                using (var csvWriter = new CsvWriter(ms))
                {
                    csvWriter.ValueDelimiter = options.ValueDelimiter[0];
                    csvWriter.ValueSeparator = options.ValueSeparator[0];

                    if (headerRecord != null)
                        csvWriter.WriteHeaderRecord(headerRecord);

                    foreach (var value in jsonArray.ElementValues.OfType<ObjectInstance>())
                    {
                        var currentRecord = new DataRecord(headerRecord);
                        if (headerRecord == null)
                        {
                            foreach (var property in value.Properties)
                            {
                                var propertyValue = String.Empty;
                                if (property.Value != null)
                                    propertyValue = property.Value.ToString();
                                currentRecord.Values.Add(propertyValue);
                            }
                        }
                        else
                        {
                            foreach (var key in headerRecord.Values)
                            {
                                var propertyValue = String.Empty;
                                if (value.HasProperty(key) && value[key] != null)
                                    propertyValue = value[key].ToString();
                                currentRecord.Values.Add(propertyValue);
                            }
                        }

                        csvWriter.WriteDataRecord(currentRecord);
                    }
                    csvWriter.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        [JSFunction(Name = "xml2Json")]
        public object Xml2Json(object xml)
        {
            if (xml == null || xml == Null.Value || xml == Undefined.Value)
                return null;

            XDocument result;
            if (xml is Base64EncodedByteArrayInstance)
            {
                using (MemoryStream oStream = new MemoryStream((xml as Base64EncodedByteArrayInstance).Data)
                {
                    using (XmlTextReader oReader = new XmlTextReader(oStream))
                    {
                        result = XDocument.Load(oReader);
                    }
                }
            }
            else
            {
                var xmlString = TypeConverter.ToString(xml);
                result = XDocument.Parse(xmlString);
            }

            var jsonDocument = JsonConvert.SerializeXmlNode(result.Root.GetXmlNode());
            return JSONObject.Parse(this.Engine, jsonDocument, null);
        }

        [JSFunction(Name = "json2Xml")]
        public string Json2Xml(object jsonObject)
        {
            string text;
            if (jsonObject is ObjectInstance)
                text = JSONObject.Stringify(this.Engine, jsonObject, null, null);
            else
                text = jsonObject as string;

            var document = JsonConvert.DeserializeXmlNode(text);
            return document.OuterXml;
        }

        //[JSFunction(Name = "yaml2Json")]
        //public object Yaml2Json(object yaml)
        //{
        //  if (yaml == null || yaml == Null.Value || yaml == Undefined.Value)
        //    return null;

        //  string yamlString;
        //  if (yaml is Base64EncodedByteArrayInstance)
        //    yamlString = (yaml as Base64EncodedByteArrayInstance).ToUtf8String();
        //  else
        //    yamlString = TypeConverter.ToString(yaml);

        //  var yamlDocument = new YamlDocument(yamlString);

        //  var yamlConverter = new XmlConverter();
        //  var doc = yamlConverter.ToXml(yamlDocument);
        //  var jsonDocument = JsonConvert.SerializeXmlNode(doc.DocumentElement);
        //  return JSONObject.Parse(this.Engine, jsonDocument, null);
        //}

        //[JSFunction(Name = "json2Yaml")]
        //public object Json2Yaml(object jsonObject)
        //{
        //  string text;
        //  if (jsonObject is ObjectInstance)
        //    text = JSONObject.Stringify(this.Engine, jsonObject, null, null);
        //  else
        //    text = jsonObject as string;

        //  var yamlConverter = new XmlConverter();
        //  var xmlDocument = JsonConvert.DeserializeXmlNode(text);
        //  var doc = yamlConverter.FromXml(xmlDocument);
        //  return doc.ToString();
        //}
    }
}
