namespace OFS.OrcaDB.Core
{
  using System;

  /// <summary>
  /// Represents a Document Store that implements all possible capabilities.
  /// </summary>
  public interface IFullyCapableDocumentStore :
    IDocumentStore,
    IAttachmentCapableDocumentStore, 
    ICommentCapableDocumentStore,
    IFolderCapableDocumentStore, 
    IEntityPartCapableDocumentStore, 
    IMetadataCapableDocumentStore,
    IPermissionsCapableDocumentStore,
    IVersionHistoryCapableDocumentStore,
    ILockCapableDocumentStore
  {
  }
}
