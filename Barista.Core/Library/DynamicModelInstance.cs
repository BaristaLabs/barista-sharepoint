namespace Barista.Library
{
  using System.Globalization;
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Data.Common;
  using System.Linq;
  using System.Text;

  [Serializable]
  public class DynamicModelConstructor : ClrFunction
  {
    public DynamicModelConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "DynamicModel", new DynamicModelInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public DynamicModelInstance Construct(string connectionStringName)
    {
      if (String.IsNullOrEmpty(connectionStringName))
        throw new JavaScriptException(this.Engine, "Error", "A connection string name must be specified.");

      if (ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>().Any( cs => cs.Name == connectionStringName) == false)
        throw new JavaScriptException(this.Engine, "Error", "A connection string setting with the specified name was not found in web.config");

      return new DynamicModelInstance(this.Engine.Object.InstancePrototype, connectionStringName);
    }

    [JSConstructorFunction]
    public DynamicModelInstance Construct(string connectionString, string providerName)
    {
      if (String.IsNullOrEmpty(connectionString))
        throw new JavaScriptException(this.Engine, "Error", "A connection string must be specified.");

      if (String.IsNullOrEmpty(providerName))
        throw new JavaScriptException(this.Engine, "Error", "A provider name must be specified.");

      return new DynamicModelInstance(this.Engine.Object.InstancePrototype, connectionString, providerName);
    }
  }

  [Serializable]
  public class DynamicModelInstance : ObjectInstance
  {
    private readonly string m_connectionString;
    private readonly string m_providerName;
    private readonly DbProviderFactory m_factory;

    private ArrayInstance m_schema;
    private string m_databaseName;

    public DynamicModelInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public DynamicModelInstance(ObjectInstance prototype, string connectionStringName)
      : this(prototype)
    {
      if (ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName != null)
        m_providerName = ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName;

      m_factory = DbProviderFactories.GetFactory(m_providerName);
      m_connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
    }

    public DynamicModelInstance(ObjectInstance prototype, string connectionString, string providerName)
      : this(prototype)
    {
      m_providerName = providerName;
      m_connectionString = connectionString;

      m_factory = DbProviderFactories.GetFactory(providerName);
    }

    #region Properties
    [JSProperty(Name = "connectionString")]
    public string ConnectionString
    {
      get { return m_connectionString; }
    }

    [JSProperty(Name = "databaseName")]
    public string DatabaseName
    {
      get
      { 
        return m_databaseName ?? (m_databaseName = (string) Scalar("SELECT DB_NAME() AS DataBaseName"));
      }
      set
      {
        m_databaseName = value;
      }
    }

    [JSProperty(Name = "schema")]
    public ArrayInstance Schema
    {
      get
      {
        return m_schema ?? (m_schema = String.IsNullOrEmpty(this.TableSchema)
                                         ? Query("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @0",
                                                 TableName)
                                         : Query(
                                           "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @0 AND TABLE_NAME = @1",
                                           TableSchema, TableName));
      }
    }

    [JSProperty(Name = "tableSchema")]
    public string TableSchema
    {
      get;
      set;
    }

    [JSProperty(Name = "tableName")]
    public string TableName
    {
      get;
      set;
    }

     [JSProperty(Name = "primaryKeyField")]
    public string PrimaryKeyField
    {
      get;
      set;
    }
    #endregion

    #region Functions
     /// <summary>
     /// Returns all records complying with the passed-in WHERE clause and arguments, 
     /// ordered as specified, limited (TOP) by limit.
     /// </summary>
     [JSFunction(Name = "all")]
     public ArrayInstance All(object allParams)
     {
       var parameters = new AllParams(this.Engine.Object.InstancePrototype);

       if (allParams != null && allParams != Undefined.Value && allParams != Null.Value)
         parameters = JurassicHelper.Coerce<AllParams>(this.Engine, allParams);

       string sql = BuildSelect(parameters.Where, parameters.OrderBy, parameters.Limit);
       return Query(string.Format(sql, parameters.Columns, GetFullTableName()), parameters.Args);
     }

    /// <summary>
    /// Returns the number of records in the table.
    /// </summary>
    /// <returns></returns>
    [JSFunction(Name = "count")]
     public int Count(object countParams)
    {
      var parameters = new CountParams(this.Engine.Object.InstancePrototype);
      if (countParams != null && countParams != Null.Value && countParams != Undefined.Value)
        parameters = JurassicHelper.Coerce<CountParams>(this.Engine, countParams);

      if (String.IsNullOrEmpty(parameters.TableName))
        parameters.TableName = GetFullTableName();

      if (String.IsNullOrEmpty(parameters.TableName))
        throw new JavaScriptException(this.Engine, "Error", "A table name must be specified either as a parameter or via the tableName property on the DynamicModel instance.");

      if (String.IsNullOrEmpty(parameters.Where))
        return (int)Scalar(String.Format("SELECT COUNT(*) FROM {0}", parameters.TableName));
      
      return (int)Scalar(String.Format("SELECT COUNT(*) FROM {0} WHERE {1}", parameters.TableName, parameters.Where));
    }

    /// <summary>
    /// Removes one or more records from the DB according to the passed-in WHERE
    /// </summary>
    [JSFunction(Name = "delete")]
    public int Delete(object deleteParams)
    {
      if (deleteParams == null || deleteParams == Null.Value || deleteParams == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "Must specify an object with where or key");
      
      var parameters = JurassicHelper.Coerce<DeleteParams>(this.Engine, deleteParams);

      int result = Execute(CreateDeleteCommand(parameters));
      return result;
    }

    /// <summary>
    /// Executes the specified sql statement.
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    [JSFunction(Name = "execute")]
    public int Execute(string sql, params object[] args)
    {
      return Execute(CreateCommand(sql, null, args));
    }

    /// <summary>
    /// Gets a default value for the column
    /// </summary>
    [JSFunction(Name = "getDefaultValueForColumn")]
    public object GetDefaultValueForColumn(object column)
    {
      var columnInstance = column as ObjectInstance;
      if (columnInstance == null)
        return null;

      object result;

      string def = columnInstance.GetPropertyValue("COLUMN_DEFAULT") as string;

      if (String.IsNullOrEmpty(def))
      {
        result = null;
      }
      else if (def == "getdate()" || def == "(getdate())")
      {
        result = DateTime.Now.ToShortDateString();
      }
      else if (def == "newid()")
      {
        result = Guid.NewGuid().ToString();
      }
      else
      {
        result = def.Replace("(", "").Replace(")", "");
      }

      return result;
    }

    /// <summary>
    /// If the object passed in has a property with the same name as your PrimaryKeyField
    /// it is returned here.
    /// </summary>
    [JSFunction(Name = "getPrimaryKeyValue")]
    public object GetPrimaryKeyValue(object o)
    {
      if (o is ObjectInstance && HasPrimaryKey(o))
        return (o as ObjectInstance).GetPropertyValue(PrimaryKeyField);

      return null;
    }

    /// <summary>
    /// Creates an empty object instance with defaults from the DB
    /// </summary>
    [JSFunction(Name = "getPrototype")]
    public ObjectInstance GetPrototype()
    {
      ObjectInstance result = this.Engine.Object.Construct();
      var schema = this.Schema;
      foreach (var column in schema.ElementValues.OfType<ObjectInstance>())
      {
        var name = (string)column.GetPropertyValue("COLUMN_NAME");
        result.SetPropertyValue(name, GetDefaultValueForColumn(column), false);
      }

      return result;
    }

    [JSFunction(Name = "getFullTableName")]
    public string GetFullTableName()
    {
      string result = String.IsNullOrEmpty(this.TableSchema)
                        ? String.Format("[{0}].[{1}]", this.DatabaseName, this.TableName)
                        : String.Format("[{0}].[{1}].[{2}]", this.DatabaseName, this.TableSchema, this.TableName);

      return result;
    }

    /// <summary>
    /// Conventionally introspects the object passed in for a field that 
    /// looks like a PK. If you've named your PrimaryKeyField, this becomes easy
    /// </summary>
    [JSFunction(Name = "hasPrimaryKey")]
    public bool HasPrimaryKey(object o)
    {
      if (o is ObjectInstance)
        return (o as ObjectInstance).HasProperty(PrimaryKeyField);

      return false;
    }

    /// <summary>
    /// Adds a record to the database. You can pass in an Anonymous object, an ExpandoObject,
    /// A regular old POCO, or a NameValueColletion from a Request.Form or Request.QueryString
    /// </summary>
    public object Insert(object o)
    {
      var ex = o as ObjectInstance;

      if (ex == null)
        return Null.Value;

      using (var conn = OpenConnection())
      {
        var cmd = CreateInsertCommand(ex);
        cmd.Connection = conn;
        cmd.ExecuteNonQuery();
        cmd.CommandText = "SELECT @@IDENTITY as newID";
        ex.SetPropertyValue("ID", cmd.ExecuteScalar(), false);
      }
      return ex;
    }

    /// <summary>
    /// Returns a dynamic PagedResult. Result properties are Items, TotalPages, and TotalRecords.
    /// </summary>
    [JSFunction(Name = "paged")]
    public ObjectInstance Paged(object pagedParams)
    {
      PagedParams parameters = new PagedParams(this.Engine.Object.InstancePrototype);

      if (pagedParams != null && pagedParams != Undefined.Value && pagedParams != Null.Value)
        parameters = JurassicHelper.Coerce<PagedParams>(this.Engine, pagedParams);

      return BuildPagedResult(parameters);
    }

    /// <summary>
    /// Enumerates the reader yielding the result
    /// </summary>
    [JSFunction(Name = "query")]
    public ArrayInstance Query(string sql, params object[] args)
    {
      var result = this.Engine.Array.Construct();

      using (var conn = OpenConnection())
      {
        var rdr = CreateCommand(sql, conn, args).ExecuteReader();
        while (rdr.Read())
        {
          ArrayInstance.Push(result, rdr.RecordToObjectInstance(this.Engine));
        }
      }

      return result;
    }

    /// <summary>
    /// Executes a set of objects as Insert or Update commands based on their property settings, within a transaction.
    /// These objects can be POCOs, Anonymous, NameValueCollections, or Expandos. Objects
    /// With a PK property (whatever PrimaryKeyField is set to) will be created at UPDATEs
    /// </summary>
    [JSFunction(Name = "save")]
    public int Save(params object[] things)
    {
      var commands = BuildCommands(things);
      return Execute(commands);
    }

    /// <summary>
    /// Returns a single result
    /// </summary>
    [JSFunction(Name = "scalar")]
    public object Scalar(string sql, params object[] args)
    {
      object result;
      using (var conn = OpenConnection())
      {
        var command = CreateCommand(sql, conn, args);

        foreach (var param in args)
          command.AddParam(param);

        result = command.ExecuteScalar();
      }
      return result;
    }

    /// <summary>
    /// Returns a single row from the database
    /// </summary>
    [JSFunction(Name="single")]
    public object Single(object where, params object[] args)
    {
      string stringWhere;

      if (where == Undefined.Value || where == Null.Value || where == null)
        stringWhere = "";
      else if (where is string)
        stringWhere = (string)where;
      else
        stringWhere = where.ToString();

      var sql = string.Format("SELECT * FROM {0} WHERE {1}", GetFullTableName(), stringWhere);
      return Query(sql, args).ElementValues.FirstOrDefault();
    }

    /// <summary>
    /// Updates a record in the database. You can pass in an Anonymous object, an ExpandoObject,
    /// A regular old POCO, or a NameValueCollection from a Request.Form or Request.QueryString
    /// </summary>
    public int Update(object o, object key)
    {
      var ex = o as ObjectInstance;
      if (ex == null)
        return 0;

      var result = Execute(CreateUpdateCommand(ex, key));
      return result;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Builds a set of Insert and Update commands based on the passed-on objects.
    /// These objects can be POCOs, Anonymous, NameValueCollections, or Expandos. Objects
    /// With a PK property (whatever PrimaryKeyField is set to) will be created at UPDATEs
    /// </summary>
    public virtual List<DbCommand> BuildCommands(params object[] things)
    {
      return things.Select(item => 
        HasPrimaryKey(item)
          ? CreateUpdateCommand(item, GetPrimaryKeyValue(item))
          : CreateInsertCommand(item)
        )
        .ToList();
    }

    private ObjectInstance BuildPagedResult(PagedParams parameters)
    {
      if (String.IsNullOrEmpty(parameters.Sql))
        parameters.Sql = "";

      if (String.IsNullOrEmpty(parameters.PrimaryKeyField))
        parameters.PrimaryKeyField = this.PrimaryKeyField;

      if (String.IsNullOrEmpty(parameters.PrimaryKeyField))
        throw new JavaScriptException(this.Engine, "Error", "The property named 'primaryKeyField' must be populated with the name of the primary key field to use.");

      if (String.IsNullOrEmpty(parameters.Where))
        parameters.Where = "";

      if (String.IsNullOrEmpty(parameters.OrderBy))
        parameters.OrderBy = parameters.PrimaryKeyField;

      if (String.IsNullOrEmpty(parameters.Columns))
        parameters.Columns = "*";

      if (parameters.PageSize <= 0)
        parameters.PageSize = 20;

      if (parameters.CurrentPage <= 0)
        parameters.CurrentPage = 1;

      var result = this.Engine.Object.Construct();

      string countSql = string.IsNullOrEmpty(parameters.Sql)
                          ? string.Format("SELECT COUNT({0}) FROM {1}", parameters.PrimaryKeyField, GetFullTableName())
                          : string.Format("SELECT COUNT({0}) FROM ({1}) AS PagedTable", parameters.PrimaryKeyField,
                                          parameters.Sql);
        
      if (!string.IsNullOrEmpty(parameters.Where))
      {
        if (!parameters.Where.Trim().StartsWith("where", StringComparison.CurrentCultureIgnoreCase))
        {
          parameters.Where = " WHERE " + parameters.Where;
        }
      }

      var query = string.IsNullOrEmpty(parameters.Sql)
                    ? string.Format(
                      "SELECT {0} FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS Row, {0} FROM {2} {3}) AS Paged ",
                      parameters.Columns, parameters.OrderBy, GetFullTableName(), parameters.Where)
                    : string.Format(
                      "SELECT {0} FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS Row, {0} FROM ({2}) AS PagedTable {3}) AS Paged ",
                      parameters.Columns, parameters.OrderBy, parameters.Sql, parameters.Where);
        
      var pageStart = (parameters.CurrentPage - 1) * parameters.PageSize;
      query += string.Format(" WHERE Row > {0} AND Row <={1}", pageStart, (pageStart + parameters.PageSize));
      countSql += parameters.Where;

      var totalRecords = (int)Scalar(countSql, parameters.Args);
      var totalPages = totalRecords / parameters.PageSize;
      if (totalRecords % parameters.PageSize > 0)
        totalPages += 1;

      result.SetPropertyValue("TotalRecords", totalRecords, false);
      result.SetPropertyValue("TotalPages", totalPages, false);
      result.SetPropertyValue("Items", Query(string.Format(query, parameters.Columns, TableName), parameters.Args), false);
      return result;
    }

    /// <summary>
    /// Creates a DBCommand that you can use for loving your database.
    /// </summary>
    private DbCommand CreateCommand(string sql, DbConnection conn, params object[] args)
    {
      var result = m_factory.CreateCommand();

      if (result == null)
        throw new InvalidOperationException("DbCommand returned by DbProviderFactory was null.");

      result.Connection = conn;
      result.CommandText = sql;
      if (args.Length > 0)
        result.AddParams(args);
      return result;
    }

    private DbCommand CreateInsertCommand(object o)
    {
      var objectInstance = o as ObjectInstance;

      if (objectInstance == null)
        throw new ArgumentOutOfRangeException("o", @"Expected ObjectInstance.");

      var sbKeys = new StringBuilder();
      var sbVals = new StringBuilder();
      const string stub = "INSERT INTO {0} ({1}) \r\n VALUES ({2})";

      DbCommand result = CreateCommand(stub, null);

      int counter = 0;

      foreach (var item in objectInstance.Properties)
      {
        sbKeys.AppendFormat("{0},", item.Name);
        sbVals.AppendFormat("@{0},", counter.ToString(CultureInfo.InvariantCulture));

        if (item.Value == Null.Value || item.Value == Undefined.Value)
          result.AddParam(DBNull.Value);
        else if (item.Value is DateInstance)
          result.AddParam(DateTime.Parse(((DateInstance)item.Value).ToIsoString()));
        else if (item.Value is Base64EncodedByteArrayInstance)
          result.AddParam(((Base64EncodedByteArrayInstance)item.Value).Data);
        else
          result.AddParam(item.Value);
        counter++;
      }

      if (counter > 0)
      {
        var keys = sbKeys.ToString().Substring(0, sbKeys.Length - 1);
        var vals = sbVals.ToString().Substring(0, sbVals.Length - 1);
        var sql = string.Format(stub, GetFullTableName(), keys, vals);
        result.CommandText = sql;
      }
      else
        throw new InvalidOperationException("Can't parse this object to the database - there are no properties set");

      return result;
    }

    /// <summary>
    /// Creates a command for use with transactions - internal stuff mostly, but here for you to play with
    /// </summary>
    private DbCommand CreateUpdateCommand(object o, object key)
    {
      var objectInstance = o as ObjectInstance;

      if (objectInstance == null)
        throw new ArgumentOutOfRangeException("o", @"Expected ObjectInstance.");

      var sbKeys = new StringBuilder();
      const string stub = "UPDATE {0} SET {1} WHERE {2} = @{3}";

      var result = CreateCommand(stub, null);
      int counter = 0;

      foreach (var item in objectInstance.Properties)
      {
        var val = item.Value;
        if (!item.Name.Equals(PrimaryKeyField, StringComparison.OrdinalIgnoreCase) && item.Value != null)
        {
          if (val == Null.Value || val == Undefined.Value)
            result.AddParam(DBNull.Value);
          if (val is DateInstance)
            result.AddParam(DateTime.Parse(((DateInstance)val).ToIsoString()));
          else if (val is Base64EncodedByteArrayInstance)
            result.AddParam(((Base64EncodedByteArrayInstance)val).Data);
          else
            result.AddParam(val);

          sbKeys.AppendFormat("{0} = @{1}, \r\n", item.Name, counter.ToString(CultureInfo.InvariantCulture));
          counter++;
        }
      }

      if (counter > 0)
      {
        //add the key
        result.AddParam(key);
        //strip the last commas
        var keys = sbKeys.ToString().Substring(0, sbKeys.Length - 4);
        result.CommandText = string.Format(stub, GetFullTableName(), keys, PrimaryKeyField, counter);
      }
      else
        throw new InvalidOperationException("No parsable object was sent in - could not divine any name/value pairs");

      return result;
    }

    /// <summary>
    /// Removes one or more records from the DB according to the passed-in WHERE
    /// </summary>
    private DbCommand CreateDeleteCommand(DeleteParams parameters)
    {
      if (String.IsNullOrEmpty(parameters.Where))
        parameters.Where = "";

      var sql = string.Format("DELETE FROM {0} ", GetFullTableName());
      if (parameters.Key != null)
      {
        sql += string.Format("WHERE {0}=@0", PrimaryKeyField);
        parameters.Args = new object[] { parameters.Key };
      }
      else if (String.IsNullOrEmpty(parameters.Where) == false)
      {
        sql += parameters.Where.Trim().StartsWith("where", StringComparison.OrdinalIgnoreCase) ? parameters.Where : "WHERE " + parameters.Where;
      }
      return CreateCommand(sql, null, parameters.Args);
    }

    private int Execute(DbCommand command)
    {
      return Execute(new[] { command });
    }

    /// <summary>
    /// Executes a series of DBCommands in a transaction
    /// </summary>
    private int Execute(IEnumerable<DbCommand> commands)
    {
      var result = 0;
      using (var conn = OpenConnection())
      {
        using (var tx = conn.BeginTransaction())
        {
          foreach (var cmd in commands)
          {
            cmd.Connection = conn;
            cmd.Transaction = tx;
            result += cmd.ExecuteNonQuery();
          }
          tx.Commit();
        }
      }
      return result;
    }

    /// <summary>
    /// Returns an Open Connection
    /// </summary>
    private DbConnection OpenConnection()
    {
      var result = m_factory.CreateConnection();

      if (result == null)
        throw new InvalidOperationException("The DbConnection returned by the DbProvider factory was null.");

      result.ConnectionString = ConnectionString;
      result.Open();
      return result;
    }

    private IEnumerable<ObjectInstance> Query(string sql, DbConnection connection, params object[] args)
    {
      using (var rdr = CreateCommand(sql, connection, args).ExecuteReader())
      {
        while (rdr.Read())
        {
          yield return rdr.RecordToObjectInstance(this.Engine);
        }
      }
    }
    #endregion

    #region Private Static Methods
    private static string BuildSelect(string where, string orderBy, int limit)
    {
      string sql = limit > 0 ? "SELECT TOP " + limit + " {0} FROM {1} " : "SELECT {0} FROM {1} ";
      if (!string.IsNullOrEmpty(where))
        sql += where.Trim().StartsWith("where", StringComparison.OrdinalIgnoreCase) ? where : " WHERE " + where;
      if (!String.IsNullOrEmpty(orderBy))
        sql += orderBy.Trim().StartsWith("order by", StringComparison.OrdinalIgnoreCase) ? orderBy : " ORDER BY " + orderBy;
      return sql;
    }
    #endregion

    #region Nested Classes
    public sealed class AllParams : ObjectInstance
    {
      public AllParams(ObjectInstance prototype)
        : base(prototype)
      {
        this.Where = "";
        this.OrderBy = "";
        this.Columns = "*";
        this.Args = new object[0];

        this.PopulateFields();
      }

      [JSProperty(Name = "where")]
      [JsonProperty("where")]
      public string Where
      {
        get;
        set;
      }

      [JSProperty(Name = "orderBy")]
      [JsonProperty("orderBy")]
      public string OrderBy
      {
        get;
        set;
      }

      [JSProperty(Name = "limit")]
      [JsonProperty("limit")]
      public int Limit
      {
        get;
        set;
      }

      [JSProperty(Name = "columns")]
      [JsonProperty("columns")]
      public string Columns
      {
        get;
        set;
      }

      [JSProperty(Name = "args")]
      [JsonProperty("args")]
      public Object[] Args
      {
        get;
        set;
      }
    }

    public sealed class CountParams : ObjectInstance
    {
      public CountParams(ObjectInstance prototype)
        : base(prototype)
      {
        this.TableName = "";
        this.Where = "";

        this.PopulateFields();
      }

      [JSProperty(Name = "tableName")]
      [JsonProperty("tableName")]
      public string TableName
      {
        get;
        set;
      }

      [JSProperty(Name = "where")]
      [JsonProperty("where")]
      public string Where
      {
        get;
        set;
      }
    }

    public sealed class PagedParams : ObjectInstance
    {
      public PagedParams(ObjectInstance prototype)
        : base(prototype)
      {
        this.Sql = "";
        this.PrimaryKeyField = "";
        this.Where = "";
        this.OrderBy = "";
        this.Columns = "*";
        this.CurrentPage = 1;
        this.PageSize = 20;
        this.Args = new object[0];

        this.PopulateFields();
      }

      [JSProperty(Name = "where")]
      [JsonProperty("where")]
      public string Where
      {
        get;
        set;
      }

      [JSProperty(Name = "orderBy")]
      [JsonProperty("orderBy")]
      public string OrderBy
      {
        get;
        set;
      }

      [JSProperty(Name = "columns")]
      [JsonProperty("columns")]
      public string Columns
      {
        get;
        set;
      }

      [JSProperty(Name = "sql")]
      [JsonProperty("sql")]
      public string Sql
      {
        get;
        set;
      }

      [JSProperty(Name = "pageSize")]
      [JsonProperty("pageSize")]
      public int PageSize
      {
        get;
        set;
      }

      [JSProperty(Name = "primaryKeyField")]
      [JsonProperty("primaryKeyField")]
      public string PrimaryKeyField
      {
        get;
        set;
      }

      [JSProperty(Name = "currentPage")]
      [JsonProperty("currentPage")]
      public int CurrentPage
      {
        get;
        set;
      }

      [JSProperty(Name = "args")]
      [JsonProperty("args")]
      public object[] Args
      {
        get;
        set;
      }
    }

    public sealed class DeleteParams : ObjectInstance
    {
      public DeleteParams(ObjectInstance prototype)
        : base(prototype)
      {
        this.Where = String.Empty;
        this.Key = null;
        this.Args = new object[0];

        this.PopulateFields();
      }

      [JSProperty(Name = "where")]
      [JsonProperty("where")]
      public string Where
      {
        get;
        set;
      }

      [JSProperty(Name = "key")]
      [JsonProperty("key")]
      public int? Key
      {
        get;
        set;
      }

      [JSProperty(Name = "args")]
      [JsonProperty("args")]
      public object[] Args
      {
        get;
        set;
      }
    }
    #endregion
  }
}
