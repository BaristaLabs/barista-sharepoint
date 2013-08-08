namespace Barista.SharePoint.Migration.Library
{
  using Barista.Jurassic.Library;

  public class SPMigrationInstance : ObjectInstance
  {
    public SPMigrationInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }
  }
}
