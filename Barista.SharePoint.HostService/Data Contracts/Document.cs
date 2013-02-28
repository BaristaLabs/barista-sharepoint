namespace Barista.SharePoint.HostService
{
  using System;
  using System.Collections.Generic;
  using System.Runtime.Serialization;
  using Lucene.Net.Documents;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class Document
  {
    [DataMember]
    public ICollection<FieldBase> Fields
    {
      get;
      set;
    }

    public static Lucene.Net.Documents.Document ConvertToLuceneDocument (Document document)
    {
      //Convert WCF document to Lucene document
      var luceneDocument = new Lucene.Net.Documents.Document();

      foreach (var field in document.Fields)
      {
        Lucene.Net.Documents.Field.Index indexType;
        switch (field.Index)
        {
          case FieldIndexType.NotIndexed:
            indexType = Field.Index.NO;
            break;
          case FieldIndexType.Analyzed:
            indexType = Field.Index.ANALYZED;
            break;
          case FieldIndexType.AnalyzedNoNorms:
            indexType = Field.Index.ANALYZED_NO_NORMS;
            break;
          case FieldIndexType.NotAnalyzed:
            indexType = Field.Index.NOT_ANALYZED;
            break;
          case FieldIndexType.NotAnalyzedNoNorms:
            indexType = Field.Index.NOT_ANALYZED_NO_NORMS;
            break;
          default:
            throw new ArgumentOutOfRangeException("Unknown or invalid field index type: " + field.Index);
        }

        Lucene.Net.Documents.Field.Store storeType;
        switch (field.Store)
        {
          case FieldStorageType.Stored:
            storeType = Field.Store.YES;
            break;
          case FieldStorageType.NotStored:
            storeType = Field.Store.NO;
            break;
          default:
            throw new ArgumentOutOfRangeException("Unknown or invalid field store type: " + field.Store);
        }

        Lucene.Net.Documents.Field.TermVector termVectorType;
        switch (field.TermVector)
        {
          case FieldTermVectorType.Yes:
            termVectorType = Field.TermVector.YES;
            break;
          case FieldTermVectorType.WithOffsets:
            termVectorType = Field.TermVector.WITH_OFFSETS;
            break;
          case FieldTermVectorType.WithPositions:
            termVectorType = Field.TermVector.WITH_POSITIONS;
            break;
          case FieldTermVectorType.WithPositionsOffsets:
            termVectorType = Field.TermVector.WITH_POSITIONS_OFFSETS;
            break;
          default:
            throw new ArgumentOutOfRangeException("Unknown or invalid field term vector type: " + field.TermVector);
        }

        IFieldable luceneField;

        if (field is StringField)
        {
          var stringField = field as StringField;

          luceneField = new Lucene.Net.Documents.Field(stringField.Name, true, stringField.Value,
                                                                 storeType, indexType, termVectorType);
        }
        else if (field is DateField)
        {
          var dateField = field as DateField;

          var dateString = DateTools.DateToString(dateField.Value, DateTools.Resolution.MILLISECOND);
          luceneField = new Field(dateField.Name, dateString, storeType, Field.Index.NOT_ANALYZED, termVectorType);
        }
        else if (field is NumericField)
        {
          var numericField = field as NumericField;

          luceneField = new Lucene.Net.Documents.NumericField(numericField.Name, numericField.PrecisionStep,
                                                                         storeType,
                                                                         field.Index != FieldIndexType.NotIndexed);

        }
        else
        {
          throw new NotImplementedException();
        }

        if (field.Boost.HasValue)
          luceneField.Boost = field.Boost.Value;

        luceneDocument.Add(luceneField);
      }

      return luceneDocument;
    }
  }
}
