namespace Barista.SharePoint
{
  using Microsoft.SharePoint.Administration;
  using System;

  /// <summary>
  ///     Class to handle loading a value from the Common Properties list
  /// </summary>
  public class CommonPropertyUtil
  {
    // private static ConfigurationPropertyCollection _properties = null;

    public static string Load(string name)
    {
      return FarmPropertyBag.Load(name);
    }

    /// <summary>
    /// Load a string from the common properties list. Returns empty string if name does not exist
    /// </summary>
    /// <param name="name">Name of the property to retrieve</param>
    /// <param name="webApp"></param>
    /// <returns>Value corresponding to Name. Empty string if not found</returns>
    public static string Load(string name, SPWebApplication webApp)
    {
      return FarmPropertyBag.Load(name);

      //if (_properties == null)
      //{
      //    ReloadProperties(webApp);
      //}

      //ConfigurationProperty prop = _properties[name];
      //if (prop == null)
      //{
      //    return "";
      //}
      //else
      //{
      //    return prop.DefaultValue.ToString();
      //}
    }

    /// <summary>
    /// Save a name value pair to the common properties list using current sharepoint context
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public static void Save(string name, string value)
    {
      FarmPropertyBag.Save(name, value);
    }

    /// <summary>
    /// Internal function for getting the encryption password from the web application
    /// </summary>
    /// <param name="webApp"></param>
    /// <returns></returns>
    private static string GetEncryptionPassword(SPWebApplication webApp)
    {
      object password = webApp.Properties["PropertyEncryptionPassword"];
      if (password == null)
      {
        throw new Exception("Unable to retrieve encryption password");
      }
      return password.ToString();
    }

    /// <summary>
    /// Internal function to decrypt a string with the web applications encryption password
    /// </summary>
    /// <param name="valueEncrypted"></param>
    /// <param name="webApp"></param>
    /// <returns></returns>
    private static string DecryptString(string valueEncrypted, SPWebApplication webApp)
    {
      string decryptedValue = valueEncrypted;
      try
      {
        decryptedValue = EncryptDecrypt.Decrypt(valueEncrypted, GetEncryptionPassword(webApp));
      }
      catch (Exception)
      {
        // return unencrypted string if we can't decrypt for any reason.
      }

      return decryptedValue;
    }

    /// <summary>
    ///  internal function to encrypt a string with the web applications encryption password
    /// </summary>
    /// <param name="valueDecrypted"></param>
    /// <param name="webApp"></param>
    /// <returns></returns>
    private static string EncryptString(string valueDecrypted, SPWebApplication webApp)
    {

      return EncryptDecrypt.Encrypt(valueDecrypted, GetEncryptionPassword(webApp));
    }

    /// <summary>
    /// Load and decrypt an encrypted string for the given name in the current SharePoint context
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string LoadEncryptedString(string name)
    {
      return LoadEncryptedString(name, BaristaContext.Current.Site.WebApplication);
    }

    /// <summary>
    /// Load and decrypt an encrypted string from the common properties list. Pass in the web applicaton
    /// Note: this is used mainly for testing in the Common Properties Maintenance applicatoin. If you 
    /// have a SharePoint context, use the version that doesn't require a webApp parameter.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="webApp"></param>
    /// <returns></returns>
    public static string LoadEncryptedString(string name, SPWebApplication webApp)
    {
      //return DecryptString(Load(name, webApp), webApp);
      return DecryptString(FarmPropertyBag.Load(name), webApp);
    }

    /// <summary>
    ///  Encrypt and store a string value for the given name. Works within a valid SharePoint context
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public static void StoreEncryptedString(string name, string value)
    {
      StoreEncryptedString(name, value, BaristaContext.Current.Site.WebApplication);
    }

    /// <summary>
    /// Encrypt and store a string into the common properties list for corresponding name, passing in appropriate web application
    /// NOTE: if you have a valid SharePoint context, use the version of this method that does not require the webApp parameter.
    /// This method is for testing with the Common Properties Maintenance application
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="webApp"></param>
    public static void StoreEncryptedString(string name, string value, SPWebApplication webApp)
    {
      //Save(name, EncryptString(value, webApp), webApp);
      FarmPropertyBag.Save(name, EncryptString(value, webApp));
    }
  }
}
