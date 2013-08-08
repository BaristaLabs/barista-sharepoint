namespace Barista.DocumentStore.FileSystem.Data
{
  public class Constants
  {
    public const string EntityPackageContentType = "application/barista.documentstore.entity.1";

    public const string EntityPartContentType = "application/json";

    public const string EntityPartV1Namespace = @"/entity-parts/";
    public const string AttachmentV1Namespace = @"/attachments/";

    //Relationship Names
    public const string EntityPartRelationship = "entity-part";
    public const string EntityPartMetadataRelationship = "entity-part-metadata";
    public const string AttachmentRelationship = "attachment";
    public const string AttachmentMetdataRelationship = "attachment-metdata";
  }
}
