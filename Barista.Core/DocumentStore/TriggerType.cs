namespace Barista.DocumentStore
{
  /// <summary>
  /// Denotes the type of action that caused a trigger to fire.
  /// </summary>
  public enum TriggerType
  {
    //Entity
    EntityCreated,
    EntityUpdated,
    EntityMoved,
    EntityCommentAdded,
    EntityDeleting,

    //EntityPart
    EntityPartCreated,
    EntityPartUpdated,
    EntityPartDeleted,

    //Attachments
    AttachmentUploaded,
    AttachmentDeleted,
  }
}
