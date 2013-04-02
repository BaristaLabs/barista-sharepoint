namespace Barista.DirectoryServices
{
  using Interop.ActiveDs;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceNamespace)]
  [DirectorySchema("group", typeof(IADsGroup))]
  public class ADGroup : DirectoryEntity
  {
    [DataMember]
    public string Name
    {
      get;
      set;
    }

    [DataMember]
    [DirectoryAttribute("member")]
    public string[] Members
    {
      get;
      set;
    }
  }
}
