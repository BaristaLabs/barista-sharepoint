namespace Barista.Justache
{
  /// <summary>
  /// This derives from Template so that it can
  /// be returned from a TemplateLocator.
  /// Should we make it implement an interface instead?
  /// </summary>
  public class TemplateDefinition : Template
  {
    public TemplateDefinition(string name)
      : base(name)
    {
    }

    public override string ToString()
    {
      return string.Format("TemplateDefinition(\"{0}\")", Name);
    }
  }
}