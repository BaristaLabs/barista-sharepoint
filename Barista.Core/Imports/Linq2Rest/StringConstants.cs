// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringConstants.cs" company="Reimers.dk">
//   Copyright © Reimers.dk 2012
//   This source is subject to the Microsoft Public License (Ms-PL).
//   Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
//   All other rights reserved.
// </copyright>
// <summary>
//   Defines the StringConstants type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Barista.Imports.Linq2Rest
{
  public static class StringConstants
  {
    public const string OrderByParameter = "$orderby";
    public const string SelectParameter = "$select";
    public const string FilterParameter = "$filter";
    public const string SkipParameter = "$skip";
    public const string TopParameter = "$top";
    public const string ExpandParameter = "$expand";
    public const string JsonMimeType = "application/json";
    public const string XmlMimeType = "application/xml";
  }
}