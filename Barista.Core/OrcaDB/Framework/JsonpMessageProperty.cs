namespace Barista.OrcaDB.Framework
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.ServiceModel.Channels;
  using System.Text;

  public class JsonpMessageProperty : IMessageProperty
  {
    public const string Name = "Barista.OrcaDB.Framework.JsonpMessageProperty";

    public IMessageProperty CreateCopy()
    {
      return new JsonpMessageProperty(this);
    }

    public JsonpMessageProperty()
    {
    }

    internal JsonpMessageProperty(JsonpMessageProperty other)
    {
      this.MethodName = other.MethodName;
    }

    public string MethodName { get; set; }
  }
}
