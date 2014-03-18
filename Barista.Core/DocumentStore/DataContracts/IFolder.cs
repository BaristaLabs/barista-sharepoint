namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.Runtime.Serialization;

  public interface IFolder
  {
    [DataMember]
    string Name
    {
      get;
      set;
    }

    [DataMember]
    string FullPath
    {
      get;
      set;
    }

    [DataMember]
    DateTime Modified
    {
      get;
      set;
    }

    [DataMember]
    IUser ModifiedBy
    {
      get;
      set;
    }

    [DataMember]
    DateTime Created
    {
      get;
      set;
    }

    [DataMember]
    IUser CreatedBy
    {
      get;
      set;
    }

    /// <summary>
    /// Gets the number of entities directly contained in the folder.
    /// </summary>
    [DataMember]
    int EntityCount
    {
      get;
    }

    #region Folders
    /// <summary>
    /// Creates the folder with the specified path in the container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    IFolder CreateFolder(string containerTitle, string path);

    /// <summary>
    /// Deletes the folder with the specified path from the container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    void DeleteFolder(string containerTitle, string path);

    /// <summary>
    /// Gets the folder with the specified path that is contained in the container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    IFolder GetFolder(string containerTitle, string path);

    /// <summary>
    /// Lists all folders contained in the specified container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    IList<IFolder> ListAllFolders(string containerTitle, string path);

    /// <summary>
    /// Lists the folders at the specified level in the container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    IList<IFolder> ListFolders(string containerTitle, string path);

    /// <summary>
    /// Renames the specified folder to the new folder name.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="newFolderName">New name of the folder.</param>
    /// <returns></returns>
    IFolder RenameFolder(string containerTitle, string path, string newFolderName);
    #endregion

  }
}