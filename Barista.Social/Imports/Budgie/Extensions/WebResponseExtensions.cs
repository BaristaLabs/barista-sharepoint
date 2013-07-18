namespace Barista.Social.Imports.Budgie.Extensions
{
  using System;
  using System.IO;
  using System.Net;
  using System.Threading.Tasks;
  using Barista.Social.Imports.Budgie.Json;
  using Barista.Newtonsoft.Json;

  internal static class WebResponseExtensions
  {
    public static Task<ITwitterResponse<T>> RespondWith<T>(this Task<WebResponse> task, Func<String, T> resultFactory)
    {
      return task.ContinueWith<ITwitterResponse<T>>(t =>
      {
        var result = t.ToTwitterResponse<T>();
        if (result.StatusCode != System.Net.HttpStatusCode.OK) return result;

        result.Result = resultFactory(result.RawContent);
        return result;
      });
    }

    public static TwitterResponse<T> ToTwitterResponse<T>(this Task<WebResponse> task, T result = default(T))
    {
      if (task.IsFaulted)
      {
        var ex = task.Exception.GetBaseException();

        var message = ex.Message;
        var webException = ex as WebException;
        if (webException != null && webException.Response != null)
        {
          var content = webException.Response.GetContentString();
          if (content != null)
          {
            try
            {
              var error = JsonConvert.DeserializeObject<JsonError>(content);
              if (error != null)
              {
                message = error.error;
              }
            }
            catch
            {
              // the content wasn't valid json, so just report the exception
            }
          }
        }

        return new TwitterResponse<T>
        {
          StatusCode = HttpStatusCode.InternalServerError,
          ErrorMessage = message,
        };
      }

      var response = task.Result as HttpWebResponse;
      var tr = new TwitterResponse<T>
      {
        StatusCode = HttpStatusCode.InternalServerError,
        ErrorMessage = "Invalid or no response from Twitter",
      };
      if (response == null) return tr;

      int limit, remaining;
      if (Int32.TryParse(response.Headers["X-Rate-Limit-Limit"], out limit)
          && Int32.TryParse(response.Headers["X-Rate-Limit-Remaining"], out remaining))
      {
        tr.RateLimit = new TwitterRateLimit
        {
          Limit = limit,
          Remaining = remaining,
        };
      }

      tr.StatusCode = response.StatusCode;
      tr.ErrorMessage = response.StatusDescription;
      tr.RawContent = response.GetContentString();
      tr.Result = result;

      return tr;
    }

    public static string GetContentString(this WebResponse response)
    {
      // not "using" rs as per CA2202
      Stream stream = null;
      try
      {
        stream = response.GetResponseStream();
        using (var reader = new StreamReader(stream))
        {
          stream = null;
          return reader.ReadToEnd();
        }
      }
      finally
      {
        if (stream != null) stream.Dispose();
      }
    }
  }
}
