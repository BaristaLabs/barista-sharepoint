﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.DocumentStore
{
  public sealed class EntityFilterCriteria
  {
    public EntityFilterCriteria()
    {
      this.NamespaceMatchType = DocumentStore.NamespaceMatchType.Equals;
      this.Path = null;
      this.Skip = null;
      this.Top = null;
    }

    public string Path
    {
      get;
      set;
    }

    public string Namespace
    {
      get;
      set;
    }

    public NamespaceMatchType NamespaceMatchType
    {
      get;
      set;
    }

    public uint? Skip
    {
      get;
      set;
    }

    public uint? Top
    {
      get;
      set;
    }

    public IDictionary<string, string> FieldValues
    {
      get;
      set;
    }
  }

  public enum NamespaceMatchType
  {
    Equals,
    StartsWith,
    EndsWith,
    Contains,
  }
}