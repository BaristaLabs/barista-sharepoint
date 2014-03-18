namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  /// <summary>
  /// Represents an object contained in a document store.
  /// </summary>
  public interface IDSObject
  {
    /// <summary>
    /// Gets a value that indicates when the DSObject was created.
    /// </summary>
    [DataMember]
    DateTime Created
    {
      get;
    }

    /// <summary>
    /// Gets a value that indicates the user that created the DSObject
    /// </summary>
    [DataMember]
    IUser CreatedBy
    {
      get;
    }

    /// <summary>
    /// Gets a value that indicates when the DSObject was last modified.
    /// </summary>
    [DataMember]
    DateTime Modified
    {
      get;
    }

    /// <summary>
    /// Gets a value that indicates the user that last modified the DSObject
    /// </summary>
    [DataMember]
    IUser ModifiedBy
    {
      get;
    }
  }
}
