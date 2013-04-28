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

  /// <summary>
  /// Converts a Json Document to a Lucene Document
  /// </summary>
  public class JsonDocumentToLuceneDocumentConverter
  {
    private readonly IndexDefinition m_indexDefinition;

    public JsonDocumentToLuceneDocumentConverter(IndexDefinition indexDefinition)
    {
      this.m_indexDefinition = indexDefinition;
    }

    /// <summary>
    /// Returns a collection of fields that represent the properties contained in the specified JObject.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="defaultStorage"></param>
    /// <returns></returns>
    public IEnumerable<AbstractField> Index(JObject document, Field.Store defaultStorage)
    {
      return from property in document.Properties()
             where property.Name != Constants.DocumentIdFieldName && property.Name.StartsWith("@@") == false
             from field in CreateFields(property.Name, property.Value, defaultStorage, Field.TermVector.NO)
             select field;
    }

    /// <summary>
    /// Returns a collection of fields that represent the properties contained in the specified JsonDocument.
    /// </summary>
    /// <param name="document"></param>
    /// <param name="defaultStorage"></param>
    /// <returns></returns>
    public IEnumerable<AbstractField> Index(JsonDocument document, Field.Store defaultStorage)
    {
      var metadataFields = from property in document.Metadata.Properties()
                           where property.Name != Constants.DocumentIdFieldName && property.Name.StartsWith("@@") == false
                           from field in CreateFields("@" + property.Name, property.Value, defaultStorage, Field.TermVector.NO)
                           select field;

      var dataFields = from property in document.DataAsJson.Properties()
                       where property.Name != Constants.DocumentIdFieldName && property.Name.StartsWith("@@") == false
                       from field in CreateFields(property.Name, property.Value, defaultStorage, Field.TermVector.WITH_POSITIONS_OFFSETS) // Maybe we shouldn't use position offsets.
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
    private IEnumerable<AbstractField> CreateFields(string name, JToken value, Field.Store defaultStorage, Field.TermVector defaultTermVector)
    {
      if (name.IsNullOrWhiteSpace())
        throw new ArgumentException(@"Field must be not null, not empty and cannot contain whitespace", "name");

      var fieldIndexingOptions = m_indexDefinition.GetIndex(name, null);
      var storage = m_indexDefinition.GetStorage(name, defaultStorage);
      var termVector = m_indexDefinition.GetTermVector(name, defaultTermVector);

      if (Equals(fieldIndexingOptions, Field.Index.NOT_ANALYZED) ||
        Equals(fieldIndexingOptions, Field.Index.NOT_ANALYZED_NO_NORMS))// explicitly not analyzed
      {
        // date time, time span and date time offset have the same structure for analyzed and not analyzed.
        if (value.Type != JTokenType.Date && value.Type != JTokenType.TimeSpan)
        {
          yield return new Field(name, value.ToString(), storage,
                            m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS), termVector);
        }
      }
      else
      {
        switch (value.Type)
        {
          case JTokenType.Array:
            {
              //Add each item in the array as a field with the same name.

              //Return an _IsArray field.
              if (Equals(storage, Field.Store.NO) == false)
                yield return new Field(name + "_IsArray", "true", Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO);

              var jArray = value as JArray;
              if (jArray == null)
                throw new InvalidOperationException("Shouldn't Happen");

              foreach (var arrayValue in jArray)
              {
                if (CanCreateFieldsForNestedArray(arrayValue, fieldIndexingOptions) == false)
                  continue;

                foreach (var field in CreateFields(name, arrayValue, storage, Field.TermVector.NO))
                  yield return field;
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
                yield return CreateBinaryField(name, bytes, storage, fieldIndexingOptions, termVector);
              }
            }
            break;
          case JTokenType.Date:
            {
              var val = value.Value<DateTime>();
              var dateAsString = val.ToString(Default.DateTimeFormatsToWrite);
              if (val.Kind == DateTimeKind.Utc)
                dateAsString += "Z";
              yield return new Field(name, dateAsString, storage,
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
              yield return new Field(name, Constants.NullValue, storage,
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
                yield return new Field(name, Constants.EmptyString, storage,
                       Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO);
                yield break;
              }
              var index = m_indexDefinition.GetIndex(name, Field.Index.ANALYZED);
              yield return new Field(name, value.ToString(), storage, index, termVector);
            }
            break;
          case JTokenType.Float:
            {
              var f = value.Value<float>();
              var index = m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS);
              yield return new Field(name, f.ToString(CultureInfo.InvariantCulture), storage, index, termVector);

              var numericField = new NumericField(name + "_Range", storage, true);
              if (m_indexDefinition.GetSortOption(name) == SortOptions.Double)
                yield return numericField.SetDoubleValue(value.Value<double>());
              else
                yield return numericField.SetFloatValue(value.Value<float>());
            }
            break;
          case JTokenType.Integer:
            {
              var i = value.Value<int>();
              var index = m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS);
              yield return new Field(name, i.ToString(CultureInfo.InvariantCulture), storage, index, termVector);

              var numericField = new NumericField(name + "_Range", storage, true);
              if (m_indexDefinition.GetSortOption(name) == SortOptions.Long)
                yield return numericField.SetLongValue(value.Value<long>());
              else
                yield return numericField.SetIntValue(value.Value<int>());
            }
            break;
          case JTokenType.TimeSpan:
            {
              var val = value.Value<TimeSpan>();
              var index = m_indexDefinition.GetIndex(name, Field.Index.NOT_ANALYZED_NO_NORMS);
              yield return new Field(name, val.ToString("c"), storage, index, termVector);

              var numericField = new NumericField(name + "_Range", storage, true);
              yield return numericField.SetLongValue(val.Ticks);
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
    }

    private Field CreateBinaryField(string name, byte[] value, Field.Store store, Field.Index index, Field.TermVector termVector)
    {
      if (value.Length > 1024)
        throw new ArgumentException("Binary values must be smaller than 1Kb");

      var stringWriter = new StringWriter();
      JsonExtensions.CreateDefaultJsonSerializer().Serialize(stringWriter, value);
      var sb = stringWriter.GetStringBuilder();
      sb.Remove(0, 1); // remove prefix "
      sb.Remove(sb.Length - 1, 1); // remove postfix "
      var val = sb.ToString();

      var field = new Field(name, val, store, index, termVector);
      field.SetValue(val);
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
          var fields = converter.Index(jsonDocument, Field.Store.YES); // This might be No to save space..

          var luceneDoc = new Document();
          var documentIdField = new Field(Constants.DocumentIdFieldName, jsonDocument.DocumentId, Field.Store.YES,
                                          Field.Index.ANALYZED_NO_NORMS);
          luceneDoc.Add(documentIdField);

          var tempJsonDocument = new JsonDocumentDto
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