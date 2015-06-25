namespace Barista.SharePoint.Bundles
{
    using Barista.Library;
    using System;
    using Barista.SharePoint.Library;

    [Serializable]
    public class SPDocumentBundle : IBundle
    {
        public bool IsSystemBundle
        {
            get { return true; }
        }

        public string BundleName
        {
            get { return "Document"; }
        }

        public string BundleDescription
        {
            get { return "Document Bundle. Provides a mechanism to interact with various document formats: CSV, Excel, Zip."; }
        }

        public object InstallBundle(Jurassic.ScriptEngine engine)
        {
            engine.SetGlobalValue("ExcelDocument", new SPExcelDocumentConstructor(engine));
            engine.SetGlobalValue("ZipFile", new ZipFileConstructor(engine));
            engine.SetGlobalValue("PdfAttachment", new PdfAttachmentConstructor(engine));

            string pdfConverterLicenseKey = null;
            if (SPBaristaContext.HasCurrentContext && SPBaristaContext.Current.Web != null && SPBaristaContext.Current.Web.Properties.ContainsKey("Winnovative_HtmlToPdfConverter"))
                pdfConverterLicenseKey = Convert.ToString(SPBaristaContext.Current.Web.Properties["Winnovative_HtmlToPdfConverter"]);

            if (String.IsNullOrEmpty(pdfConverterLicenseKey))
                pdfConverterLicenseKey = Barista.SharePoint.Utilities.GetFarmKeyValue("Winnovative_HtmlToPdfConverter");

            return new DocumentInstance(engine)
              {
                  WinnovativeHtmlToPdfConverterLicenseKey = pdfConverterLicenseKey
              };
        }
    }
}
