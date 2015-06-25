namespace Barista.Social.Imports.Budgie
{
  using System;
  using System.Text;
  using System.Security.Cryptography;

  public class DesktopPlatformAdaptor : IPlatformAdaptor
  {
    public string ComputeSha1Hash(string key, string buffer)
    {
      using (var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
      {
        return Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(buffer)));
      }
    }
  }
}
