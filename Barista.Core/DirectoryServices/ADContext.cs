namespace Barista.DirectoryServices
{
  using System;
  using System.DirectoryServices;

  public sealed class ADContext : DirectoryContext
  {
    public ADContext(DirectoryEntry searchRoot)
      : base(searchRoot)
    {
    }

    [DirectorySearchOptions(SearchScope.Subtree)]
    public DirectorySource<ADUser> Users { get; set; }

    [DirectorySearchOptions(SearchScope.Subtree)]
    public DirectorySource<ADGroup> Groups { get; set; }
  }

}
