namespace Barista
{
  using System;
  using System.Collections.Concurrent;
  using System.Threading;

  /// <summary>
  /// The basic interface of smart pool
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface ISmartPool<T>
  {
    /// <summary>
    /// Initializes the specified min pool size.
    /// </summary>
    /// <param name="minPoolSize">The min size of the pool.</param>
    /// <param name="maxPoolSize">The max size of the pool.</param>
    /// <param name="sourceCreator">The source creator.</param>
    /// <returns></returns>
    void Initialize(int minPoolSize, int maxPoolSize, ISmartPoolSourceCreator<T> sourceCreator);

    /// <summary>
    /// Gets the min size of the pool.
    /// </summary>
    /// <value>
    /// The min size of the pool.
    /// </value>
    int MinPoolSize { get; }

    /// <summary>
    /// Gets the max size of the pool.
    /// </summary>
    /// <value>
    /// The max size of the pool.
    /// </value>
    int MaxPoolSize { get; }


    /// <summary>
    /// Gets the avialable items count.
    /// </summary>
    /// <value>
    /// The avialable items count.
    /// </value>
    int AvialableItemsCount { get; }

    /// <summary>
    /// Pushes the specified item into the pool.
    /// </summary>
    /// <param name="item">The item.</param>
    void Push(T item);

    /// <summary>
    /// Tries to get one item from the pool.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    bool TryGet(out T item);
  }

  /// <summary>
  /// ISmartPoolSource
  /// </summary>
  public interface ISmartPoolSource
  {
    /// <summary>
    /// Gets the count.
    /// </summary>
    /// <value>
    /// The count.
    /// </value>
    int Count { get; }
  }

  /// <summary>
  /// SmartPoolSource
  /// </summary>
  public class SmartPoolSource : ISmartPoolSource
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SmartPoolSource" /> class.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="itemsCount">The items count.</param>
    public SmartPoolSource(object source, int itemsCount)
    {
      Source = source;
      Count = itemsCount;
    }

    /// <summary>
    /// Gets the source.
    /// </summary>
    /// <value>
    /// The source.
    /// </value>
    public object Source { get; private set; }

    /// <summary>
    /// Gets the count.
    /// </summary>
    /// <value>
    /// The count.
    /// </value>
    public int Count { get; private set; }
  }



  /// <summary>
  /// ISmartPoolSourceCreator
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface ISmartPoolSourceCreator<T>
  {
    /// <summary>
    /// Creates the specified size.
    /// </summary>
    /// <param name="size">The size.</param>
    /// <param name="poolItems">The pool items.</param>
    /// <returns></returns>
    ISmartPoolSource Create(int size, out T[] poolItems);
  }

  /// <summary>
  /// The smart pool
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SmartPool<T> : ISmartPool<T>
  {
    private ConcurrentStack<T> m_globalStack;

    private ISmartPoolSource[] m_itemsSource;

    private int m_currentSourceCount;

    private ISmartPoolSourceCreator<T> m_sourceCreator;

    private int m_minPoolSize;

    /// <summary>
    /// Gets the size of the min pool.
    /// </summary>
    /// <value>
    /// The size of the min pool.
    /// </value>
    public int MinPoolSize
    {
      get
      {
        return m_minPoolSize;
      }
    }

    private int m_maxPoolSize;

    /// <summary>
    /// Gets the size of the max pool.
    /// </summary>
    /// <value>
    /// The size of the max pool.
    /// </value>
    public int MaxPoolSize
    {
      get
      {
        return m_maxPoolSize;
      }
    }

    /// <summary>
    /// Gets the avialable items count.
    /// </summary>
    /// <value>
    /// The avialable items count.
    /// </value>
    public int AvialableItemsCount
    {
      get
      {
        return m_globalStack.Count;
      }
    }

    /// <summary>
    /// Initializes the specified min and max pool size.
    /// </summary>
    /// <param name="minPoolSize">The min size of the pool.</param>
    /// <param name="maxPoolSize">The max size of the pool.</param>
    /// <param name="sourceCreator">The source creator.</param>
    public void Initialize(int minPoolSize, int maxPoolSize, ISmartPoolSourceCreator<T> sourceCreator)
    {
      m_minPoolSize = minPoolSize;
      m_maxPoolSize = maxPoolSize;
      m_sourceCreator = sourceCreator;
      m_globalStack = new ConcurrentStack<T>();

      var n = 0;

      if (minPoolSize != maxPoolSize)
      {
        var currentValue = minPoolSize;

        while (true)
        {
          n++;

          var thisValue = currentValue * 2;

          if (thisValue >= maxPoolSize)
            break;

          currentValue = thisValue;
        }
      }

      m_itemsSource = new ISmartPoolSource[n + 1];

      T[] items;
      m_itemsSource[0] = sourceCreator.Create(minPoolSize, out items);
      m_currentSourceCount = 1;

      foreach (T t in items)
      {
        m_globalStack.Push(t);
      }
    }

    /// <summary>
    /// Pushes the specified item into the pool.
    /// </summary>
    /// <param name="item">The item.</param>
    public void Push(T item)
    {
      m_globalStack.Push(item);
    }

    /// <summary>
    /// Tries to get one item from the pool.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public bool TryGet(out T item)
    {
      if (m_globalStack.TryPop(out item))
        return true;

      var currentSourceCount = m_currentSourceCount;

      if (currentSourceCount >= m_itemsSource.Length)
        return false;

      if (Interlocked.CompareExchange(ref m_currentSourceCount, currentSourceCount + 1, currentSourceCount) != currentSourceCount)
      {
        var spinWait = new SpinWait();

        while (true)
        {
          spinWait.SpinOnce();

          if (m_globalStack.TryPop(out item))
            return true;

          if (spinWait.Count >= 100)
            return false;
        }
      }

      int totalItemsCount = 0;

      for (var i = 0; i < currentSourceCount; i++)
      {
        totalItemsCount += m_itemsSource[i].Count;
      }

      totalItemsCount = Math.Min(totalItemsCount, m_maxPoolSize - totalItemsCount);

      T[] items;
      m_itemsSource[currentSourceCount] = m_sourceCreator.Create(totalItemsCount, out items);

      foreach (T t in items)
      {
        m_globalStack.Push(t);
      }

      return m_globalStack.TryPop(out item);
    }
  }
}
