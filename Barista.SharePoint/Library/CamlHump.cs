using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.SharePoint.Library
{
  public enum CamlHumpElementType
  {
    Start,
    End,
    Count,
    Value,
    FieldRef,
  }

  public class CamlHump
  {
    public CamlHump()
    {
      Attributes = new Dictionary<string, string>();
    }

    public int? Count
    {
      get;
      set;
    }

    public CamlHumpElementType Element
    {
      get;
      set;
    }

    public string Name
    {
      get;
      set;
    }

    public string ValueType
    {
      get;
      set;
    }

    public string Value
    {
      get;
      set;
    }

    public bool IsDescending
    {
      get;
      set;
    }

    public bool LookupId
    {
      get;
      set;
    }

    public IDictionary<string, string> Attributes
    {
      get;
      set;
    }

    public override string ToString()
    {
      return this.Element.ToString() + " " + this.Name;
    }
  }
}
