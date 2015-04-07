namespace Barista.V8.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Barista.Extensions;


#if V2 || V3 || V3_5
    #else
    using System.Dynamic;
    #endif

    // ========================================================================================================================

    /// <summary>
    /// Represents a JavaScript callback function for a managed class method.
    /// </summary>
    /// <param name="isConstructCall">True only if this function is being called to construct a new object (such as using the "new" operator within JavaScript).
    /// If this is true, the function is expected to create and return a new object (as the constructor for that object).</param>
    /// <param name="args">The arguments supplied for the JavaScript function call.</param>
    public delegate InternalHandle JSFunction(V8Engine engine, bool isConstructCall, InternalHandle _this, params InternalHandle[] args);

    // ========================================================================================================================

    public unsafe class FunctionTemplate : TemplateBase<IV8Function>, IFinalizable
    {
        // --------------------------------------------------------------------------------------------------------------------

        internal NativeFunctionTemplateProxy* NativeFunctionTemplateProxyInternal;

        public string ClassName { get; private set; }

        /// <summary>
        /// Set this to an object that implements a call-back to execute when the function associated with this FunctionTemplate is called within JavaScript.
        /// </summary>
        private readonly Dictionary<Type, int> m_functionsByTypeInternal = new Dictionary<Type, int>();

        /// <summary>
        /// The V8 engine automatically creates two templates with every function template: one for object creation (instances) and one for function object itself (prototype inheritance).
        /// This property returns the ObjectTemplate wrapper associated with the V8 native instance template for creating new objects using the function in this template as the constructor.
        /// </summary>
        public ObjectTemplate InstanceTemplate { get; private set; }

        /// <summary>
        /// The V8 engine automatically creates two templates with every function template: one for object creation (instances) and one for object inheritance (prototypes).
        /// This property returns the ObjectTemplate wrapper associated with the prototype template for the function object in this template.
        /// </summary>
        public ObjectTemplate PrototypeTemplate { get; private set; }

        // --------------------------------------------------------------------------------------------------------------------

        ~FunctionTemplate()
        {
            if (!((IFinalizable)this).CanFinalize)
                lock (_Engine.ObjectsToFinalizeInternal)
                {
                    _Engine.ObjectsToFinalizeInternal.Add(this);
                    GC.ReRegisterForFinalize(this);
                }
        }

        bool IFinalizable.CanFinalize { get; set; }

        void IFinalizable.DoFinalize()
        {
            if (((ITemplateInternal)this)._ReferenceCount == 0
                && _Engine.GetObjects(this).Length == 0
                && _Engine.GetObjects(PrototypeTemplate).Length == 0
                && _Engine.GetObjects(InstanceTemplate).Length == 0)
                Dispose();
        }

        public void Dispose()
        {
            if (NativeFunctionTemplateProxyInternal != null)
            {
                V8NetProxy.DeleteFunctionTemplateProxy(NativeFunctionTemplateProxyInternal); // (delete the corresponding native object as well; WARNING: This is done on the GC thread!)
                NativeFunctionTemplateProxyInternal = null;

                PrototypeTemplate.Parent = null;
                InstanceTemplate.Parent = null;
                PrototypeTemplate = null;
                InstanceTemplate = null;
            }

            ((IFinalizable)this).CanFinalize = true;
        }

        internal void _Initialize(V8Engine v8EngineProxy, string className)
        {
            ClassName = className;

            _Initialize(v8EngineProxy,
                V8NetProxy.CreateFunctionTemplateProxy(
                    v8EngineProxy.NativeV8EngineProxy,
                    ClassName,
                    _SetDelegate<ManagedJSFunctionCallback>(_CallBack)) // (create a corresponding native object)
            );
        }

        internal void _Initialize(V8Engine v8EngineProxy, NativeFunctionTemplateProxy* nativeFunctionTemplateProxy)
        {
            if (v8EngineProxy == null)
                throw new ArgumentNullException("v8EngineProxy");

            if (nativeFunctionTemplateProxy == null)
                throw new ArgumentNullException("nativeFunctionTemplateProxy");

            _Engine = v8EngineProxy;

            NativeFunctionTemplateProxyInternal = nativeFunctionTemplateProxy;

            InstanceTemplate = _Engine.CreateObjectTemplate<ObjectTemplate>(true);
            InstanceTemplate.Parent = this;
            InstanceTemplate._Initialize(_Engine, V8NetProxy.GetFunctionInstanceTemplateProxy(NativeFunctionTemplateProxyInternal), true);

            PrototypeTemplate = _Engine.CreateObjectTemplate<ObjectTemplate>(true);
            PrototypeTemplate.Parent = this;
            PrototypeTemplate._Initialize(_Engine, (NativeObjectTemplateProxy*)V8NetProxy.GetFunctionPrototypeTemplateProxy(NativeFunctionTemplateProxyInternal), true);

            OnInitialized();
        }

        /// <summary>
        /// Called when the object is initialized instance is ready for use.
        /// </summary>
        protected override void OnInitialized()
        {
        }

        // --------------------------------------------------------------------------------------------------------------------

        HandleProxy* _CallBack(Int32 managedObjectId, bool isConstructCall, HandleProxy* _this, HandleProxy** args, Int32 argCount)
        {
            var functions = from f in
                                (from t in m_functionsByTypeInternal.Keys.ToArray() // (need to convert this to an array in case the callbacks modify the dictionary!)
                                 select _Engine._GetObjectWeakReference(m_functionsByTypeInternal[t]))
                            where f != null && f.Object != null && ((V8Function)f.Object).Callback != null
                            select ((V8Function)f.Object).Callback;

            return _CallBack(managedObjectId, isConstructCall, _this, args, argCount, functions.ToArray());
        }

        internal static HandleProxy* _CallBack(Int32 managedObjectId, bool isConstructCall, HandleProxy* _this, HandleProxy** args, Int32 argCount, params JSFunction[] functions)
        {
            // ... get a handle to the native "this" object ...

            using (InternalHandle hThis = new InternalHandle(_this, false))
            {
                V8Engine engine = hThis.Engine;

                // ... wrap the arguments ...

                var internalHandleArgs = new InternalHandle[argCount];
                int i;

                for (i = 0; i < argCount; i++)
                    internalHandleArgs[i]._Set(args[i], false); // (since these will be disposed immediately after, the "first" flag is not required [this also prevents it from getting passed on])

                InternalHandle result = null;

                try
                {
                    // ... call all function types (multiple custom derived function types are allowed, but only one of each type) ...
                    foreach (var callback in functions)
                    {
                        result = callback(engine, isConstructCall, hThis, internalHandleArgs);

                        if (!result.IsEmpty) break;
                    }
                }
                finally
                {
                    for (i = 0; i < internalHandleArgs.Length; i++)
                        if (internalHandleArgs[i] != result)
                            internalHandleArgs[i].Dispose();
                }

                if (isConstructCall && result.HasObject && result.Object is V8ManagedObject && result.Object.Handle.HandleInternal == hThis)
                    throw new InvalidOperationException("You've attempted to return the type '" + result.Object.GetType().Name
                        + "' which implements/extends IV8ManagedObject/V8ManagedObject in a construction call (using 'new' in JavaScript) to wrap the new native object."
                        + " The native V8 engine only supports interceptor hooks for objects generated from ObjectTemplate instances.  You will need to first derive/implement from V8NativeObject/IV8NativeObject"
                        + " for construction calls, then wrap it around your object (or rewrite your object to use V8NativeObject directly instead and use the 'SetAccessor()' handle method).");

                return result;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the specified V8Function object type associated with this function template.
        /// There can only ever be one native V8 function object per native V8 function template in a single native V8 JavaScript context;
        /// however, V8.NET (the managed side) does allow one function type per template. In this case, a single call triggers all derived types at once.
        /// The first callback to return a value terminates the cycle and any following callbacks are ignored.
        /// <para>WARNING: The returned function object will be garbage collected if you don't store the reference anywhere. If this happens, then calling 
        /// the function object in JavaScript will return "undefined".</para>
        /// </summary>
        /// <typeparam name="T">A type that implements IV8Function, or derives from V8Function.</typeparam>
        /// <param name="callback">When a new instance of type 'T' is created, it's 'Callback' property will overwritten by this value (replacing anything that may be set when it was created).
        /// It is expect to provide a callback method when using the default 'V8Function' object, but if you have a custom derivation you can set this to 'null'.</param>
        public T GetFunctionObject<T>(JSFunction callback) where T : V8Function, new()
        {
            if (_Engine == null)
                throw new InvalidOperationException("You must create object templates by calling one of the 'V8Engine.CreateFunctionTemplate()' overloads.");

            if (NativeFunctionTemplateProxyInternal == null)
                throw new InvalidOperationException("This managed function template is not initialized.");

            int funcId;
            V8Function func;

            if (m_functionsByTypeInternal.TryGetValue(typeof(T), out funcId))
            {
                var weakRef = _Engine._GetObjectWeakReference(funcId);
                func = weakRef != null ? weakRef.Reset() as V8Function : null;
                if (func != null)
                    return (T)func;
            }

            // ... get the v8 "Function" object ...

            InternalHandle hNativeFunc = V8NetProxy.GetFunction(NativeFunctionTemplateProxyInternal);

            // ... create a managed wrapper for the V8 "Function" object (note: functions inherit the native V8 "Object" type) ...

            func = _Engine._GetObject<T>(this, hNativeFunc.PassOn(), true, false, true); // (note: this will "connect" the native object [hNativeFunc] to a new managed V8Function wrapper, and set the prototype!)

            if (callback != null)
                func.Callback = callback;

            // ... get the function's prototype object, wrap it, and give it to the new function object ...
            // (note: this is a special case, because the function object auto generates the prototype object natively using an existing object template)

            func._Prototype = V8NetProxy.GetObjectPrototype(func._Handle);

            m_functionsByTypeInternal[typeof(T)] = func.ID; // (this exists to index functions by type)

            func.Initialize(false, null);

            return (T)func;
        }

        /// <summary>
        /// Returns a JavaScript V8Function object instance associated with this function template.
        /// There can only ever be ONE V8 function object per V8 function template in a single V8 JavaScript context;
        /// however, V8.NET does allow one MANAGED function type per managed template. In this case, a single call triggers all derived types at once.
        /// The first callback to return a value terminates the cycle and any following callbacks are ignored.
        /// <para>WARNING: The returned function object will be garbage collected if you don't store the reference anywhere. If this happens, then calling 
        /// the function object in JavaScript will return "undefined". This is because function object callbacks are dynamic and are only valid when
        /// the calling object is still in use.</para>
        /// </summary>
        /// <param name="callback">When a new instance of V8Function is created, it's 'Callback' property will set to the specified value.
        /// If you don't provide a callback, then calling the function in JavaScript will simply do nothing and return "undefined".</param>
        public V8Function GetFunctionObject(JSFunction callback) { return GetFunctionObject<V8Function>(callback); }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls the underlying native function to create a new native object and return its handle.
        /// Use this method if you only need the native object and not a managed wrapper.
        /// </summary>
        /// <param name="args">Arguments to pass to the function to construct the new native instance.</param>
        /// <returns>A handle to the new object.</returns>
        public InternalHandle CreateNativeInstance(params InternalHandle[] args) // TODO: Parameter passing needs testing.
        {
            HandleProxy** proxyArgs = null;

            if (args.Length > 0)
            {
                proxyArgs = (HandleProxy**)Utilities.AllocPointerArray(args.Length);
                for (var i = 0; i < args.Length; i++)
                    proxyArgs[i] = args[i];
            }

            try
            {
                return V8NetProxy.CreateFunctionInstance(NativeFunctionTemplateProxyInternal, -1, args.Length, proxyArgs);
            }
            finally
            {
                Utilities.FreeNativeMemory((IntPtr)proxyArgs);
            }
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls the underlying native function to create and return a new instance, which will be wrapped in the specified managed object type.
        /// </summary>
        /// <typeparam name="T">A managed object type to wrap the new native object handle.</typeparam>
        /// <param name="args">Arguments to pass to the function to construct the new native instance.</param>
        /// <returns>A new instance of 'T'.</returns>
        public V8ManagedObject CreateInstance<T>(params InternalHandle[] args) // TODO: Parameter passing needs testing.
            where T : V8ManagedObject, new()
        {
            HandleProxy** proxyArgs = null;

            if (args.Length > 0)
            {
                proxyArgs = (HandleProxy**)Utilities.AllocPointerArray(args.Length);
                for (var i = 0; i < args.Length; i++)
                    proxyArgs[i] = args[i];
            }

            // (note: the special case here is that the native function object will use its own template to create instances)

            T obj = _Engine._CreateManagedObject<T>(this, null, true);
            obj.Template = InstanceTemplate;

            try
            {
                obj._Handle._Set(V8NetProxy.CreateFunctionInstance(NativeFunctionTemplateProxyInternal, obj.ID, args.Length, proxyArgs));
                // (note: setting '_NativeObject' also updates it's '_ManagedObject' field if necessary.

                obj.Initialize(true, args);
            }
            catch (Exception)
            {
                // ... something went wrong, so remove the new managed object ...
                _Engine._RemoveObjectWeakReference(obj.ID);
                throw;
            }
            finally
            {
                Utilities.FreeNativeMemory((IntPtr)proxyArgs);
            }

            return obj;
        }

        /// <summary>
        /// Calls the underlying native function to create and return a new instance, which will be wrapped in a 'V8ManagedObject' instance.
        /// </summary>
        /// <param name="args">Arguments to pass to the function to construct the new native instance.</param>
        /// <returns>A new instance of 'V8ManagedObject'.</returns>
        public V8ManagedObject CreateInstance(params InternalHandle[] args) // TODO: Parameter passing needs testing.
        {
            return CreateInstance<V8ManagedObject>(args);
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This is called by '{V8NativeObject}._OnNativeGCRequested()' when the managed function object is ready to be deleted.
        /// </summary>
        internal void _RemoveFunctionType(int objectId)
        {
            var callbackTypes = m_functionsByTypeInternal.Keys.ToArray();
            foreach (var t in callbackTypes.Where(t => m_functionsByTypeInternal[t] == objectId))
                m_functionsByTypeInternal[t] = -1;
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls the V8 'Set()' function on the underlying native function template to set properties that will exist on all function objects created from this template.
        /// </summary>
        public void SetProperty(string name, InternalHandle value, V8PropertyAttributes attributes)
        {
            try
            {
                if (name.IsNullOrWhiteSpace())
                    throw new ArgumentNullException("name");

                V8NetProxy.SetFunctionTemplateProperty(NativeFunctionTemplateProxyInternal, name, value, attributes);
            }
            finally
            {
                value._DisposeIfFirst();
            }
        }

        // --------------------------------------------------------------------------------------------------------------------
    }

    // ========================================================================================================================
}
