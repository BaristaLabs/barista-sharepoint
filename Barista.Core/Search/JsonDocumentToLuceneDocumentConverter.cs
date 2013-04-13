namespace Barista.Search
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using Barista.Newtonsoft.Json;
  using Barista.Newtonsoft.Json.Linq;
  using Lucene.Net.Documents;
  using Barista.Extensions;

  public class JsonDocumentToLuceneDocumentConverter
  {
    private readonly IndexDefinition m_indexDefinition;
    private readonly List<int> m_multipleItemsSameFieldCount = new List<int>();
    private readonly Dictionary<FieldCacheKey, Field> m_fieldsCache = new Dictionary<FieldCacheKey, Field>();
    private readonly Dictionary<FieldCacheKey, NumericField> m_numericFieldsCache = new Dictionary<FieldCacheKey, NumericField>();

    public JsonDocumentToLuceneDocumentConverter(IndexDefinition indexDefinition)
    {
      this.m_indexDefinition = indexDefinition;
    }

    public IEnumerable<AbstractField> Index(JObject document, Field.Store defaultStorage)
    {
      return from property in document.Properties()
             where property.Name != Constants.DocumentIdFieldName
             from field in CreateFields(property.Name, property.Value, defaultStorage, Field.TermVector.NO)
             select field;
    }

    public IEnumerable<AbstractField> Index(JsonDocument document, Field.Store defaultStorage)
    {
      var metadataFields = from property in document.Metadata.Properties()
                           where property.Name != Constants.DocumentIdFieldName
                           from field in CreateFields("@" + property.Name, property.Value, defaultStorage, Field.TermVector.NO)
                           select field;

      var dataFields = from property in document.DataAsJson.Properties()
                       where property.Name != Constants.DocumentIdFieldName
                       from field in CreateFields(property.Name, property.Value, defaultStorage, Field.TermVector.NO)
                       select field;

      return metadataFields.Union(dataFields);
    }

    /// <summary>
    /// This method generate the fields for indexing documents in lucene from the values.
    /// Given a name and a value, it has the following behavior:
    /// * If the value is enumerable, index all the items in the enumerable under the same field name
    /// * If the value is null, create a single field with the supplied name with the unanalyzed value 'NULL_VALUE'
    /// * If the value is string or was set to not analyzed, create a single field with the supplied name
    /// * If the value is date, create a single field with millisecond precision with the supplied name
    /// * If the value is numeric (int, long, double, decimal, or float) will create two fields:
    ///		1. with the supplied name, containing the numeric value as an unanalyzed string - useful for direct queries
    ///		2. with the name: name +'_Range', containing the numeric value in a form that allows range queries
    /// </summary>
    public IEnumerable<AbstractField> CreateFields(string name, JToken value, Field.Store defaultStorage, Field.TermVector defaultTermVector)
    {
      if (name.IsNullOrWhiteSpace())
        throw new ArgumentException(@"Field must be not null, not empty and cannot contain whitespace", "name");

      if (char.IsLetter(name[0]) == false &&
        name[0] != '_')
      {
        name = "_" + name;
      }

      var fieldIndexingOptions = m_indexDefinition.GetIndex(name, null);
      var storage = m_indexDefinition.GetStorage(name, defaultStorage);
      var termVector = m_indexDefinition.GetTermVector(name, defaultTermVector);

      if (Equals(fieldIndexingOptions, Field.Index.NOT_ANALYZED) ||
        Equals(fieldIndexingOptions, Field.Index.NOT_ANALYZED_NO_NORMS))// explicitly not analyzed
      {
        // date time, time span and date time offset have the same structure for analyzed and not analyzed.
        if (value.Type != JTokenType.Date && value.Type != JTokenType.TimeSpan)
        {
          yield return CreateFieldWithCaching(name, value.ToString(), storage,
                            m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS), termVector);
          yield break;
        }
      }
      else
      {
        switch (value.Type)
        {
          case JTokenType.Array:
            {
              //Add each item in the array as a field with the same name.
              int count = 1;

              //Return an _IsArray field.
              if (Equals(storage, Field.Store.NO) == false)
                yield return new Field(name + "_IsArray", "true", Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO);

              foreach (var arrayValue in value.Values())
              {
                if (CanCreateFieldsForNestedArray(arrayValue, fieldIndexingOptions) == false)
                  continue;

                m_multipleItemsSameFieldCount.Add(count++);
                foreach (var field in CreateFields(name, arrayValue, storage, Field.TermVector.NO))
                  yield return field;
                m_multipleItemsSameFieldCount.RemoveAt(m_multipleItemsSameFieldCount.Count - 1);
              }
              break;
            }
          case JTokenType.Boolean:
            {
              yield return new Field(name, (value.Value<bool>()) ? "true" : "false", storage,
                 m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS), termVector);
            }
            break;
          case JTokenType.Bytes:
            {
              var bytes = value.Value<byte[]>();
              if (bytes != null)
              {
                yield return CreateBinaryFieldWithCaching(name, bytes, storage, fieldIndexingOptions, termVector);
              }
            }
            break;
          case JTokenType.Date:
            {
              var val = value.Value<DateTime>();
              var dateAsString = val.ToString(Default.DateTimeFormatsToWrite);
              if (val.Kind == DateTimeKind.Utc)
                dateAsString += "Z";
              yield return CreateFieldWithCaching(name, dateAsString, storage,
                                                  m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS),
                                                  termVector);
            }
            break;
          case JTokenType.Guid:
            {
              yield return new Field(name, value.Value<Guid>().ToString(), storage,
                                     m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS), termVector);
            }
            break;

          case JTokenType.None:
          case JTokenType.Null:
            {
              yield return CreateFieldWithCaching(name, Constants.NullValue, storage,
                                                  Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO);
            }
            break;
          case JTokenType.Object:
            {
              //Add an _IsObject field
              if (Equals(storage, Field.Store.NO) == false)
                yield return new Field(name + "_IsObject", "true", Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO);

              //Recursively add properties on the object.
              foreach (var objectValue in value.Children<JProperty>())
              {
                if (CanCreateFieldsForNestedObject(objectValue, fieldIndexingOptions) == false)
                  continue;

                foreach (var field in CreateFields(name + "." + objectValue.Name, objectValue.Value, storage, defaultTermVector))
                  yield return field;
              }
            }
            break;
          case JTokenType.String:
            {
              if (Equals(value.Value<string>(), string.Empty))
              {
                yield return CreateFieldWithCaching(name, Constants.EmptyString, storage,
                       Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO);
                yield break;
              }
              var index = m_indexDefinition.GetIndex(name, Field.Index.ANALYZED);
              yield return CreateFieldWithCaching(name, value.ToString(), storage, index, termVector);
            }
            break;
          case JTokenType.Float:
            {
              var f = value.Value<float>();
              yield return CreateFieldWithCaching(name, f.ToString(CultureInfo.InvariantCulture), storage,
                 m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS), termVector);
            }
            break;
          case JTokenType.Integer:
            {
              var i = value.Value<int>();
              yield return CreateFieldWithCaching(name, i.ToString(CultureInfo.InvariantCulture), storage,
                 m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS), termVector);
            }
            break;
          case JTokenType.TimeSpan:
            {
              var val = value.Value<TimeSpan>();
              yield return CreateFieldWithCaching(name, val.ToString("c"), storage,
                     m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS), termVector);
            }
            break;
          case JTokenType.Uri:
            {
              yield return new Field(name, value.Value<Uri>().ToString(), storage,
                                     m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS), termVector);
            }
            break;
          case JTokenType.Undefined:
          case JTokenType.Raw:
          case JTokenType.Property:
          case JTokenType.Constructor:
          case JTokenType.Comment:
            //Do Nothing...
            break;
          default:
            throw new ArgumentOutOfRangeException("The specified JToken Type: " + value.Type + " is invalid or has not been implemented.");
        }
      }

      //Create the range fields for numeric field types:
      foreach (var numericField in CreateNumericFieldWithCaching(name, value, storage, termVector))
        yield return numericField;
    }

    private IEnumerable<AbstractField> CreateNumericFieldWithCaching(string name, JToken value,
      Field.Store defaultStorage, Field.TermVector termVector)
    {

      var fieldName = name + "_Range";
      var storage = m_indexDefinition.GetStorage(name, defaultStorage);
      var cacheKey = new FieldCacheKey(name, null, storage, termVector, m_multipleItemsSameFieldCount.ToArray());
      NumericField numericField;
      if (m_numericFieldsCache.TryGetValue(cacheKey, out numericField) == false)
      {
        m_numericFieldsCache[cacheKey] = numericField = new NumericField(fieldName, storage, true);
      }

      switch (value.Type)
      {
        case JTokenType.Float:
          if (m_indexDefinition.GetSortOption(name) == SortOptions.Double)
            yield return numericField.SetDoubleValue(value.Value<float>());
          else
            yield return numericField.SetFloatValue(value.Value<float>());
          break;
        case JTokenType.Integer:
          if (m_indexDefinition.GetSortOption(name) == SortOptions.Long)
            yield return numericField.SetLongValue(value.Value<long>());
          else
            yield return numericField.SetIntValue(value.Value<int>());
          break;
        case JTokenType.TimeSpan:
          yield return numericField.SetLongValue(((TimeSpan)value).Ticks);
          break;
      }
    }

    private Field CreateBinaryFieldWithCaching(string name, byte[] value, Field.Store store, Field.Index index, Field.TermVector termVector)
    {
      if (value.Length > 1024)
        throw new ArgumentException("Binary values must be smaller than 1Kb");

      var cacheKey = new FieldCacheKey(name, null, store, termVector, m_multipleItemsSameFieldCount.ToArray());
      Field field;
      var stringWriter = new StringWriter();
      JsonExtensions.CreateDefaultJsonSerializer().Serialize(stringWriter, value);
      var sb = stringWriter.GetStringBuilder();
      sb.Remove(0, 1); // remove prefix "
      sb.Remove(sb.Length - 1, 1); // remove postfix "
      var val = sb.ToString();

      if (m_fieldsCache.TryGetValue(cacheKey, out field) == false)
      {
        m_fieldsCache[cacheKey] = field = new Field(name, val, store, index, termVector);
      }
      field.SetValue(val);
      field.Boost = 1;
      field.OmitNorms = true;
      return field;
    }

    public class FieldCacheKey
    {
      private readonly string m_name;
      private readonly Field.Index? m_index;
      private readonly Field.Store m_store;
      private readonly Field.TermVector m_termVector;
      private readonly int[] m_multipleItemsSameField;

      public FieldCacheKey(string name, Field.Index? index, Field.Store store, Field.TermVector termVector, int[] multipleItemsSameField)
      {
        this.m_name = name;
        this.m_index = index;
        this.m_store = store;
        this.m_termVector = termVector;
        this.m_multipleItemsSameField = multipleItemsSameField;
      }


      protected bool Equals(FieldCacheKey other)
      {
        return string.Equals(m_name, other.m_name) &&
          Equals(m_index, other.m_index) &&
          Equals(m_store, other.m_store) &&
          Equals(m_termVector, other.m_termVector) &&
          m_multipleItemsSameField.SequenceEqual(other.m_multipleItemsSameField);
      }

      public override bool Equals(object obj)
      {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != typeof(FieldCacheKey)) return false;
        return Equals((FieldCacheKey)obj);
      }

      public override int GetHashCode()
      {
        unchecked
        {
          int hashCode = (m_name != null ? m_name.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ (m_index != null ? m_index.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ m_store.GetHashCode();
          hashCode = (hashCode * 397) ^ m_termVector.GetHashCode();
          hashCode = m_multipleItemsSameField.Aggregate(hashCode, (h, x) => h * 397 ^ x);
          return hashCode;
        }
      }
    }

    private Field CreateFieldWithCaching(string name, string value, Field.Store store, Field.Index index, Field.TermVector termVector)
    {
      var cacheKey = new FieldCacheKey(name, index, store, termVector, m_multipleItemsSameFieldCount.ToArray());
      Field field;

      if (m_fieldsCache.TryGetValue(cacheKey, out field) == false)
        m_fieldsCache[cacheKey] = field = new Field(name, value, store, index, termVector);

      field.SetValue(value);
      field.Boost = 1;
      field.OmitNorms = true;
      return field;
    }

    private static bool CanCreateFieldsForNestedArray(JToken value, Field.Index fieldIndexingOptions)
    {
      if (!fieldIndexingOptions.IsAnalyzed())
        return true;

      if (value == null || value.Type == JTokenType.Null)
        return false;

      return true;
    }

    private static bool CanCreateFieldsForNestedObject(JToken value, Field.Index fieldIndexingOptions)
    {
      if (!fieldIndexingOptions.IsAnalyzed())
        return true;

      if (value == null || value.Type == JTokenType.Null)
        return false;

      return true;
    }

    public static IEnumerable<BatchedDocument> ConvertJsonDocumentToLuceneDocument(IndexDefinition definition,
                                                               IEnumerable<JsonDocument> jsonDocuments)
    {
      var converter = new JsonDocumentToLuceneDocumentConverter(definition);
      return jsonDocuments.Select(jsonDocument =>
        {
          var fields = converter.Index(jsonDocument, Field.Store.NO);

          var luceneDoc = new Document();
          var documentIdField = new Field(Constants.DocumentIdFieldName, jsonDocument.DocumentId, Field.Store.YES,
                                          Field.Index.ANALYZED_NO_NORMS);
          luceneDoc.Add(documentIdField);

          var tempJsonDocument = new Services.JsonDocument
          {
            DocumentId = jsonDocument.DocumentId,
            MetadataAsJson = jsonDocument.Metadata.ToString(),
            DataAsJson = jsonDocument.DataAsJson.ToString()
          };

          var jsonDocumentField = new Field(Constants.JsonDocumentFieldName, JsonConvert.SerializeObject(tempJsonDocument), Field.Store.YES,
                                          Field.Index.NOT_ANALYZED_NO_NORMS);

          luceneDoc.Add(jsonDocumentField);

          foreach (var field in fields)
          {
            luceneDoc.Add(field);
          }

          return new BatchedDocument
            {
              DocumentId = jsonDocument.DocumentId,
              Document = luceneDoc,
              SkipDeleteFromIndex = false,
            };
        });

    }
  }
}