namespace Barista.SharePoint.HostService
{
  using System;
  using System.Runtime.Serialization;

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
