namespace Barista.Social.Imports.Budgie
{
  public interface IPlatformAdaptor
  {
    string ComputeSha1Hash(string key, string buffer);
  }

  /*
  internal class StorePlatformAdaptor : IPlatformAdaptor
  {
      public string ComputeSha1Hash(string key, string buffer)
      {
          var algorithm = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha1);
          var keymaterial = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
          var hmacKey = algorithm.CreateKey(keymaterial);

          return CryptographicBuffer.EncodeToBase64String(CryptographicEngine.Sign(
              hmacKey,
              CryptographicBuffer.ConvertStringToBinary(buffer, BinaryStringEncoding.Utf8)));
      }
  }

  internal class WpfPlatformAdaptor : IPlatformAdaptor
  {
      public string ComputeSha1Hash(string key, string buffer)
      {
          using (var hasher = new HMACSHA1(UTF8Encoding.UTF8.GetBytes(key)))
          {
              return Convert.ToBase64String(hasher.ComputeHash(UTF8Encoding.UTF8.GetBytes(buffer)));
          }
      }
  }
  */
}
