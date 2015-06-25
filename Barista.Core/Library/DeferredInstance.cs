namespace Barista.Library
{
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;

  [Serializable]
  public class DeferredConstructor : ClrFunction
  {
    public DeferredConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Deferred", new DeferredInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public DeferredInstance Construct(FunctionInstance function, params object[] parameters)
    {
      var state = new DeferredState
        {
        This = this.Engine.Evaluate("this;"),
        Parameters = parameters,
      };

      var task = new Task<object>( s => {
                                          var localState = s as DeferredState;
                                          return localState == null
                                            ? null
                                            : function.Call(localState.This, localState.Parameters);
      }, state);

      task.Start();

      return new DeferredInstance(this.InstancePrototype, task);
    }

    public DeferredInstance Construct(Task<object> task)
    {
      if (task == null)
        throw new ArgumentNullException("task");

      return new DeferredInstance(this.InstancePrototype, task);
    }
  }

  [Serializable]
  public class DeferredInstance : ObjectInstance
  {
    private readonly Task<object> m_task;
    private readonly List<Task<bool>> m_continuations = new List<Task<bool>>();
    private readonly SemaphoreSlim m_scriptEngineSemaphore = new SemaphoreSlim(1);

    public DeferredInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public DeferredInstance(ObjectInstance prototype, Task<object> task)
      : this(prototype)
    {
      this.m_task = task;

      //m_continuation = m_task.ContinueWith( (t) => {
      //  m_scriptEngineSemaphore.Wait();

      //  if (t.IsCompleted)
      //  {
      //    foreach (var function in m_doneFunctions)
      //    {
      //      function.Function.Call(function.This, t.Result);
      //    }
      //  }
      //  else if (t.IsFaulted)
      //  {
      //    foreach (var function in m_failFunctions)
      //    {
      //      function.Function.Call(function.This);
      //    }
      //  }

      //  foreach (var function in m_alwaysFunctions)
      //  {
      //    function.Function.Call(function.This, t.Result);
      //  }

      //  m_scriptEngineSemaphore.Release();
      //  return true;
      //});
    }

    internal Task<object> Task
    {
      get { return m_task; }
    }

    internal List<Task<bool>> Continuations
    {
      get { return m_continuations; }
    }

    #region Properties
    [JSProperty(Name = "status")]
    public string Status
    {
      get { return m_task.Status.ToString(); }
    }
    #endregion

    #region Functions
    [JSFunction(Name = "always")]
    public DeferredInstance Always(object alwaysCallbacks)
    {
      AddFunctionToCollection(TaskContinuationOptions.None, alwaysCallbacks);

      return this;
    }

    [JSFunction(Name = "done")]
    public DeferredInstance Done(object doneCallbacks)
    {
      AddFunctionToCollection(TaskContinuationOptions.OnlyOnRanToCompletion, doneCallbacks);

      return this;
    }

    [JSFunction(Name = "fail")]
    public DeferredInstance Fail(object failCallbacks)
    {
      AddFunctionToCollection(TaskContinuationOptions.OnlyOnFaulted, failCallbacks);

      return this;
    }

    [JSFunction(Name = "then")]
    public DeferredInstance Then(object doneCallbacks, object failCallbacks)
    {
      Done(doneCallbacks);
      Fail(failCallbacks);

      return this;
    }

    private void AddFunctionToCollection(TaskContinuationOptions options, object callback)
    {
      if (callback is ArrayInstance)
      {
        var array = callback as ArrayInstance;
        var that = this.Engine.Evaluate("this;");

        for (int i = 0; i < array.Length; i++)
        {
          var item = array[i];
          if (item is FunctionInstance)
          {
            var func = item as FunctionInstance;
            this.Continuations.Add(this.Task.ContinueWith(result =>
            {
              m_scriptEngineSemaphore.Wait();
              func.Call(that, result.Result);
              m_scriptEngineSemaphore.Release();
              return true;
            }, options));
            //deferredCollection.Add(new DeferredFunction()
            //{
            //  This = that,
            //  Function = item as FunctionInstance,
            //});
          }
        }
      }
      else if (callback is FunctionInstance)
      {
        var that = this.Engine.Evaluate("this;");
        var func = callback as FunctionInstance;

        this.Continuations.Add(this.Task.ContinueWith(result =>
        {
          m_scriptEngineSemaphore.Wait();
          func.Call(that, result.Result);
          m_scriptEngineSemaphore.Release();
          return true;
        }, options));

        //deferredCollection.Add(new DeferredFunction()
        //{
        //  This = that,
        //  Function = callback as FunctionInstance,
        //});
      }
    }

    [JSFunction(Name = "wait")]
    public void Wait()
    {
// ReSharper disable CoVariantArrayConversion
      System.Threading.Tasks.Task.WaitAll(this.Continuations.ToArray());
// ReSharper restore CoVariantArrayConversion
    }

    public static void WaitAll(object deferreds, object timeout)
    {
      List<Task<bool>> tasks = new List<Task<bool>>();

      if (deferreds is ArrayInstance)
      {
        var array = deferreds as ArrayInstance;

        for (int i = 0; i < array.Length; i++)
        {
          var item = array[i];
          if (item is DeferredInstance)
          {
            var deferredInstance = item as DeferredInstance;

            tasks.AddRange(deferredInstance.Continuations);
          }
        }
      }
      else if (deferreds is DeferredInstance)
      {
        var deferredInstance = deferreds as DeferredInstance;

        tasks.AddRange(deferredInstance.Continuations);
      }
      
      try
      {
        if (timeout == null || timeout is Null || timeout is Undefined)
// ReSharper disable CoVariantArrayConversion
          System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
// ReSharper restore CoVariantArrayConversion
        else
        {
          if (timeout is int)
// ReSharper disable CoVariantArrayConversion
            System.Threading.Tasks.Task.WaitAll(tasks.ToArray(), (int)timeout);
// ReSharper restore CoVariantArrayConversion
        }
      }
      catch(AggregateException ex)
      {
        var rethrow = ex.InnerExceptions.Where(innerException => !(innerException is TaskCanceledException)).ToList();

        if (rethrow.Count > 0)
          throw new AggregateException(rethrow);
      }
    }
    #endregion
  }

  public class DeferredFunction
  {
    public object This
    {
      get;
      set;
    }

    public FunctionInstance Function
    {
      get;
      set;
    }
  }

  public class DeferredState
  {
    public object This
    {
      get;
      set;
    }

    public object[] Parameters
    {
      get;
      set;
    }
  }
}
