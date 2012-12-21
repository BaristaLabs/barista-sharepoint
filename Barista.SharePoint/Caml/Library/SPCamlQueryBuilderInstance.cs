namespace Barista.SharePoint.Library
{
  using System;
  using System.Collections.Generic;
  using Jurassic;
  using Jurassic.Library;
  using System.Text;
  using System.Xml;

  [Serializable]
  public class SPCamlQueryBuilderConstructor : ClrFunction
  {
    public SPCamlQueryBuilderConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPCamlQueryBuilder", new SPCamlQueryBuilderInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPCamlQueryBuilderInstance Construct(object objectInstance)
    {
      return new SPCamlQueryBuilderInstance(this.Engine.Object.InstancePrototype);
    }
  }

  /// <summary>
  /// Represents an object that facilitates building CAML Queries.
  /// </summary>
  /// <remarks>
  /// Converted from the EcmaScript version at camljs.codeplex.com
  /// </remarks>
  [Serializable]
  public class SPCamlQueryBuilderInstance : ObjectInstance
  {
    public SPCamlQueryBuilderInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.Tree = new List<CamlHump>();
      this.UnclosedTags = 0;

      this.PopulateFunctions();
    }

    #region Internal Properties
    internal List<CamlHump> Tree
    {
      get;
      set;
    }

    internal int UnclosedTags
    {
      get;
      set;
    }
    #endregion

    #region Functions
    [JSFunction(Name = "Where")]
    public SPCamlQueryBuilder_Where Where()
    {
      Tree.Add(new CamlHump { Element = CamlHumpElementType.Start, Name = "Where" });
      UnclosedTags++;
      return new SPCamlQueryBuilder_Where(this.Engine.Object.InstancePrototype, this);
    }

    [JSFunction(Name = "GroupBy")]
    public SPCamlQueryBuilder_GroupedQuery GroupBy(string groupFieldName, bool collapse)
    {
      StartGroupBy(this, groupFieldName, collapse);
      return new SPCamlQueryBuilder_GroupedQuery(this.Engine.Object.InstancePrototype, this);
    }

    [JSFunction(Name = "OrderBy")]
    public SPCamlQueryBuilder_OrderedQuery OrderBy(string sortFieldName, bool overrideSort, bool useIndexForOrderBy)
    {
      StartOrderBy(this, overrideSort, useIndexForOrderBy);
      Tree.Add(new CamlHump { Element = CamlHumpElementType.FieldRef, Name = sortFieldName });
      return new SPCamlQueryBuilder_OrderedQuery(this.Engine.Object.InstancePrototype, this);
    }

    [JSFunction(Name = "OrderByDesc")]
    public SPCamlQueryBuilder_OrderedQuery OrderByDesc(string sortFieldName, bool overrideSort, bool useIndexForOrderBy)
    {
      StartOrderBy(this, overrideSort, useIndexForOrderBy);
      Tree.Add(new CamlHump { Element = CamlHumpElementType.FieldRef, Name = sortFieldName, IsDescending = true });
      return new SPCamlQueryBuilder_OrderedQuery(this.Engine.Object.InstancePrototype, this);
    }
    #endregion

    #region Static Methods
    public static void Membership(SPCamlQueryBuilderInstance camlBuilder, int startIndex, string type)
    {
      camlBuilder.Tree.Insert(startIndex, new CamlHump
      {
        Element = CamlHumpElementType.Start,
        Name = "Membership",
        Attributes = new Dictionary<string, string> { { "Type", type } },
      });

      camlBuilder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.End,
      });
    }

    public static void UnaryOperator(SPCamlQueryBuilderInstance camlBuilder, int startIndex, string operation)
    {
      camlBuilder.Tree.Insert(startIndex, new CamlHump
      {
        Element = CamlHumpElementType.Start,
        Name = operation,
      });

      camlBuilder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.End,
      });
    }

    public static void BinaryOperator(SPCamlQueryBuilderInstance camlBuilder, int startIndex, string operation, string valueType, string value)
    {
      camlBuilder.Tree.Insert(startIndex, new CamlHump
      {
        Element = CamlHumpElementType.Start,
        Name = operation,
      });

      camlBuilder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.Value,
        ValueType = valueType,
        Value = value
      });
      camlBuilder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.End,
      });
    }

    public static void StartGroupBy(SPCamlQueryBuilderInstance camlBuilder, string groupFieldName, bool collapse)
    {
      if (camlBuilder.UnclosedTags > 0)
      {
        camlBuilder.Tree.Add(new CamlHump
        {
          Element = CamlHumpElementType.End,
          Count = camlBuilder.UnclosedTags
        });
      }

      camlBuilder.UnclosedTags = 0;

      if (collapse)
      {
        camlBuilder.Tree.Add(new CamlHump
        {
          Element = CamlHumpElementType.Start,
          Name = "GroupBy",
          Attributes = new Dictionary<String, String> { { "Collapse", "TRUE" } }
        });
      }
      else
      {
        camlBuilder.Tree.Add(new CamlHump
        {
          Element = CamlHumpElementType.Start,
          Name = "GroupBy"
        });
      }

      camlBuilder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.FieldRef,
        Name = groupFieldName
      });

      camlBuilder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.End
      });
    }

    public static void StartOrderBy(SPCamlQueryBuilderInstance camlBuilder, bool overrideOrderBy, bool useIndexForOrderBy)
    {
      if (camlBuilder.UnclosedTags > 0)
      {
        camlBuilder.Tree.Add(new CamlHump
        {
          Element = CamlHumpElementType.End,
          Count = camlBuilder.UnclosedTags
        });
      }

      camlBuilder.UnclosedTags = 1;

      Dictionary<string, string> attributes = new Dictionary<string, string>();
      if (overrideOrderBy)
        attributes.Add("Override", "TRUE");

      if (useIndexForOrderBy)
        attributes.Add("UseIndexForOrderBy", "TRUE");

      if (attributes.Count > 0)
      {
        camlBuilder.Tree.Add(new CamlHump
        {
          Element = CamlHumpElementType.Start,
          Name = "OrderBy",
          Attributes = attributes,
        });
      }
      else
      {
        camlBuilder.Tree.Add(new CamlHump
        {
          Element = CamlHumpElementType.Start,
          Name = "OrderBy",
        });
      }
    }

    public static string Finalize(SPCamlQueryBuilderInstance camlBuilder)
    {
      var sb = new StringBuilder();
      var writer = XmlWriter.Create(sb, new XmlWriterSettings { 
        Indent = true, 
        OmitXmlDeclaration = true, 
        ConformanceLevel = ConformanceLevel.Fragment 
      });

      foreach (var node in camlBuilder.Tree)
      {
        switch (node.Element)
        {
          case CamlHumpElementType.FieldRef:
            writer.WriteStartElement("FieldRef");
            writer.WriteAttributeString("Name", node.Name);
            if (node.LookupId)
              writer.WriteAttributeString("LookupId", "True");

            if (node.IsDescending)
              writer.WriteAttributeString("Ascending", "False");
            writer.WriteEndElement();
            break;
          case CamlHumpElementType.Start:
            writer.WriteStartElement(node.Name);
            if (node.Attributes != null && node.Attributes.Count > 0)
              foreach (var attribute in node.Attributes)
                writer.WriteAttributeString(attribute.Key, attribute.Value);
            break;
          case CamlHumpElementType.Value:
            writer.WriteStartElement("Value");
            writer.WriteAttributeString("Type", node.ValueType);
            if (node.Value.StartsWith("{") && node.Value.EndsWith("}"))
              writer.WriteRaw("<" + node.Value.Trim('{', '}') + "/>");
            else
              writer.WriteString(node.Value);
            writer.WriteEndElement();
            break;
          case CamlHumpElementType.End:
            if (node.Count.HasValue)
              for (int i = 0; i < node.Count; i++)
                writer.WriteEndElement();
            else
              writer.WriteEndElement();
            break;
        }
      }

      for (int i = 0; i < camlBuilder.UnclosedTags; i++)
        writer.WriteEndElement();

      writer.Close();
      return sb.ToString();
    }
    #endregion
  }
}
