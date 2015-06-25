namespace Barista.Framework
{
  using System.ServiceModel.Channels;
  using System.Xml;

  public class RawBodyWriter : BodyWriter
  {
    private readonly byte[] m_content;

    public RawBodyWriter(byte[] content)
      : base(true)
    {
      this.m_content = content;
    }

    protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
    {
      writer.WriteStartElement("Binary");
      writer.WriteBase64(m_content, 0, m_content.Length);
      writer.WriteEndElement();
    }
  }
}
