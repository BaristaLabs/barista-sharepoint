namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  public interface IContainer : IDSObject, IDSMetadata, IDSPermissions
  {
    /// <summary>
    /// Gets the id of the container.
    /// </summary>
    [DataMember]
    Guid Id
    {
      get;
    }

    /// <summary>
    /// Gets or sets the title of the container.
    /// </summary>
    [DataMember]
    string Title
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the container description.
    /// </summary>
    [DataMember]
    string Description
    {
      get;
      set;
    }

    /// <summary>
    /// Gets the root folder of the container.
    /// </summary>
    [DataMember]
    IFolder RootFolder
    {
      get;
    }

    /// <summary>
    /// Gets the url of the container.
    /// </summary>
    [DataMember]
    string Url
    {
      get;
    }

    #region Methods

    #endregion
  }
}