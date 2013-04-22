using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.Search.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class ODataQueryConstructor : ClrFunction
  {
    public ODataQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ODataQuery", new ODataQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ODataQueryInstance Construct()
    {
      return new ODataQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ODataQueryInstance : ObjectInstance
  {
    private readonly ODataQuery m_oDataQuery;

    public ODataQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ODataQueryInstance(ObjectInstance prototype, ODataQuery oDataQuery)
      : this(prototype)
    {
      if (oDataQuery == null)
        throw new ArgumentNullException("oDataQuery");

      m_oDataQuery = oDataQuery;
    }

    public ODataQuery ODataQuery
    {
      get { return m_oDataQuery; }
    }
  }
}
