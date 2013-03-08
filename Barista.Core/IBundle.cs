namespace Barista
{
  using Barista.Jurassic;

  /// <summary>
  /// Represents a Bundle that, when installed, provides additional functionality.
  /// </summary>
  public interface IBundle
  {
    /// <summary>
    /// The name of the bundle. The name must be unique across all registered bundles.
    /// </summary>
    string BundleName
    {
      get;
    }

    /// <summary>
    /// The description of the bundle (optional);
    /// </summary>
    string BundleDescription
    {
      get;
    }

    /// <summary>
    /// Installs the bundle within the specified script engine.
    /// </summary>
    /// <param name="engine"></param>
    /// <returns></returns>
    object InstallBundle(ScriptEngine engine);
  }
}
