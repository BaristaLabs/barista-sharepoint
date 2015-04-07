namespace Barista.V8.Net
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

#if !(V1_1 || V2 || V3 || V3_5)
using System.Dynamic;
#endif
    
    // ========================================================================================================================
    // The worker section implements a bridge GC system, which marks objects weak when 'V8NativeObject' instances no longer have any references.  Such objects
    // are called "weak" objects, and the worker calls the native side to also mark the native object as weak.  Once the V8 GC calls back, the managed object
    // will then complete it's finalization process.

    /// <summary>
    /// Applied to V8 related objects to take control of the finalization process from the GC so it can be better coordinated with the native V8 engine.
    /// </summary>
    internal interface IFinalizable
    {
        void DoFinalize(); // (proceed to finalize the object)
        bool CanFinalize { get; set; } // (if this is true, the GC, when triggered again, can finally collect the instance)
    }

    public partial class V8Engine
    {
        // --------------------------------------------------------------------------------------------------------------------

        internal Thread WorkerThreadInternal;

        /// <summary>
        /// When 'V8NativeObject' objects are no longer in use, they are registered here for quick reference so the worker thread can dispose of them.
        /// </summary>
        internal readonly List<int> WeakObjectsInternal = new List<int>(100);
        private int m_weakObjectsInternalIndex = -1;

        internal readonly List<IFinalizable> ObjectsToFinalizeInternal = new List<IFinalizable>(100);
        private int m_objectsToFinalizeIndex = -1;

        // --------------------------------------------------------------------------------------------------------------------

        void _Initialize_Worker()
        {
            // (note: must set 'IsBackground=true', else the app will hang on exit)
            WorkerThreadInternal = new Thread(_WorkerLoop) {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            }; 
            WorkerThreadInternal.Start();
        }

        // --------------------------------------------------------------------------------------------------------------------

        private volatile int m_pauseWorker;

        private void _WorkerLoop()
        {
            while (true)
            {
                //??if (GlobalObject.AsInternalHandle._HandleProxy->Disposed > 0)
                //    System.Diagnostics.Debugger.Break();

                if (m_pauseWorker == 1) m_pauseWorker = 2;
                else if (m_pauseWorker == -1) break;
                else
                {
                    var workPending = WeakObjectsInternal.Count > 0 || ObjectsToFinalizeInternal.Count > 0;

                    while (workPending && m_pauseWorker == 0)
                    {
                        workPending = _DoWorkStep();
                        DoIdleNotification(1);
                        Thread.Sleep(0);
                    }
                }
                Thread.Sleep(100);
                DoIdleNotification(100);
            }

            m_pauseWorker = -2;
        }

        /// <summary>
        /// Does one step in the work process (mostly garbage collection for freeing up unused handles).
        /// True is returned if more work is pending, and false otherwise.
        /// </summary>
        bool _DoWorkStep()
        {
            // ... do one weak object ...

            var objId = -1;

            lock (WeakObjectsInternal)
            {
                if (m_weakObjectsInternalIndex < 0)
                    m_weakObjectsInternalIndex = WeakObjectsInternal.Count - 1;

                if (m_weakObjectsInternalIndex >= 0)
                {
                    objId = WeakObjectsInternal[m_weakObjectsInternalIndex];

                    WeakObjectsInternal.RemoveAt(m_weakObjectsInternalIndex);

                    m_weakObjectsInternalIndex--;
                }
            }

            if (objId >= 0)
            {
                V8NativeObject obj;
                using (_ObjectsLocker.ReadLock(Int32.MaxValue))
                {
                    obj = _Objects[objId].Object;
                }
                obj._MakeWeak(); // (don't call this while '_Objects' is locked, because the main thread may be executing script that also may need a lock, but this call may also be blocked by a native V8 mutex)
            }

            // ... and do one object ready to be finalized ...

            IFinalizable objectToFinalize = null;

            lock (ObjectsToFinalizeInternal)
            {
                if (m_objectsToFinalizeIndex < 0)
                    m_objectsToFinalizeIndex = ObjectsToFinalizeInternal.Count - 1;

                if (m_objectsToFinalizeIndex >= 0)
                {
                    objectToFinalize = ObjectsToFinalizeInternal[m_objectsToFinalizeIndex];

                    ObjectsToFinalizeInternal.RemoveAt(m_objectsToFinalizeIndex);

                    m_objectsToFinalizeIndex--;
                }
            }

            if (objectToFinalize != null)
                objectToFinalize.DoFinalize();

            return m_weakObjectsInternalIndex >= 0 || m_objectsToFinalizeIndex >= 0;
        }

        /// <summary>
        /// Pauses the worker thread (usually for debug purposes). (Note: The worker thread manages object GC along with the native V8 GC.)
        /// </summary>
        public void PauseWorker()
        {
            if (WorkerThreadInternal.IsAlive)
            {
                m_pauseWorker = 1;
                while (m_pauseWorker == 1 && WorkerThreadInternal.IsAlive) { }
            }
        }

        /// <summary>
        /// Terminates the worker thread, without a 3 second timeout to be sure.
        /// This is called when the engine is shutting down. (Note: The worker thread manages object GC along with the native V8 GC.)
        /// </summary>
        internal void TerminateWorkerInternal()
        {
            if (!WorkerThreadInternal.IsAlive)
                return;

            m_pauseWorker = -1;
            var timeoutCountdown = 3000;
            while (m_pauseWorker == -1 && WorkerThreadInternal.IsAlive)
                if (timeoutCountdown-- > 0)
                    Thread.Sleep(1);
                else
                {
                    WorkerThreadInternal.Abort();
                    break;
                }
        }

        /// <summary>
        /// Unpauses the worker thread (see <see cref="PauseWorker"/>).
        /// </summary>
        public void ResumeWorker()
        {
            m_pauseWorker = 0;
        }

        // --------------------------------------------------------------------------------------------------------------------
    }

    // ========================================================================================================================
}
