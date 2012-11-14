﻿namespace Barista.SharePoint.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

  public class PrincipalInstance : ObjectInstance
  {
    Principal m_principal;

    public PrincipalInstance(ScriptEngine engine, Principal principal)
      : base(engine)
    {
      if (principal == null)
        throw new ArgumentNullException("principal");

      m_principal = principal;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "type")]
    public string Type
    {
      get
      {
        if (m_principal is User)
          return "User";
        else
          return "Group";
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_principal.Name; }
      set { m_principal.Name = value; }
    }

    [JSProperty(Name = "loginName")]
    public string LoginName
    {
      get
      {
        if (m_principal is User)
          return (m_principal as User).LoginName;
        else
          return (m_principal as Group).LoginName;
      }
      set
      {
        if (m_principal is User)
          (m_principal as User).LoginName = value;
        else
          (m_principal as Group).LoginName = value;
      }
    }

    [JSProperty(Name = "email")]
    public string Email
    {
      get
      {
        if (m_principal is User)
          return (m_principal as User).Email;
        else
          return (m_principal as Group).DistributionGroupEmail;
      }
      set
      {
        if (m_principal is User)
          (m_principal as User).Email = value;
        else
          (m_principal as Group).DistributionGroupEmail = value;
      }
    }
    
  }
}