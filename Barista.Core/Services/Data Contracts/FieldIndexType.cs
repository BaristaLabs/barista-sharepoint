namespace Barista.Services
{
  using System.Runtime.Serialization;

  /// <summary>
  /// Controls how the text in the field will be made searchable via the inverted index.
  /// </summary>
  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public enum FieldIndexType
  {
    /// <summary>
    /// Don't make this field's value available for searching.
    /// </summary>
    [EnumMember]
    NotIndexed,
    /// <summary>
    /// Use the analyzer to break the field's value into a stream of seperate tokens and make each token searchable. This option is useful for normal text fields (body, title, abstract, etc.).
    /// </summary>
    [EnumMember]
    Analyzed,
    /// <summary>
    /// A variant of Analyzed that doesn't store norms information in the index.
    /// Norms record index-time boost information in the index but can be memory consuming when you're searching.
    /// </summary>
    [EnumMember]
    AnalyzedNoNorms,
    /// <summary>
    /// Do index the field, but don't analyze the string value. Instead, treat the field's
    /// entire value as a single token and make that token searchable. This option is useful for fields that you'd
    /// like to search on but that shouldn't be broken up, such as URLs, file system paths,
    /// dates, personal names, Social Security numbers and telephone numbers. This option is especially useful for
    /// enabling "exact match" searching.
    /// </summary>
    [EnumMember]
    NotAnalyzed,
    /// <summary>
    /// Just like NotAnalyzed, but also doesn't store norms.
    /// This option is frequently used to save index space and memory usage during searching, because single-token fields
    /// don't need the norms information unless they're boosted.
    /// </summary>
    [EnumMember]
    NotAnalyzedNoNorms,
  }
}
