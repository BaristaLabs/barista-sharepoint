namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Taxonomy;

  [Serializable]
  public class TermLabelInstance : ObjectInstance
  {
    private Label m_termLabel;

    public TermLabelInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TermLabelInstance(ObjectInstance prototype, Label termLabel)
      : this(prototype)
    {
      this.m_termLabel = termLabel;
    }

    internal Label TermLabel
    {
      get { return m_termLabel; }
    }

    #region Properties
    [JSProperty(Name = "isDefaultForLanguage")]
    public bool IsDefaultForLanguage
    {
      get { return m_termLabel.IsDefaultForLanguage; }
    }

    [JSProperty(Name = "language")]
    public int Language
    {
      get { return m_termLabel.Language; }
    }

    [JSProperty(Name = "value")]
    public string Value
    {
      get { return m_termLabel.Value; }
      set { m_termLabel.Value = value; }
    }
    #endregion

    #region Functions
    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_termLabel.Delete();
    }

    [JSFunction(Name = "getTerm")]
    public TermInstance GetTerm()
    {
      return new TermInstance(this.Engine.Object.InstancePrototype, m_termLabel.Term);
    }

    [JSFunction(Name = "setAsDefaultForLanguage")]
    public void SetAsDefaultForLanguage()
    {
      m_termLabel.SetAsDefaultForLanguage();
    }
    #endregion
  }
}
