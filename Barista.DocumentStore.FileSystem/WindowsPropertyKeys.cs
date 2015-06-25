namespace Barista.DocumentStore.FileSystem
{
  using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

  public static class WindowsPropertyKeys
  {
    #region System
    public static PropertyKey Title = new PropertyKey("F29F85E0-4FF9-1068-AB91-08002B27B3D9", 2);
    public static PropertyKey Type = new PropertyKey("28636AA6-953D-11D2-B5D6-00C04FD918D0", 11);
    public static PropertyKey Description = new PropertyKey("0CEF7D53-FA64-11D1-A203-0000F81FEDEE", 3);
    #endregion

    #region System.Document
    public static PropertyKey DocumentId = new PropertyKey("E08805C8-E395-40DF-80D2-54F0D6C43154", 100);
    #endregion

    #region System.PropList
    public static PropertyKey FullDetails = new PropertyKey("C9944A21-A406-48FE-8225-AEC7E24C211B", 2);
    #endregion
  }
}
