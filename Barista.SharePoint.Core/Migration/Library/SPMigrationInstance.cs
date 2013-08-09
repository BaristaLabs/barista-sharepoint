namespace Barista.SharePoint.Migration.Library
{
  using Barista.Jurassic.Library;
  using Barista.SharePoint.Library;
  using Microsoft.SharePoint.Administration;

  public class SPMigrationInstance : ObjectInstance
  {
    private readonly SPContextInstance m_context;
    private readonly SPFarmInstance m_farm;
    private readonly SPServerInstance m_server;
    private readonly SPSecureStoreInstance m_secureStore;

    public SPMigrationInstance(ObjectInstance prototype, SPBaristaContext context, SPFarm farmContext, SPServer serverContext)
      : base(prototype)
    {
      m_context = new SPContextInstance(this.Engine, context);
      m_farm = new SPFarmInstance(this.Engine.Object.InstancePrototype, farmContext);
      m_server = new SPServerInstance(this.Engine.Object.InstancePrototype, serverContext);
      m_secureStore = new SPSecureStoreInstance(this.Engine.Object.InstancePrototype);
      this.PopulateFields();
      this.PopulateFunctions();
    }

    //public void ExportListByName(

  }
}
