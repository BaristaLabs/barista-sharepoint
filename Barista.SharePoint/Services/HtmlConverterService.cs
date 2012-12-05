using System.Globalization;

namespace Barista.SharePoint.Services
{
  using iTextSharp.text;
  using iTextSharp.text.html.simpleparser;
  using iTextSharp.text.pdf;

  using Microsoft.SharePoint.Client.Services;

  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;

  /// <summary>
  /// Represents a service that converts Html to various formats
  /// </summary>
  [BasicHttpBindingServiceMetadataExchangeEndpoint]
  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public sealed class HtmlConverterService
  {
    [OperationContract]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    public Stream ConvertHtmlToPdf(string html, string fileName)
    {
      if (WebOperationContext.Current == null)
        throw new InvalidOperationException("This operation only supports REST-based requests.");

      byte[] pdfBuffer = ConvertHtmlToPDFInternal(new List<string> { html }, null);

      var response = WebOperationContext.Current.OutgoingResponse;
      response.ContentType = "application/pdf";
      response.Headers.Add("Content-Disposition", String.Format("filename=\"{0}\"", fileName + ".pdf"));
      response.Headers.Add("Content-Length", pdfBuffer.LongLength.ToString(CultureInfo.InvariantCulture));
      return new MemoryStream(pdfBuffer);
    }

    /// <summary>
    /// Converts the specified html documents to a single pdf file.
    /// </summary>
    /// <param name="htmlDocuments"></param>
    /// <returns></returns>
    public byte[] ConvertHtmlToPDFInternal(IEnumerable<string> htmlDocuments, IEnumerable<InfoPathAttachment> pdfAttachments)
    {
      Winnovative.WnvHtmlConvert.PdfConverter pdfConverter = new Winnovative.WnvHtmlConvert.PdfConverter
        {
          PdfStandardSubset = Winnovative.WnvHtmlConvert.PdfStandardSubset.Pdf_A_1b
        };

      string pdfConverterLicenseKey = Barista.SharePoint.Utilities.GetFarmKeyValue("Winnovative_HtmlToPdfConverter");
      if (string.IsNullOrEmpty(pdfConverterLicenseKey) == false)
        pdfConverter.LicenseKey = pdfConverterLicenseKey;

      Winnovative.WnvHtmlConvert.PdfDocument.Document doc = null;
      try
      {
        foreach (var htmlDocument in htmlDocuments)
        {
          if (doc == null)
            doc = pdfConverter.GetPdfDocumentObjectFromHtmlString(htmlDocument);
          else
            doc.AppendDocument(pdfConverter.GetPdfDocumentObjectFromHtmlString(htmlDocument));
        }

        if (doc == null)
          throw new InvalidOperationException("No PDF Document Object was created.");

        //Use iTextSharp to add all attachments
        var infoPathAttachments = pdfAttachments as InfoPathAttachment[] ?? pdfAttachments.ToArray();
        if (pdfAttachments != null && infoPathAttachments.Any())
        {
          PdfReader reader = new PdfReader(doc.Save());

          using (MemoryStream outputPdfStream = new MemoryStream())
          {
            var stamper = new PdfStamper(reader, outputPdfStream);
            foreach (var pdfAttachment in infoPathAttachments)
            {
              stamper.AddFileAttachment(pdfAttachment.Description, pdfAttachment.Data, pdfAttachment.FileName, pdfAttachment.FileDisplayName);
            }
            stamper.Close();
            outputPdfStream.Flush();
            return outputPdfStream.GetBuffer();
          }
        }
        else
        {
          return doc.Save();
        }
      }
      finally
      {
        if (doc != null)
          doc.Close();
      }
    }

    /// <summary>
    /// Converts Html to PDF using iTextSharp.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    private byte[] ConvertHtmlToPdf_ITextSharp(string html)
    {
      using (MemoryStream pdfStream = new MemoryStream())
      {
        Document pdfDocument = new iTextSharp.text.Document(PageSize.LETTER, 72, 72, 72, 72);

        //TODO: Retrieve the page settings from either the view definition or the style within the xsl.
        var writer = PdfWriter.GetInstance(pdfDocument, pdfStream);
        writer.CloseStream = false;

        pdfDocument.Open();

        var elements = HTMLWorker.ParseToList(new StringReader(html), new StyleSheet());
        foreach (var e in elements)
        {
          pdfDocument.Add(e);
        }

        if (pdfDocument.IsOpen())
          pdfDocument.Close();

        pdfStream.Flush();
        pdfStream.Seek(0, SeekOrigin.Begin);
        return pdfStream.GetBuffer();
      }
    }

    /// <summary>
    /// Converts the specified html documents to a single image.
    /// </summary>
    /// <param name="htmlDocuments"></param>
    /// <returns></returns>
    public System.Drawing.Image ConvertHtmlToImageInternal(IEnumerable<string> htmlDocuments)
    {
      Winnovative.WnvHtmlConvert.ImgConverter imgConverter = new Winnovative.WnvHtmlConvert.ImgConverter();

      string pdfConverterLicenseKey = Barista.SharePoint.Utilities.GetFarmKeyValue("Winnovative_HtmlToPdfConverter");
      if (string.IsNullOrEmpty(pdfConverterLicenseKey) == false)
        imgConverter.LicenseKey = pdfConverterLicenseKey;

      System.Drawing.Image img = null;
      foreach (var htmlDocument in htmlDocuments)
      {
        if (img == null)
          img = imgConverter.GetImageFromHtmlString(htmlDocument, System.Drawing.Imaging.ImageFormat.Png);
        else
        {
          using (System.Drawing.Image tempImage = imgConverter.GetImageFromHtmlString(htmlDocument, System.Drawing.Imaging.ImageFormat.Png))
          {
            int newImageHeight = img.Height + tempImage.Height;
            int newImageWidth = img.Width > tempImage.Width ? img.Width : tempImage.Width;

            System.Drawing.Bitmap newBitmap = new System.Drawing.Bitmap(newImageWidth, newImageHeight);
            using (System.Drawing.Graphics newImageGraphics = System.Drawing.Graphics.FromImage(newBitmap))
            {
              newImageGraphics.DrawImageUnscaled(img, 0, 0);
              newImageGraphics.DrawImageUnscaled(tempImage, 0, newImageHeight);
            }
            img.Dispose();
            img = newBitmap;
          }
        }
      }

      if (img != null)
        return img;

      throw new InvalidOperationException("No image object was created.");
    }
  }
}