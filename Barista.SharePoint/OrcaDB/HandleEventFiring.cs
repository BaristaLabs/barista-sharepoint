namespace Barista.SharePoint.OrcaDB
{
  using Microsoft.SharePoint;

  public class HandleEventFiring : SPItemEventReceiver
  {
    public HandleEventFiring()
    {
    }

    public void CustomDisableEventFiring()
    {
      this.EventFiringEnabled = false;
    }

    public void CustomEnableEventFiring()
    {
      this.EventFiringEnabled = true;
    }
  }
}
