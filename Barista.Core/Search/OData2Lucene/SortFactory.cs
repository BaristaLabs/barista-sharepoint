namespace Barista.Search.OData2Lucene
{
  using System;
  using System.Linq;
  using Barista.Extensions;
  using Lucene.Net.Search;

  public class SortFactory : ISortFactory
  {
    public Sort Create(string sort)
    {
      if (sort.IsNullOrWhiteSpace())
        return new Sort();

      var sortTokens = sort.Split(',');
      var sortFields = sortTokens.Select(sortToken =>
        {
          var sortOption = sortToken.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

          var reverse = false;
          var reverseToken = sortOption.ElementAtOrDefault(1);
          if (reverseToken != null)
            reverse = reverseToken.ToLowerInvariant() != "desc";

          var type = SortField.STRING;
          var typeToken = sortOption.ElementAtOrDefault(2);
          if (typeToken != null)
          {
            int typeTokenValue;
            if (int.TryParse(typeToken, out typeTokenValue))
              type = typeTokenValue;
          }

          //Special order bys...
          var fieldName = sortOption.First();
          switch (fieldName)
          {
            case "id":
              return new SortField(Constants.DocumentIdFieldName, SortField.INT, reverse);
            default:
              return new SortField(fieldName, type, reverse);
          }
        });

      return new Sort(sortFields.ToArray());
    }
  }
}
