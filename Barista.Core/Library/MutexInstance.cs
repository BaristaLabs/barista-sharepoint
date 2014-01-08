namespace Barista.Library
{
  using System.Threading;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class MutexInstance : ObjectInstance
  {
    private readonly Mutex m_mutex;

    public MutexInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public MutexInstance(ObjectInstance prototype, Mutex mutex)
      : this(prototype)
    {
      if (mutex == null)
        throw new ArgumentNullException("mutex");

      m_mutex = mutex;
    }

    public Mutex Mutex
    {
      get { return m_mutex; }
    }

    [JSFunction(Name = "waitOne")]
    public bool WaitOne(object timeout)
    {
      if (timeout == Undefined.Value || timeout == Null.Value || timeout == null)
        return m_mutex.WaitOne();
      return m_mutex.WaitOne(TypeConverter.ToInteger(timeout));
    }

    [JSFunction(Name = "releaseMutex")]
    public void ReleaseMutex()
    {
      m_mutex.ReleaseMutex();
    }
  }
}
