namespace Barista.SharePoint.Library
{
  using Barista.Extensions;
  using Barista.Library;
  using Barista.SharePoint.Services;
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Xml;
  using System.Xml.Linq;

  [Serializable]
  public class DocumentInstance : ObjectInstance
  {
    public DocumentInstance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "html2Pdf")]
    public Base64EncodedByteArrayInstance Html2Pdf(string html)
    {
      HtmlConverterService service = new HtmlConverterService();
      var result = service.ConvertHtmlToPdfInternal(new List<string> { html }, null);
      var byteResult =  new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, result)
        {
          MimeType = "application/pdf"
        };
      return byteResult;
    }

    [JSFunction(Name = "xml2Json")]
    public object Xml2Json(object xml)
    {
      if (xml == null || xml == Null.Value || xml == Undefined.Value)
        return null;

      string xmlString;
      if (xml is Base64EncodedByteArrayInstance)
        xmlString = (xml as Base64EncodedByteArrayInstance).ToUtf8String();
      else
        xmlString = xml.ToString();

      var doc = XDocument.Parse(xmlString);
      var jsonDocument = JsonConvert.SerializeXmlNode(doc.Root.GetXmlNode());
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
  }
}
