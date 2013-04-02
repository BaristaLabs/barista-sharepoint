namespace Barista.Services
{
  using System;
  using System.Runtime.Serialization;

  /// <summary>
  /// Represents the type and location of an index.
  /// </summary>
  [DataContract(Namespace=Barista.Constants.ServiceNamespace)]
  public class IndexDefinition : IEquatable<IndexDefinition>
  {
    [DataMember]
    public DirectoryType DirectoryType
    {
      get;
      set;
    }

    [DataMember]
    public string DirectoryUri
    {
      get;
      set;
    }

    public bool Equals(IndexDefinition other)
    {
      if (other == null)
        return false;

      return (this.DirectoryType == other.DirectoryType &&
              String.Compare(this.DirectoryUri, other.DirectoryUri, StringComparison.InvariantCultureIgnoreCase) == 0);
    }
  }
}
