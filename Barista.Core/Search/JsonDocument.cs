namespace Barista.Search
{
  using System;
  using Barista.Newtonsoft.Json.Linq;

  /// <summary>
  /// A document representation:
  /// * Data / Projection
  /// * Etag
  /// * Metadata
  /// </summary>
  public class JsonDocument : IJsonDocumentMetadata
  {
    private JObject m_dataAsJson;
    private JObject m_metadata;

    /// <summary>
    /// Gets or sets the document data as json.
    /// </summary>
    /// <value>The data as json.</value>
    public JObject DataAsJson
    {
      get { return m_dataAsJson ?? (m_dataAsJson = new JObject()); }
      set { m_dataAsJson = value; }
    }

    /// <summary>
    /// Gets or sets the metadata for the document
    /// </summary>
    /// <value>The metadata.</value>
    public JObject Metadata
    {
      get { return m_metadata ?? (m_metadata = new JObject(StringComparer.InvariantCultureIgnoreCase)); }
      set { m_metadata = value; }
    }

    /// <summary>
    /// Gets or sets the document id
    /// </summary>
    /// <value>The unique-per-index id of the document.</value>
    public string DocumentId
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this document is non authoritative (modified by uncommitted transaction).
    /// </summary>
    public bool? NonAuthoritativeInformation
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the etag.
    /// </summary>
    /// <value>The etag.</value>
    public Guid? Etag
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the last modified date for the document
    /// </summary>
    /// <value>The last modified.</value>
    public DateTime? LastModified
    {
      get;
      set;
    }

    /// <summary>
    /// The ranking of this result in the current query
    /// </summary>
    public float? TempIndexScore
    {
      get;
      set;
    }

    /// <summary>
    /// How much space this document takes on disk
    /// Only relevant during indexing phases, and not available on the client
    /// </summary>
    public int SerializedSizeOnDisk;

    /// <summary>
    /// Whatever this document can be skipped from delete
    /// Only relevant during indexing phases, and not available on the client
    /// </summary>
    public bool SkipDeleteFromIndex;

    /// <summary>
    /// Translate the json document to a <see cref = "JObject" />
    /// </summary>
    /// <returns></returns>
    public JObject ToJson()
    {
      var docSnapshot = (JObject) DataAsJson.CloneToken();
      var metadataSnapshot = (JObject) Metadata.CloneToken();

      if (LastModified != null)
        metadataSnapshot[Constants.LastModified] = LastModified.Value;
      if (Etag != null)
        metadataSnapshot["@etag"] = Etag.Value.ToString();
      if (NonAuthoritativeInformation != null)
        metadataSnapshot["Non-Authoritative-Information"] = NonAuthoritativeInformation.Value;
      //if (metadata.ContainsKey("@id") == false)
      //	metadata["@id"] = Key;
      docSnapshot["@metadata"] = metadataSnapshot;

      return docSnapshot;
    }

    public override string ToString()
    {
      return DocumentId;
    }
  }
}