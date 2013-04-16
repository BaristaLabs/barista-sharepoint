namespace Barista.Search
{
  using System;
  using System.Collections.Generic;
  using Barista.Newtonsoft.Json.Linq;
  using Lucene.Net.Index;
  using Lucene.Net.Util;

  public class IndexedTerms
  {
    public static void ReadEntriesForFields(IndexReader reader, HashSet<string> fieldsToRead, HashSet<int> docIds, Action<Lucene.Net.Index.Term> onTermFound)
    {
      using (var termDocs = reader.TermDocs())
      {
        foreach (var field in fieldsToRead)
        {
          using (var termEnum = reader.Terms(new Lucene.Net.Index.Term(field)))
          {
            do
            {
              if (termEnum.Term == null || field != termEnum.Term.Field)
                break;

              if (LowPrecisionNumber(termEnum.Term))
                continue;

              var totalDocCountIncludedDeletes = termEnum.DocFreq();
              termDocs.Seek(termEnum.Term);
              while (termDocs.Next() && totalDocCountIncludedDeletes > 0)
              {
                totalDocCountIncludedDeletes -= 1;
                if (reader.IsDeleted(termDocs.Doc))
                  continue;
                if (docIds.Contains(termDocs.Doc) == false)
                  continue;
                onTermFound(termEnum.Term);
              }
            } while (termEnum.Next());
          }
        }
      }
    }

    private static bool LowPrecisionNumber(Lucene.Net.Index.Term term)
    {
      if (term.Field.EndsWith("_Range") == false)
        return false;

      if (string.IsNullOrEmpty(term.Text))
        return false;

      return term.Text[0] - NumericUtils.SHIFT_START_INT != 0 &&
             term.Text[0] - NumericUtils.SHIFT_START_LONG != 0;
    }

    public static JObject[] ReadAllEntriesFromIndex(IndexReader reader)
    {
      if (reader.MaxDoc > 128 * 1024)
      {
        throw new InvalidOperationException("Refusing to extract all index entires from an index with " + reader.MaxDoc +
                          " entries, because of the probable time / memory costs associated with that." +
                          Environment.NewLine +
                          "Viewing Index Entries are a debug tool, and should not be used on indexes of this size. You might want to try Luke, instead.");
      }
      var results = new JObject[reader.MaxDoc];
      using (var termDocs = reader.TermDocs())
      using (var termEnum = reader.Terms())
      {
        while (termEnum.Next())
        {
          var term = termEnum.Term;
          if (term == null)
            break;

          var text = term.Text;

          termDocs.Seek(termEnum);
          for (int i = 0; i < termEnum.DocFreq() && termDocs.Next(); i++)
          {
            var result = results[termDocs.Doc];
            if (result == null)
              results[termDocs.Doc] = result = new JObject();
            var propertyName = term.Field;
            if (propertyName.EndsWith("_ConvertToJson") ||
              propertyName.EndsWith("_IsArray"))
              continue;

            JToken value;
            if (result.TryGetValue(propertyName, out value))
            {
              switch (value.Type)
              {
                case JTokenType.Array:
                  ((JArray)result[propertyName]).Add(text);
                  break;
                case JTokenType.String:
                  result[propertyName] = new JArray
                    {
										result[propertyName],
										text
									};
                  break;
                default:
                  throw new ArgumentException("No idea how to handle " + result[propertyName].Type);
              }
            }
            else
            {
              result[propertyName] = text;
            }
          }
        }
      }
      return results;
    }

  }
}
