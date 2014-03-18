namespace Barista.DocumentStore
{
  using System.Collections.Generic;

  public interface IDSMetadata
  {
    /// <summary>
    /// Gets the metadata value on the DSObject.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    string GetMetadataValue(string key);

    /// <summary>
    /// Sets the  metadata.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    bool SetMetadataValue(string key, string value);

    /// <summary>
    /// Deletes the specified metadata value.
    /// </summary>
    /// <param name="key"></param>
    bool DeleteMetadataValue(string key);

    /// <summary>
    /// Lists the entity metadata.
    /// </summary>
    /// <returns></returns>
    IDictionary<string, string> ListMetadataValues();
  }
}
