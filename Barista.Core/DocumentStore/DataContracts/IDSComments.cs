namespace Barista.DocumentStore
{
  using System.Collections.Generic;

  public interface IDSComments
  {
    /// <summary>
    /// Adds a comment to the specified DSObject.
    /// </summary>
    /// <param name="comment">The comment.</param>
    /// <returns></returns>
    IComment AddComment(string comment);

    /// <summary>
    /// Lists the comments associated with the DSObject.
    /// </summary>
    /// <returns></returns>
    IList<IComment> ListComments();
  }
}
