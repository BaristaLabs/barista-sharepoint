namespace Barista.Jurassic.Compiler
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  /// <summary>
  /// Represents a generated method and it's dependencies.  For internal use only.
  /// </summary>
  public class GeneratedMethod
  {
    /// <summary>
    /// Creates a new GeneratedMethod instance.
    /// </summary>
    /// <param name="delegateToMethod"> A delegate that refers to the generated method. </param>
    /// <param name="dependencies"> A list of dependent generated methods.  Can be <c>null</c>. </param>
    public GeneratedMethod(Delegate delegateToMethod, IList<GeneratedMethod> dependencies)
    {
      if (delegateToMethod == null)
        throw new ArgumentNullException("delegateToMethod");
      this.GeneratedDelegate = delegateToMethod;
      this.Dependencies = dependencies;
    }

    /// <summary>
    /// Gets a delegate which refers to the generated method.
    /// </summary>
    public Delegate GeneratedDelegate
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets a list of dependent generated methods.
    /// </summary>
    public IList<GeneratedMethod> Dependencies
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets or sets the disassembled IL code for the method.
    /// </summary>
    public string DisassembledIL
    {
      get;
      set;
    }

    private static Dictionary<long, WeakReference> s_generatedMethodCache;
    private static readonly object CacheLock = new object();
    private static long s_generatedMethodId;
    private const int CompactGeneratedCacheCount = 100;

    /// <summary>
    /// Retrieves the code for a generated method, given the ID.  For internal use only.
    /// </summary>
    /// <param name="id"> The ID of the generated method. </param>
    /// <returns> A <c>GeneratedMethodInfo</c> instance. </returns>
    public static GeneratedMethod Load(long id)
    {
      lock (CacheLock)
      {
        if (s_generatedMethodCache == null)
          throw new InvalidOperationException("Internal error: no generated method cache available.");
        WeakReference generatedMethodReference;
        if (s_generatedMethodCache.TryGetValue(id, out generatedMethodReference) == false)
          throw new InvalidOperationException(string.Format("Internal error: generated method {0} was garbage collected.", id));
        var generatedMethod = (GeneratedMethod)generatedMethodReference.Target;
        if (generatedMethod == null)
          throw new InvalidOperationException(string.Format("Internal error: generated method {0} was garbage collected.", id));
        return generatedMethod;
      }
    }

    /// <summary>
    /// Saves the given generated method and returns an ID.  For internal use only.
    /// </summary>
    /// <param name="generatedMethod"> The generated method to save. </param>
    /// <returns> The ID that was associated with the generated method. </returns>
    public static long Save(GeneratedMethod generatedMethod)
    {
      if (generatedMethod == null)
        throw new ArgumentNullException("generatedMethod");
      lock (CacheLock)
      {
        // Create a cache (if it hasn't already been created).
        if (s_generatedMethodCache == null)
          s_generatedMethodCache = new Dictionary<long, WeakReference>();

        // Create a weak reference to the generated method and add it to the cache.
        long id = s_generatedMethodId;
        var weakReference = new WeakReference(generatedMethod);
        s_generatedMethodCache.Add(id, weakReference);

        // Increment the ID for next time.
        s_generatedMethodId++;

        // Every X calls to this method, compact the cache by removing any weak references that
        // point to objects that have been collected.
        if (s_generatedMethodId % CompactGeneratedCacheCount == 0)
        {
          // Remove any weak references that have expired.
          var expiredIDs = s_generatedMethodCache
            .Where(pair => pair.Value.Target == null)
            .Select(pair => pair.Key)
            .ToList();

          foreach (var expiredId in expiredIDs)
            s_generatedMethodCache.Remove(expiredId);
        }

        // Return the ID that was allocated.
        return id;
      }
    }
  }
}
