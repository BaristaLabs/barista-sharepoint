namespace Barista.Justache
{
  using System;

  public class LiteralText : Part
  {
    private readonly string m_text;

    public LiteralText(string text)
    {
      if (text == null)
      {
        throw new ArgumentNullException("text");
      }

      m_text = text;
    }

    public string Text { get { return m_text; } }

    public override void Render(RenderContext context)
    {
      context.Write(m_text);
    }

    public override string Source()
    {
      return m_text;
    }

    public override string ToString()
    {
      return string.Format("LiteralText(\"{0}\")", m_text);
    }
  }
}