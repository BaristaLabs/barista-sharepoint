namespace Barista.SharePoint.Library
{
  using Barista.Extensions;
  using Barista.Library;
  using Barista.SharePoint.Services;
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;
  using System.Collections.Generic;
  using System.Xml;
  using System.Xml.Linq;

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
      var result = service.ConvertHtmlToPDFInternal(new List<string>() { html }, null);
      var byteResult =  new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, result);
      byteResult.MimeType = "application/pdf";
      return byteResult;
    }

    [JSFunction(Name = "xml2Json")]
    public object Xml2Json(string xml)
    {
      XDocument doc = XDocument.Parse(xml);
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

      XmlDocument document = JsonConvert.DeserializeXmlNode(text);
      return document.OuterXml;
    }
  }
}
