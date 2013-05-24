namespace Barista.Library
{
  using Barista.Jurassic;
  using System.Reflection;

  public class BaristaWebInstance : WebInstanceBase
  {
    public BaristaWebInstance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Public);
    }
  }
}
