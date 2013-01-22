using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.SharePoint.HostService
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class Document
  {
    [DataMember]
    public ICollection<Field> Fields
    {
      get;
      set;
    }
  }
}
