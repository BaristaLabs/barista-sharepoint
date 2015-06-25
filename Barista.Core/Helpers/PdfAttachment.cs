namespace Barista
{
  public class PdfAttachment
  {
    public string FileName
    {
      get;
      set;
    }

    public string FileDisplayName
    {
      get;
      set;
    }

    public string Description
    {
      get;
      set;
    }

    public byte[] Data
    {
      get;
      set;
    }
  }
}