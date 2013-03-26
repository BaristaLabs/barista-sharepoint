namespace Barista.SharePoint
{
  using Jurassic;
  using System.IO;

  public class BaristaScriptSource : ScriptSource
  {
    private readonly string m_code;
    private readonly string m_codePath;

    public BaristaScriptSource(string code, string codePath)
    {
      m_code = code;
      m_codePath = codePath;
    }

    public override string Path
    {
      get { return m_codePath; }
    }

    public override TextReader GetReader()
    {
      return new StringReader(m_code);
    }
  }
}
