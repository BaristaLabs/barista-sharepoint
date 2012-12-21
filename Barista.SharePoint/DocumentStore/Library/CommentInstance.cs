namespace Barista.SharePoint.DocumentStore.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class CommentInstance : ObjectInstance
  {
    private readonly Comment m_comment;

    public CommentInstance(ScriptEngine engine, Comment comment)
      : base(engine)
    {
      if (comment == null)
        throw new ArgumentNullException("comment");

      m_comment = comment;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "id")]
    public int Id
    {
      get { return m_comment.Id; }
      set { m_comment.Id = value; }
    }

    [JSProperty(Name = "commentText")]
    public string CommentText
    {
      get { return m_comment.CommentText; }
      set { m_comment.CommentText = value; }
    }
  }
}
