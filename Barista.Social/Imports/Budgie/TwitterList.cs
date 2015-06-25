namespace Barista.Social.Imports.Budgie
{
  public class TwitterList
  {
    internal TwitterList()
    {
    }

    public long Id { get; internal set; }
    public string Name { get; internal set; }
    public string Path { get; internal set; }
    public int SubscriberCount { get; internal set; }
    public int MemberCount { get; internal set; }
    public string FullName { get; internal set; }
    public string Description { get; internal set; }
    public bool IsFollowing { get; internal set; }
    public TwitterUser User { get; internal set; }
  }
}
