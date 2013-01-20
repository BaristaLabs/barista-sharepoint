namespace Barista.SocketBase.Security
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Security.Cryptography.X509Certificates;
  using Barista.SocketBase.Config;

  public static class CertificateManager
  {
    internal static X509Certificate Initialize(ICertificateConfig cerConfig)
    {
      if (!string.IsNullOrEmpty(cerConfig.FilePath))
      {
        //To keep compatible with website hosting

        string filePath = Path.IsPathRooted(cerConfig.FilePath)
                            ? cerConfig.FilePath
                            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cerConfig.FilePath);

        return new X509Certificate2(filePath, cerConfig.Password);
      }

      var storeName = cerConfig.StoreName;
      if (string.IsNullOrEmpty(storeName))
        storeName = "Root";

      var store = new X509Store(storeName);

      store.Open(OpenFlags.ReadOnly);

      var cert = store.Certificates.OfType<X509Certificate2>()
        .FirstOrDefault(c => c.Thumbprint != null && c.Thumbprint.Equals(cerConfig.Thumbprint, StringComparison.OrdinalIgnoreCase));

      store.Close();

      return cert;
    }
  }
}
