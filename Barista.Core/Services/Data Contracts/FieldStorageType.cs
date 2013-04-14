namespace Barista.Services
{
  using System.Runtime.Serialization;

  /// <summary>
  /// Thie options for stored fields determine whether the field's exact value should be stored away so that you can later retrieve it during searching.
  /// </summary>
  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public enum FieldStorageType
  {
    /// <summary>
    /// Stores the value.
    /// When the value is stored, the original string in its entirety is recorded in the index
    /// and may be retrieved by an IndexReader. This option is useful for fields that you'd like to
    /// use when displaying the search results (such as a URL, title, or database primary key). Try
    /// not to store very large fields, if index size is a concern, as stored fields consume space in the index.
    /// </summary>
    [EnumMember]
    Stored,

    /// <summary>
    /// Doesn't store the value. This option is often used along with Index.Analyzed to index a large text field
    /// that doesn't need to be retrieved in its original form, such as bodies of web pages, or any other type of text document.
    /// </summary>
    [EnumMember]
    NotStored,

  }
}
