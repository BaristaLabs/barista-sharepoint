using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.Library
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false, AllowMultiple = true)]
  public class JSDocAttribute : Attribute
  {
    public JSDocAttribute(string tag, string text)
    {
      this.Tag = tag;
      this.Text = text;
    }

    public JSDocAttribute(string text)
    {
      this.Tag = null;
      this.Text = text;
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
