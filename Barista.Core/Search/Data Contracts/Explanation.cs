namespace Barista.Search
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class Explanation
  {
    [DataMember]
    public string Description
    {
      get;
      set;
    }

    [DataMember]
    public IList<Explanation> Details
    {
      get;
      set;
    }

    [DataMember]
    public string ExplanationHtml
    {
      get;
      set;
    }

    [DataMember]
    public bool IsMatch
    {
      get;
      set;
    }

    [DataMember]
    public double Value
    {
      get;
      set;
    }

    public static Explanation ConvertLuceneExplanationToExplanation(Lucene.Net.Search.Explanation lExplanation)
    {
      if (lExplanation == null)
        throw new ArgumentNullException("lExplanation");

      var result = new Explanation
        {
          Description = lExplanation.Description,
          
          ExplanationHtml = lExplanation.ToHtml(),
          IsMatch = lExplanation.IsMatch,
          Value = lExplanation.Value,
        };

      var lDetails = lExplanation
        .GetDetails();

      if (lDetails != null)
      {
        result.Details = lDetails.ToList()
                                 .Select(e => Explanation.ConvertLuceneExplanationToExplanation(e))
                                 .ToList();
      }

      return result;
    }
  }
}
