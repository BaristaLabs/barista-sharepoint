namespace Barista.SocketBase.Provider
{
  using System;

  /// <summary>
  /// Export Factory
  /// </summary>
  [Serializable]
  public class ExportFactory
  {
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    public string TypeName { get; set; }

    private Type m_loadedType;

    [NonSerialized]
    private object m_instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportFactory"/> class.
    /// </summary>
    public ExportFactory()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportFactory"/> class.
    /// </summary>
    /// <param name="instance">The instance.</param>
    public ExportFactory(object instance)
    {
      m_instance = instance;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportFactory"/> class.
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    public ExportFactory(string typeName)
    {
      TypeName = typeName;
    }

    /// <summary>
    /// Ensures the instance's existance.
    /// </summary>
    public void EnsureInstance()
    {
      if (m_instance != null)
        return;

      m_instance = CreateInstance();
    }

    private object CreateInstance()
    {
      if (m_loadedType == null)
      {
        m_loadedType = System.Type.GetType(TypeName, true);
      }

      return Activator.CreateInstance(m_loadedType);
    }

    /// <summary>
    /// Creates the export type instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T CreateExport<T>()
    {
      if (m_instance != null)
        return (T)m_instance;

      return (T)CreateInstance();
    }
  }
}
