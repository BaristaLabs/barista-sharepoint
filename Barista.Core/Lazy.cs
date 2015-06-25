namespace Barista.DocumentStore
{
  using System;

  public sealed class Lazy<T>
  {
    private readonly Func<T> m_createValue;
    private bool m_isValueCreated;
    private readonly object m_padlock;
    private T m_value;

    public bool IsValueCreated
    {
      get
      {
        bool flag;
        lock (this.m_padlock)
        {
          flag = this.m_isValueCreated;
        }
        return flag;
      }
    }

    public T Value
    {
      get
      {
        if (!this.m_isValueCreated)
        {
          lock (this.m_padlock)
          {
            if (!this.m_isValueCreated)
            {
              this.m_value = this.m_createValue();
              this.m_isValueCreated = true;
            }
          }
        }
        return this.m_value;
      }
    }

    public Lazy(Func<T> createValue)
    {
      this.m_padlock = new object();
      if (createValue != null)
      {
        this.m_createValue = createValue;
      }
      else
      {
        throw new ArgumentNullException("createValue");
      }
    }

    public override string ToString()
    {
      T @value = this.Value;
      return @value.ToString();
    }
  }

}
