namespace Barista
{
  using System.Drawing;
  using System.Drawing.Imaging;
  using Winnovative.WnvHtmlConvert;
  using iTextSharp.text;
  using iTextSharp.text.html.simpleparser;
  using iTextSharp.text.pdf;

  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Image = iTextSharp.text.Image;

  /// <summary>
  /// Represents helper class that converts Html to various formats
  /// </summary>
  public static class HtmlConverterHelper
  {
    /// <summary>
    /// Converts the specified html documents to a single pdf file.
    /// </summary>
    /// <param name="htmlDocuments"></param>
    /// <param name="pdfAttachments"></param>
    /// <param name="pdfConverterLicenseKey"></param>
    /// <returns></returns>
    public static byte[] ConvertHtmlToPdf(IEnumerable<string> htmlDocuments, ICollection<PdfAttachment> pdfAttachments, string pdfConverterLicenseKey)
    {
      //TODO: Add the ability to customize layout and margins.

      PdfConverter pdfConverter = new PdfConverter
        {
          PdfStandardSubset = PdfStandardSubset.Pdf_A_1b
        };

      if (String.IsNullOrEmpty(pdfConverterLicenseKey) == false)
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
        if (pdfAttachments != null && pdfAttachments.Any())
        {
          var reader = new PdfReader(doc.Save());

          using (var outputPdfStream = new MemoryStream())
          {
            var stamper = new PdfStamper(reader, outputPdfStream);
            foreach (var pdfAttachment in pdfAttachments)
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
    public static byte[] ConvertHtmlToPdf_ITextSharp(string html)
    {
      using (var pdfStream = new MemoryStream())
      {
        var pdfDocument = new Document(PageSize.LETTER, 72, 72, 72, 72);

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
    /// <param name="pdfConverterLicenseKey"></param>
    /// <returns></returns>
    public static System.Drawing.Image ConvertHtmlToImage(IEnumerable<string> htmlDocuments, string pdfConverterLicenseKey)
    {
      var imgConverter = new ImgConverter();

      if (String.IsNullOrEmpty(pdfConverterLicenseKey) == false)
        imgConverter.LicenseKey = pdfConverterLicenseKey;

      System.Drawing.Image img = null;
      foreach (var htmlDocument in htmlDocuments)
      {
        if (img == null)
          img = imgConverter.GetImageFromHtmlString(htmlDocument, ImageFormat.Png);
        else
        {
          using (System.Drawing.Image tempImage = imgConverter.GetImageFromHtmlString(htmlDocument, ImageFormat.Png))
          {
            int newImageHeight = img.Height + tempImage.Height;
            int newImageWidth = img.Width > tempImage.Width ? img.Width : tempImage.Width;

            Bitmap newBitmap = new Bitmap(newImageWidth, newImageHeight);
            using (Graphics newImageGraphics = Graphics.FromImage(newBitmap))
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

    /// <summary>
    /// Utility function to crop a png into page-sized portions and embed each png into the pdf.
    /// </summary>
    /// <param name="png"></param>
    /// <returns></returns>
    public static byte[] ConvertPngToPdf(byte[] png)
    {
      using (MemoryStream pdfStream = new MemoryStream())
      {
        Document pdfDocument = new iTextSharp.text.Document(PageSize.LETTER, 36, 36, 36, 36);

        var writer = PdfWriter.GetInstance(pdfDocument, pdfStream);
        writer.CloseStream = false;

        pdfDocument.Open();

        var maxImageHeight = pdfDocument.PageSize.Height + 36;

        using (var source = new System.Drawing.Bitmap(new MemoryStream(png)))
        {
          if (source.Height > maxImageHeight)
          {
            int heightOffset = 0;

            while (heightOffset < source.Height)
            {
              System.Drawing.Rectangle sourceRect;
              sourceRect = heightOffset + (int)(maxImageHeight) > source.Height
                ? new System.Drawing.Rectangle(0, heightOffset, source.Width, source.Height)
                : new System.Drawing.Rectangle(0, heightOffset, source.Width, (int)(maxImageHeight));

              using (var target = new System.Drawing.Bitmap(sourceRect.Width, sourceRect.Height))
              {

                using (var g = Graphics.FromImage(target))
                {
                  g.DrawImage(source, new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
                              sourceRect,
                              GraphicsUnit.Pixel);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                  target.Save(ms, ImageFormat.Png);
                  pdfDocument.Add(new Paragraph());
                  Image img = Image.GetInstance(ms.GetBuffer());
                  img.ScaleToFit(pdfDocument.PageSize.Width - 72, img.Height);
                  pdfDocument.Add(img);
                  if (sourceRect.Height >= maxImageHeight)
                    pdfDocument.NewPage();
                }
              }

              heightOffset += sourceRect.Height;
            }
          }
          else
          {
            pdfDocument.Add(new Paragraph());

            Image img = Image.GetInstance(png);
            img.ScaleToFit(pdfDocument.PageSize.Width - 72, img.Height);
            pdfDocument.Add(img);
          }
        }

        if (pdfDocument.IsOpen())
          pdfDocument.Close();

        pdfStream.Flush();
        pdfStream.Seek(0, SeekOrigin.Begin);
        return pdfStream.GetBuffer();
      }
    }
  }
}