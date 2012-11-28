namespace Barista.SharePoint
{

  using Microsoft.SharePoint;

  public class HandleEventFiring : SPItemEventReceiver
  {
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
