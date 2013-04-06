namespace Barista.Search
{
  using Barista.Extensions;
  using System.Collections.Generic;

  public class FieldsToFetch
  {
    private readonly string m_additionalField;
    private readonly HashSet<string> m_fieldsToFetch;
    private readonly AggregationOperation m_aggregationOperation;
    private HashSet<string> m_ensuredFieldNames;
    public bool FetchAllStoredFields { get; set; }

    public FieldsToFetch(ICollection<string> fieldsToFetch, AggregationOperation aggregationOperation, string additionalField)
    {
      this.m_additionalField = additionalField;
      if (fieldsToFetch != null)
      {
        this.m_fieldsToFetch = new HashSet<string>(fieldsToFetch);
        FetchAllStoredFields = this.m_fieldsToFetch.Remove(Constants.AllFields);
      }
      this.m_aggregationOperation = aggregationOperation.RemoveOptionals();

      if (this.m_aggregationOperation != AggregationOperation.None)
        EnsureHasField(this.m_aggregationOperation.ToString());

      IsDistinctQuery = aggregationOperation.HasFlag(AggregationOperation.Distinct) &&
                fieldsToFetch != null && fieldsToFetch.Count > 0;

      IsProjection = fieldsToFetch != null && fieldsToFetch.Count != 0;

      if (IsProjection && IsDistinctQuery == false)
        EnsureHasField(additionalField);
    }

    public bool IsDistinctQuery { get; private set; }

    public bool IsProjection { get; private set; }


    public IEnumerable<string> Fields
    {
      get
      {
        HashSet<string> fieldsWeMustReturn = m_ensuredFieldNames == null
                                              ? new HashSet<string>()
                                              : new HashSet<string>(m_ensuredFieldNames);
        foreach (var fieldToReturn in GetFieldsToReturn())
        {
          fieldsWeMustReturn.Remove(fieldToReturn);
          yield return fieldToReturn;
        }

        foreach (var field in fieldsWeMustReturn)
        {
          yield return field;
        }
      }
    }

    private IEnumerable<string> GetFieldsToReturn()
    {
      if (m_fieldsToFetch == null)
        yield break;
      foreach (var field in m_fieldsToFetch)
      {
        yield return field;
      }
    }


    public FieldsToFetch CloneWith(string[] newFieldsToFetch)
    {
      return new FieldsToFetch(newFieldsToFetch, m_aggregationOperation, m_additionalField);
    }

    public void EnsureHasField(string ensuredFieldName)
    {
      if (m_ensuredFieldNames == null)
        m_ensuredFieldNames = new HashSet<string>();
      m_ensuredFieldNames.Add(ensuredFieldName);
    }

    public bool HasField(string name)
    {
      return m_fieldsToFetch.Contains(name);
    }
  }
}
