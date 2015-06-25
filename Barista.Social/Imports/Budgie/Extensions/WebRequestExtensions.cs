namespace Barista.Social.Imports.Budgie.Extensions
{
  using System;
  using System.Net;
  using System.Threading.Tasks;

  internal static class WebRequestExtensions
  {
    internal static Task<WebResponse> GetResponseAsync(this WebRequest request, TimeSpan timeout)
    {
      return Task.Factory.StartNew(() =>
      {
        var t = Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);

        if (!t.Wait(timeout)) throw new TimeoutException();

        return t.Result;
      });
    }
  }
}
