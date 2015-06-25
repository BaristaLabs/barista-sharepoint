namespace Barista.Search
{
  using System;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using Barista.Logging;
  using Lucene.Net.Search;
  using global::Barista.Newtonsoft.Json.Linq;

  public class IndexSearcherHolder
  {
    private static readonly ILog Log = LogManager.GetCurrentClassLogger();

    private volatile IndexSearcherHoldingState m_current;

    public ManualResetEvent SetIndexSearcher(IndexSearcher searcher, bool wait)
    {
      var old = m_current;
      m_current = new IndexSearcherHoldingState(searcher);

      if (old == null)
        return null;

      Interlocked.Increment(ref old.Usage);
      using (old)
      {
        if (wait)
          return old.MarkForDisposalWithWait();
        old.MarkForDisposal();
        return null;
      }
    }

    public IDisposable GetSearcher(out IndexSearcher searcher)
    {
      var indexSearcherHoldingState = GetCurrentStateHolder();
      try
      {
        searcher = indexSearcherHoldingState.IndexSearcher;
        return indexSearcherHoldingState;
      }
      catch (Exception e)
      {
        Log.ErrorException("Failed to get the index searcher.", e);
        indexSearcherHoldingState.Dispose();
        throw;
      }
    }

    public IDisposable GetSearcherAndTermDocs(out IndexSearcher searcher, out JObject[] termDocs)
    {
      var indexSearcherHoldingState = GetCurrentStateHolder();
      try
      {
        searcher = indexSearcherHoldingState.IndexSearcher;
        termDocs = indexSearcherHoldingState.GetOrCreateTerms();
        return indexSearcherHoldingState;
      }
      catch (Exception)
      {
        indexSearcherHoldingState.Dispose();
        throw;
      }
    }

    private IndexSearcherHoldingState GetCurrentStateHolder()
    {
      while (true)
      {
        var state = m_current;
        Interlocked.Increment(ref state.Usage);
        if (state.ShouldDispose)
        {
          state.Dispose();
          continue;
        }

        return state;
      }
    }


    private class IndexSearcherHoldingState : IDisposable
    {
      public readonly IndexSearcher IndexSearcher;

      public volatile bool ShouldDispose;
      public int Usage;
      private JObject[] m_readEntriesFromIndex;
      private readonly Lazy<ManualResetEvent> m_disposed = new Lazy<ManualResetEvent>(() => new ManualResetEvent(false));

      public IndexSearcherHoldingState(IndexSearcher indexSearcher)
      {
        IndexSearcher = indexSearcher;
      }

      public void MarkForDisposal()
      {
        ShouldDispose = true;
      }

      public ManualResetEvent MarkForDisposalWithWait()
      {
        var x = m_disposed.Value;//  first create the value
        ShouldDispose = true;
        return x;
      }

      public void Dispose()
      {
        if (Interlocked.Decrement(ref Usage) > 0)
          return;
        if (ShouldDispose == false)
          return;
        DisposeRudely();
      }

      private void DisposeRudely()
      {
        if (IndexSearcher != null)
        {
          using (IndexSearcher)
          using (IndexSearcher.IndexReader) { }
        }
        if (m_disposed.IsValueCreated)
          m_disposed.Value.Set();
      }

      [MethodImpl(MethodImplOptions.Synchronized)]
      public JObject[] GetOrCreateTerms()
      {
        if (m_readEntriesFromIndex != null)
          return m_readEntriesFromIndex;

        var indexReader = IndexSearcher.IndexReader;
        m_readEntriesFromIndex = IndexedTerms.ReadAllEntriesFromIndex(indexReader);
        return m_readEntriesFromIndex;
      }
    }
  }
}
