namespace Barista
{
  using System;

  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false, AllowMultiple = true)]
  public class JSDocAttribute : Attribute
  {
    public JSDocAttribute(string tag, string text)
    {
      Tag = tag;
      Text = text;
    }

    public JSDocAttribute(string text)
    {
      Tag = null;
      Text = text;
    }

    public string Tag
    {
      get;
      set;
    }

    public string Text
    {
      get;
      set;
    }
  }
}
