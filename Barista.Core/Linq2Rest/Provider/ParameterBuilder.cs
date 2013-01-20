namespace Barista.Linq2Rest.Provider
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web;
  using Barista.Extensions;

  internal class ParameterBuilder
  {
    private readonly Uri m_serviceBase;

    public ParameterBuilder(Uri serviceBase)
    {
      if (serviceBase == null)
        throw new ArgumentNullException("serviceBase");

      if (serviceBase.Scheme != Uri.UriSchemeHttp && serviceBase.Scheme != Uri.UriSchemeHttps)
        throw new ArgumentOutOfRangeException("serviceBase");

      m_serviceBase = serviceBase;
      OrderByParameter = new List<string>();
    }

    public string FilterParameter
    {
      get;
      set;
    }

    public IList<string> OrderByParameter
    {
      get;
      private set;
    }

    public string SelectParameter
    {
      get;
      set;
    }

    public string SkipParameter
    {
      get;
      set;
    }

    public string TakeParameter
    {
      get;
      set;
    }

    public string ExpandParameter
    {
      get;
      set;
    }

    public Uri GetFullUri()
    {
      var parameters = new List<string>();
      if (!FilterParameter.IsNullOrWhiteSpace())
      {
        parameters.Add(BuildParameter(StringConstants.FilterParameter, HttpUtility.UrlEncode(FilterParameter)));
      }

      if (!SelectParameter.IsNullOrWhiteSpace())
      {
        parameters.Add(BuildParameter(StringConstants.SelectParameter, SelectParameter));
      }

      if (!SkipParameter.IsNullOrWhiteSpace())
      {
        parameters.Add(BuildParameter(StringConstants.SkipParameter, SkipParameter));
      }

      if (!TakeParameter.IsNullOrWhiteSpace())
      {
        parameters.Add(BuildParameter(StringConstants.TopParameter, TakeParameter));
      }

      if (OrderByParameter.Any())
      {
        parameters.Add(BuildParameter(StringConstants.OrderByParameter, OrderByParameter.Join(",")));
      }

      if (!ExpandParameter.IsNullOrWhiteSpace())
      {
        parameters.Add(BuildParameter(StringConstants.ExpandParameter, ExpandParameter));
      }

      var builder = new UriBuilder(m_serviceBase);
      builder.Query = (string.IsNullOrEmpty(builder.Query) ? string.Empty : "&") + parameters.Join("&");

      var resultUri = builder.Uri;
      return resultUri;
    }

    private static string BuildParameter(string name, string value)
    {
      return name + "=" + value;
    }
  }
}