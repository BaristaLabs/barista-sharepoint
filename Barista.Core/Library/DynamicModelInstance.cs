namespace Barista.Library
{
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Data.Common;
  using System.Linq;
  using System.Text;

  public class DynamicModelInstance : ObjectInstance
  {
    private string m_connectionString;
    private DbProviderFactory m_factory;

    public DynamicModelInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    #region Private Methods
    /// <summary>
    /// Creates a DBCommand that you can use for loving your database.
    /// </summary>
    private DbCommand CreateCommand(string sql, DbConnection conn, params object[] args)
    {
      var result = m_factory.CreateCommand();
      result.Connection = conn;
      result.CommandText = sql;
      if (args.Length > 0)
        result.AddParams(args);
      return result;
    }
    #endregion
  }
}
