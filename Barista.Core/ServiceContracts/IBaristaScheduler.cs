namespace Barista.ServiceContracts
{
  public interface IBaristaScheduler
  {
    void ScheduleTask(string schedule, string script);
  }
}
