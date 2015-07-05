namespace Barista.TuesPechkin
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;

    public sealed class ThreadSafeConverter : StandardConverter
    {
        public ThreadSafeConverter(IToolset toolset)
            : base(toolset)
        {
            toolset.Unloaded += (sender, args) =>
            {
                new Thread(StopThread).Start();
            };

            var nestingToolset = toolset as NestingToolset;
            if (nestingToolset != null)
            {
                nestingToolset.BeforeUnload += (sender, args) =>
                {
                    Invoke(sender as ActionShim);
                };
            }
        }

        public override byte[] Convert(IDocument document)
        {
            return Invoke(() => base.Convert(document));
        }

        public TResult Invoke<TResult>(FuncShim<TResult> @delegate)
        {
            StartThread();

            // create the task
            var task = new Task<TResult>(@delegate);

            // we don't want the task to be completed before we start waiting for that, so the outer lock
            lock (task)
            {
                lock (m_queueLock)
                {
                    m_taskQueue.Add(task);

                    Monitor.Pulse(m_queueLock);
                }

                // until this point, evaluation could not start
                Monitor.Wait(task);

                if (task.Exception != null)
                {
                    throw task.Exception;
                }

                // and when we're done waiting, we know that the result was already set
                return task.Result;
            }
        }
        
        public void Invoke(ActionShim @delegate)
        {
            StartThread();

            // create the task
            var task = new Task(@delegate);

            // we don't want the task to be completed before we start waiting for that, so the outer lock
            lock (task)
            {
                lock (m_queueLock)
                {
                    m_taskQueue.Add(task);

                    Monitor.Pulse(m_queueLock);
                }

                // until this point, evaluation could not start
                Monitor.Wait(task);

                if (task.Exception != null)
                {
                    throw task.Exception;
                }
            }
        }

        private Thread m_innerThread;

        private readonly object m_queueLock = new object();

        private readonly object m_startLock = new object();

        private bool m_stopRequested;

        private readonly List<Task> m_taskQueue = new List<Task>();

        private void StartThread()
        {
            lock (m_startLock)
            {
                if (m_innerThread == null)
                {
                    m_innerThread = new Thread(Run)
                    {
                        IsBackground = true
                    };

                    m_stopRequested = false;

                    m_innerThread.Start();
                }
            }
        }

        private void StopThread()
        {
            lock (m_startLock)
            {
                if (m_innerThread != null)
                {
                    m_stopRequested = true;

                    while (m_innerThread.ThreadState != ThreadState.Stopped) { }

                    m_innerThread = null;
                }
            }
        }

        private void Run()
        {
            //using (WindowsIdentity.Impersonate(IntPtr.Zero))
            //{
            //    Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            //}

            while (!m_stopRequested)
            {
                Task task;

                lock (m_queueLock)
                {
                    if (m_taskQueue.Count > 0)
                    {
                        task = m_taskQueue[0];
                        m_taskQueue.RemoveAt(0);
                    }
                    else
                    {
                        Monitor.Wait(m_queueLock, 100);
                        continue;
                    }
                }

                // if there's a task, process it asynchronously
                lock (task)
                {
                    try
                    {
                        task.Action.DynamicInvoke();
                    }
                    catch (TargetInvocationException e)
                    {
                        Tracer.Critical(string.Format("Exception in SynchronizedDispatcherThread \"{0}\"", Thread.CurrentThread.Name), e);
                        task.Exception = e.InnerException;
                    }

                    // notify waiting thread about completeion
                    Monitor.Pulse(task);
                }
            }
        }

        private class Task
        {
            public Task(ActionShim action)
            {
                Action = action;
            }

            public virtual ActionShim Action { get; protected set; }

            public Exception Exception { get; set; }
        }

        private class Task<TResult> : Task
        {
            public Task(FuncShim<TResult> @delegate)
                : base(null)
            {
                Delegate = @delegate;
                Action = () => Result = Delegate();
            }

            // task code
            private FuncShim<TResult> Delegate
            {
                get;
                set;
            }

            // result, filled out after it's executed
            public TResult Result
            {
                get;
                private set;
            }
        }
    }
}