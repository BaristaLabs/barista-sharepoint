namespace Barista.SharePoint.SharePointSearch.Library
{
    using System.Linq;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.Office.Server.Search.Query;

    [Serializable]
    public class ResultTableCollectionConstructor : ClrFunction
    {
        public ResultTableCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "ResultTableCollection", new ResultTableCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public ResultTableCollectionInstance Construct()
        {
            return new ResultTableCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class ResultTableCollectionInstance : ObjectInstance
    {
        private readonly ResultTableCollection m_resultTableCollection;

        public ResultTableCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public ResultTableCollectionInstance(ObjectInstance prototype, ResultTableCollection resultTableCollection)
            : this(prototype)
        {
            if (resultTableCollection == null)
                throw new ArgumentNullException("resultTableCollection");

            m_resultTableCollection = resultTableCollection;
        }

        public ResultTableCollection ResultTableCollection
        {
            get
            {
                return m_resultTableCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_resultTableCollection.Count;
            }
        }

        [JSProperty(Name = "databaseTime")]
        public int DatabaseTime
        {
            get
            {
                return m_resultTableCollection.DatabaseTime;
            }
        }
        
        //Definition

        [JSProperty(Name = "elapsedTime")]
        public int ElapsedTime
        {
            get
            {
                return m_resultTableCollection.ElapsedTime;
            }
        }

        [JSProperty(Name = "ignoredNoiseWords")]
        public ArrayInstance IgnoredNoiseWords
        {
            get
            {
                return m_resultTableCollection.IgnoredNoiseWords == null
                    ? null
// ReSharper disable once CoVariantArrayConversion
                    : this.Engine.Array.Construct(m_resultTableCollection.IgnoredNoiseWords);
            }
        }

        //KeywordInformation

        [JSProperty(Name = "queryModification")]
        public string QueryModification
        {
            get
            {
                return m_resultTableCollection.QueryModification;
            }
        }

        [JSProperty(Name = "queryProcessingTime")]
        public int QueryProcessingTime
        {
            get
            {
                return m_resultTableCollection.QueryProcessingTime;
            }
        }

        [JSProperty(Name = "queryTerms")]
        public ArrayInstance QueryTerms
        {
            get
            {
                return m_resultTableCollection.IgnoredNoiseWords == null
                    ? null
                    // ReSharper disable once CoVariantArrayConversion
                    : this.Engine.Array.Construct(m_resultTableCollection.QueryTerms);
            }
        }

        [JSProperty(Name = "spellingSuggestion")]
        public string SpellingSuggestion
        {
            get
            {
                return m_resultTableCollection.SpellingSuggestion;
            }
        }

        [JSFunction(Name = "exists")]
        public bool Exists(string resultType)
        {
            ResultType rt;
            return resultType.TryParseEnum(true, out rt) && m_resultTableCollection.Exists(rt);
        }

        [JSFunction(Name = "getResultTableByResultType")]
        public ResultTableInstance GetResultTableByResultType(string resultType)
        {
            ResultType rt;
            if (resultType.TryParseEnum(true, out rt))
            {
                var result = m_resultTableCollection[rt];
                return result == null
                    ? null
                    : new ResultTableInstance(this.Engine.Object.InstancePrototype, result);
            }

            return null;
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var resultTable in m_resultTableCollection.OfType<ResultTable>())
                ArrayInstance.Push(result, new ResultTableInstance(this.Engine.Object.InstancePrototype, resultTable));

            return result;
        }

    }
}
