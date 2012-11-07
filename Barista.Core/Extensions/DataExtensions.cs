namespace Barista.Extensions
{
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
        if (item.GetType() == typeof(Guid))
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

        if (item.GetType() == typeof(string))
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
        e.SetPropertyValue(rdr.GetName(i), DBNull.Value.Equals(rdr[i]) ? Null.Value : rdr[i], false);
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
            value = JurassicHelper.ToDateInstance(engine, ((DateTime)value).ToLocalTime());

          result.SetPropertyValue(item.Name, value, false);
        }
      }
      return result;
    }
  }
}
