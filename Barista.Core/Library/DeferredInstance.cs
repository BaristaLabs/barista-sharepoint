namespace Barista.Library
{
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
      var state = new DeferredState()
      {
        This = this.Engine.Evaluate("this;"),
        Parameters = parameters,
      };

      var task = new Task<object>( new Func<object, object>( s => {
        var localState = s as DeferredState;
        return function.Call(localState.This, localState.Parameters);
      }), (object)state);

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
    private Task<object> m_task;
    private List<Task<bool>> m_continuations = new List<Task<bool>>();
    private SemaphoreSlim m_scriptEngineSemaphore = new SemaphoreSlim(1);

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
    public void Always(object alwaysCallbacks)
    {
      AddFunctionToCollection(TaskContinuationOptions.None, alwaysCallbacks);
    }

    [JSFunction(Name = "done")]
    public void Done(object doneCallbacks)
    {
      AddFunctionToCollection(TaskContinuationOptions.OnlyOnRanToCompletion, doneCallbacks);
    }

    [JSFunction(Name = "fail")]
    public void Fail(object failCallbacks)
    {
      AddFunctionToCollection(TaskContinuationOptions.OnlyOnFaulted, failCallbacks);
    }

    [JSFunction(Name = "then")]
    public void Then(object doneCallbacks, object failCallbacks)
    {
      Done(doneCallbacks);
      Fail(failCallbacks);
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
            this.Continuations.Add(this.Task.ContinueWith((result) =>
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

        this.Continuations.Add(this.Task.ContinueWith((result) =>
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
      Task<bool>.WaitAll(this.Continuations.ToArray());
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

        tasks.AddRange((deferreds as DeferredInstance).Continuations);
      }
      
      try
      {
        if (timeout == null || timeout is Null || timeout is Undefined)
          Task<bool>.WaitAll(tasks.ToArray());
        else
        {
          if (timeout.GetType() == typeof(int))
            Task<bool>.WaitAll(tasks.ToArray(), (int)timeout);
        }
      }
      catch(AggregateException ex)
      {
        List<Exception> rethrow = new List<Exception>();

        foreach(var innerException in ex.InnerExceptions)
        {
          //Ignore TaskCancelledExceptions
          if (innerException is TaskCanceledException)
            continue;

          rethrow.Add(innerException);
        }
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
