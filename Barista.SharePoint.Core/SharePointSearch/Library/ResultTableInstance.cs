namespace Barista.SharePoint.SharePointSearch.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Barista.Newtonsoft.Json;
    using Barista.Newtonsoft.Json.Converters;
    using Microsoft.Office.Server.Search.Query;

    [Serializable]
    public class ResultTableConstructor : ClrFunction
    {
        public ResultTableConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "ResultTable", new ResultTableInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public ResultTableInstance Construct()
        {
            return new ResultTableInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class ResultTableInstance : ObjectInstance
    {
        private readonly ResultTable m_resultTable;

        public ResultTableInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFunctions();
        }

        public ResultTableInstance(ObjectInstance prototype, ResultTable resultTable)
            : this(prototype)
        {
            if (resultTable == null)
                throw new ArgumentNullException("resultTable");

            m_resultTable = resultTable;
        }

        public ResultTable ResultTable
        {
            get
            {
                return m_resultTable;
            }
        }

        [JSProperty(Name = "depth")]
        public int Depth
        {
            get
            {
                return m_resultTable.Depth;
            }
        }

        [JSProperty(Name = "fieldCount")]
        public int FieldCount
        {
            get
            {
                return m_resultTable.FieldCount;
            }
        }

        [JSProperty(Name = "isClosed")]
        public bool IsClosed
        {
            get
            {
                return m_resultTable.IsClosed;
            }
        }

        [JSProperty(Name = "isTotalRowsExact")]
        public bool IsTotalRowsExact
        {
            get
            {
                return m_resultTable.IsTotalRowsExact;
            }
        }

        [JSProperty(Name = "recordsAffected")]
        public int RecordsAffected
        {
            get
            {
                return m_resultTable.RecordsAffected;
            }
        }

        [JSProperty(Name = "resultType")]
        public string ResultType
        {
            get
            {
                return m_resultTable.ResultType.ToString();
            }
        }

        [JSProperty(Name = "rowCount")]
        public int RowCount
        {
            get
            {
                return m_resultTable.RowCount;
            }
        }

        [JSProperty(Name = "totalRows")]
        public int TotalRows
        {
            get
            {
                return m_resultTable.TotalRows;
            }
        }

        [JSProperty(Name = "totalRowsIncludingDuplicates")]
        public int TotalRowsIncludingDuplicates
        {
            get
            {
                return m_resultTable.TotalRowsIncludingDuplicates;
            }
        }

        [JSFunction(Name = "close")]
        public void Close()
        {
            m_resultTable.Close();
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            m_resultTable.Dispose();
        }

        [JSFunction(Name = "getBoolean")]
        public bool GetBoolean(int index)
        {
            return m_resultTable.GetBoolean(index);
        }

        [JSFunction(Name = "getDataTypeName")]
        public string GetDataTypeName(int index)
        {
            return m_resultTable.GetDataTypeName(index);
        }

        [JSFunction(Name = "getDateTime")]
        public DateInstance GetDateTime(int index)
        {
            var result = m_resultTable.GetDateTime(index);
            return JurassicHelper.ToDateInstance(this.Engine, result);
        }

        [JSFunction(Name = "getFloat")]
        public float GetFloat(int index)
        {
            return m_resultTable.GetFloat(index);
        }

        [JSFunction(Name = "getGuid")]
        public GuidInstance GetGuid(int index)
        {
            var result = m_resultTable.GetGuid(index);
            return new GuidInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getInt32")]
        public int GetInt32(int index)
        {
            return m_resultTable.GetInt32(index);
        }

        [JSFunction(Name = "getName")]
        public string GetName(int index)
        {
            return m_resultTable.GetName(index);
        }

        [JSFunction(Name = "getOrdinal")]
        public int GetOrdinal(string name)
        {
            return m_resultTable.GetOrdinal(name);
        }

        [JSFunction(Name = "getSchemaTable")]
        public object GetSchemaTable()
        {
            var result = JsonConvert.SerializeObject(m_resultTable.GetSchemaTable(), new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new DataTableConverter() }
            });
            return JSONObject.Parse(this.Engine, result, null);
        }

        [JSFunction(Name = "getString")]
        public string GetString(int index)
        {
            return m_resultTable.GetString(index);
        }

        [JSFunction(Name = "getValue")]
        public object GetValue(int index)
        {
            return m_resultTable.GetValue(index);
        }


        //GetValues

        [JSFunction(Name = "getValueByIndex")]
        public object GetColumnByIndex(int index)
        {
           return m_resultTable[index];
        }

        [JSFunction(Name = "getValueByName")]
        public object GetColumnByName(string name)
        {
            return m_resultTable[name];
        }

        [JSFunction(Name = "getTableAsObject")]
        public object GetTableAsObject()
        {
            var result = JsonConvert.SerializeObject(m_resultTable.Table, new JsonSerializerSettings {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new DataTableConverter() }
            });
            return JSONObject.Parse(this.Engine, result, null);
        }

        [JSFunction(Name = "isDbNull")]
        public bool IsDbNull(int index)
        {
            return m_resultTable.IsDBNull(index);
        }

        [JSFunction(Name = "nextResult")]
        public bool NextResult()
        {
            return m_resultTable.NextResult();
        }

        [JSFunction(Name = "read")]
        public bool Read()
        {
            return m_resultTable.Read();
        }
    }
}
