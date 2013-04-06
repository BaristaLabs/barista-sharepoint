namespace Barista.Services
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.Serialization;

  /// <summary>
  /// Represents the type and location of a Lucene Directory
  /// </summary>
  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class DirectoryDefinition : IEquatable<DirectoryDefinition>, IEqualityComparer<DirectoryDefinition>
  {
    /// <summary>
    /// Gets or sets the type of the index.
    /// </summary>
    [DataMember]
    public string DirectoryTypeName
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the path to the index storage location. (Directory implementation specific)
    /// </summary>
    [DataMember]
    public string IndexStoragePath
    {
      get;
      set;
    }

    public bool Equals(DirectoryDefinition other)
    {
      if (other == null)
        return false;

      return (this.DirectoryTypeName == other.DirectoryTypeName &&
              String.Compare(this.IndexStoragePath, other.IndexStoragePath, StringComparison.InvariantCultureIgnoreCase) == 0);
    }

    public bool Equals(DirectoryDefinition x, DirectoryDefinition y)
    {
      return x.DirectoryTypeName == y.DirectoryTypeName && x.IndexStoragePath == y.IndexStoragePath;
    }

    public int GetHashCode(DirectoryDefinition obj)
    {
      return obj.DirectoryTypeName.GetHashCode();
    }

    public override int GetHashCode()
    {
      return this.DirectoryTypeName.GetHashCode();
    }
  }
}
