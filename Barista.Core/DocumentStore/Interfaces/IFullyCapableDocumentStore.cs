namespace Barista.DocumentStore
{
  /// <summary>
  /// Represents a Document Store that implements all possible capabilities.
  /// </summary>
  public interface IFullyCapableDocumentStore :
    IDocumentStore,
    IAttachmentCapableDocumentStore, 
    ICommentCapableDocumentStore,
    IFolderCapableDocumentStore, 
    IEntityPartCapableDocumentStore, 
    IEntityPartFolderCapableDocumentStore,
    IMetadataCapableDocumentStore,
    IPermissionsCapableDocumentStore,
    IVersionHistoryCapableDocumentStore,
    ILockCapableDocumentStore
  {
  }
}
