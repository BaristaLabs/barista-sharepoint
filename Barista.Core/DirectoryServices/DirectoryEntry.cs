namespace Barista.DirectoryServices
{
  using System.ComponentModel;
  using System.DirectoryServices;
  using System.Runtime.Serialization;

  /// <summary>
  /// Represents an entity in Directory Services.
  /// </summary>
  [DataContract(Namespace = Constants.ServiceNamespace)]
  [KnownType(typeof(ADUser))]
  [KnownType(typeof(ADGroup))]
  public class DirectoryEntity : INotifyPropertyChanged
  {
    /// <summary>
    /// Wrapped DirectoryEntry.
    /// </summary>
    private DirectoryEntry directoryEntry;

    /// <summary>
    /// Gets/sets the underlying DirectoryEntry.
    /// </summary>
    protected internal DirectoryEntry DirectoryEntry
    {
      get { return directoryEntry; }
      set { directoryEntry = value; }
    }

    /// <summary>
    /// Event raised when a property on a subclass (= strongly typed entity) has been changed.
    /// This is used for change tracking.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for the specified entity property.
    /// </summary>
    /// <param name="propertyName">Entity property that has been changed.</param>
    protected void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}