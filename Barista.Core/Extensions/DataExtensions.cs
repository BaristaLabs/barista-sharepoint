namespace Barista.Extensions
{
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Data;
  using System.Data.Common;
  using System.Linq;

  public static class ObjectExtensions
  {
    /// <summary>
    /// Extension method for adding in a bunch of parameters
    /// </summary>
    public static void AddParams(this DbCommand cmd, params object[] args)
    {
      foreach (var item in args)
      {
        AddParam(cmd, item);
      }
    }

    /// <summary>
    /// Extension for adding single parameter
    /// </summary>
    public static void AddParam(this DbCommand cmd, object item)
    {
      var p = cmd.CreateParameter();
      p.ParameterName = string.Format("@{0}", cmd.Parameters.Count);
      if (item == null)
      {
        p.Value = DBNull.Value;
      }
      else
      {
        if (item is Guid)
        {
          p.Value = item.ToString();
          p.DbType = DbType.String;
          p.Size = 4000;
        }
        else if (item.GetType() == typeof(ObjectInstance))
        {
          var oi = item as ObjectInstance;
          p.Value = oi.ToDictionary().FirstOrDefault();
        }
        else
        {
          p.Value = item;
        }

        if (item is string)
          p.Size = ((string)item).Length > 4000 ? -1 : 4000;
      }
      cmd.Parameters.Add(p);
    }

    public static IDictionary<string, object> ToDictionary(this ObjectInstance oi)
    {
      Dictionary<string, object> result = new Dictionary<string, object>();
      if (oi == null)
        return result;

      foreach (var property in oi.Properties)
      {
        result.Add(property.Name, property.Value);
      }

      return result;
    }

    /// <summary>
    /// Turns an IDataReader to an ArrayInstance
    /// </summary>
    public static ArrayInstance ToArrayInstance(this IDataReader rdr, ScriptEngine engine)
    {
      var result = engine.Array.Construct();
      while (rdr.Read())
      {
        ArrayInstance.Push(result, rdr.RecordToObjectInstance(engine));
      }
      return result;
    }

    public static ObjectInstance RecordToObjectInstance(this IDataReader rdr, ScriptEngine engine)
    {
      ObjectInstance e = engine.Object.Construct();
      for (int i = 0; i < rdr.FieldCount; i++)
      {
        object propertyValue;
        var value = rdr[i];

        if (DBNull.Value.Equals(value))
          propertyValue = Null.Value;
        else if (value is DateTime)
          propertyValue = JurassicHelper.ToDateInstance(engine, (DateTime)value);
        else if (value is Byte)
          propertyValue = Convert.ToInt32((Byte)value);
        else if (value is Int16)
          propertyValue = Convert.ToInt32((Int16)value);
        else if (value is Int64)
          propertyValue = Convert.ToDouble((Int64)value);
        else if (value is decimal)
          propertyValue = Convert.ToDouble((decimal)value);
        else if (value is Guid)
          propertyValue = ((Guid)value).ToString();
        else if (value is Byte[])
          propertyValue = new Base64EncodedByteArrayInstance(engine.Object.InstancePrototype, (Byte[])value);
        else if (value.GetType().FullName == "Microsoft.SqlServer.Types.SqlHierarchyId")
          propertyValue = value.ToString();
        else
          propertyValue = value;

        e.SetPropertyValue(rdr.GetName(i), propertyValue, false);
      }
      return e;
    }

    /// <summary>
    /// Turns the object into an ObjectInstance
    /// </summary>
    public static ObjectInstance ToObjectInstance(this object o, ScriptEngine engine)
    {
      var result = engine.Object.Construct();

      if (o.GetType() == typeof(ObjectInstance))
        return o as ObjectInstance; //shouldn't have to... but just in case

      if (o.GetType() == typeof(NameValueCollection) || o.GetType().IsSubclassOf(typeof(NameValueCollection)))
      {
        var nv = (NameValueCollection)o;
        nv.Cast<string>().Select(key => new KeyValuePair<string, object>(key, nv[key])).ToList().ForEach(i => result.SetPropertyValue(i.Key, i.Value, false));
      }
      else
      {
        var props = o.GetType().GetProperties();
        foreach (var item in props)
        {
          //TODO: Be more smart about the types? (should only be primitives, but could be something else)
          var value = item.GetValue(o, null);

          if (value is DateTime)
            value = JurassicHelper.ToDateInstance(engine, (DateTime)value);

          result.SetPropertyValue(item.Name, value, false);
        }
      }
      return result;
    }
  }
}
