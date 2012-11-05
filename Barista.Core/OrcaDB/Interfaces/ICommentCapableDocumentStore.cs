namespace Barista.OrcaDB
{
  using System;
  using System.Collections.Generic;

  public interface ICommentCapableDocumentStore
  {
    #region Comments
    /// <summary>
    /// Adds a comment to the specified attachment.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="comment">The comment.</param>
    /// <returns></returns>
    Comment AddAttachmentComment(string containerTitle, Guid entityId, string fileName, string comment);

    /// <summary>
    /// Adds a comment to the specified entity.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="comment">The comment.</param>
    /// <returns></returns>
    Comment AddEntityComment(string containerTitle, Guid entityId, string comment);

    /// <summary>
    /// Adds a comment to the specified entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="comment">The comment.</param>
    /// <returns></returns>
    Comment AddEntityPartComment(string containerTitle, Guid entityId, string partName, string comment);

    /// <summary>
    /// Lists the comments associated with the specified attachment.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    IList<Comment> ListAttachmentComments(string containerTitle, Guid entityId, string fileName);

    /// <summary>
    /// Lists the comments associated with the specified entity.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    IList<Comment> ListEntityComments(string containerTitle, Guid entityId);

    /// <summary>
    /// Lists the comments associated with the specified entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <returns></returns>
    IList<Comment> ListEntityPartComments(string containerTitle, Guid entityId, string partName);
    #endregion
  }
}
