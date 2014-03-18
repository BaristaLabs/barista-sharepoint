namespace Barista.DocumentStore.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class CommentInstance : ObjectInstance, IComment
  {
    private readonly IComment m_comment;

    public CommentInstance(ScriptEngine engine, IComment comment)
      : base(engine)
    {
      if (comment == null)
        throw new ArgumentNullException("comment");

      m_comment = comment;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IComment Comment
    {
      get { return m_comment; }
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

    [JSProperty(Name = "created")]
    public DateInstance Created
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_comment.Created); }
      set { m_comment.Created = DateTime.Parse(value.ToIsoString()); }
    }

    DateTime IComment.Created
    {
      get { return m_comment.Created; }
      set { m_comment.Created = value; }
    }

    [JSProperty(Name = "createdBy")]
    public object CreatedBy
    {
      get
      {
        if (m_comment.CreatedBy == null)
          return Null.Value;

        return m_comment.CreatedBy.LoginName;
      }
    }

    IUser IComment.CreatedBy
    {
      get { return m_comment.CreatedBy; }
      set { m_comment.CreatedBy = value; }
    }
  }
}
