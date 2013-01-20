namespace Barista.Linq2Rest.Provider
{
  /// <summary>
  /// Defines the public interface for a factory of <see cref="ISerializer{T}"/>.
  /// </summary>
  public interface ISerializerFactory
  {
    /// <summary>
    /// Creates an instance of an <see cref="ISerializer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The item type for the serializer.</typeparam>
    /// <returns>An instance of an <see cref="ISerializer{T}"/>.</returns>
    ISerializer<T> Create<T>();
  }

  internal abstract class SerializerFactoryContracts : ISerializerFactory
  {
    public ISerializer<T> Create<T>()
    {
      throw new System.NotImplementedException();
    }
  }
}