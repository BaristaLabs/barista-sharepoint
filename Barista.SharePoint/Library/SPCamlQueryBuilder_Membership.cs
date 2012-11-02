namespace Barista.SharePoint.Library
{
  using System;
  using System.Collections.Generic;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using System.Text;
  using System.Xml;

  public class SPCamlQueryBuilder_Membership : ObjectInstance
  {
    public SPCamlQueryBuilder_Membership(ObjectInstance prototype, SPCamlQueryBuilderInstance builder, int startIndex)
      : base(prototype)
    {
      this.Builder = builder;
      this.StartIndex = startIndex;
      this.SPWeb = new SPCamlQueryBuilder_SPWeb(this.Engine.Object.InstancePrototype, builder, startIndex);
      this.PopulateFields();
      this.PopulateFunctions();
    }

    internal SPCamlQueryBuilderInstance Builder
    {
      get;
      private set;
    }

    internal int StartIndex
    {
      get;
      private set;
    }

    [JSProperty(Name = "SPWeb")]
    public SPCamlQueryBuilder_SPWeb SPWeb
    {
      get;
      private set;
    }

    [JSFunction(Name = "SPGroup")]
    public SPCamlQueryBuilder_Token SPGroup()
    {
      SPCamlQueryBuilderInstance.Membership(this.Builder, this.StartIndex, "SPGroup");
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    [JSFunction(Name = "CurrentUserGroups")]
    public SPCamlQueryBuilder_Token CurrentUserGroups()
    {
      SPCamlQueryBuilderInstance.Membership(this.Builder, this.StartIndex, "CurrentUserGroups");
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }
  }

  public class SPCamlQueryBuilder_SPWeb : ObjectInstance
  {
    public SPCamlQueryBuilder_SPWeb(ObjectInstance prototype, SPCamlQueryBuilderInstance builder, int startIndex)
      : base(prototype)
    {
      this.Builder = builder;
      this.StartIndex = startIndex;
      this.PopulateFunctions();
    }

    internal SPCamlQueryBuilderInstance Builder
    {
      get;
      private set;
    }

    internal int StartIndex
    {
      get;
      private set;
    }

    [JSFunction(Name = "AllUsers")]
    public SPCamlQueryBuilder_Token AllUsers()
    {
      SPCamlQueryBuilderInstance.Membership(this.Builder, this.StartIndex, "SPWeb.AllUsers");
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    [JSFunction(Name = "Users")]
    public SPCamlQueryBuilder_Token Users()
    {
      SPCamlQueryBuilderInstance.Membership(this.Builder, this.StartIndex, "SPWeb.Users");
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    [JSFunction(Name = "Groups")]
    public SPCamlQueryBuilder_Token Groups()
    {
      SPCamlQueryBuilderInstance.Membership(this.Builder, this.StartIndex, "SPWeb.Groups");
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }
  }
}
