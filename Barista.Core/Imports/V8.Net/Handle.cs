namespace Barista.V8.Net
{
    // ========================================================================================================================
    using System;
    #if !(V1_1 || V2 || V3 || V3_5)
        using System.Dynamic;
        using System.Reflection;
    using System.Linq.Expressions;
    using System.Collections.Generic;

        //public unsafe class DynamicHandleMetaObject : DynamicMetaObject
        //{
        //    InternalHandle _Handle;

        //    internal DynamicHandleMetaObject(InternalHandle handle, Expression parameter)
        //        : base(parameter, BindingRestrictions.Empty, handle)
        //    {
        //        _Handle = handle;
        //    }

        //    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        //    {
        //        return base.BindGetMember(binder);
        //    }

        //    public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        //    {
        //        if (_Handle.IsObjectType)
        //        {
        //            V8NetProxy.SetObjectPropertyByName(_Handle, binder.Name, _Handle.Engine.CreateValue(value.Value));
        //            return;
        //        }
        //        return base.BindSetMember(binder, value);
        //    }

        //    public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        //    {
        //        return base.BindInvoke(binder, args);
        //    }

        //    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        //    {
        //        return base.BindInvokeMember(binder, args);
        //    }

        //    public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        //    {
        //        return base.BindGetIndex(binder, indexes);
        //    }

        //    public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        //    {
        //        return base.BindSetIndex(binder, indexes, value);
        //    }

        //    public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
        //    {
        //        return base.BindDeleteMember(binder);
        //    }

        //    public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
        //    {
        //        return base.BindDeleteIndex(binder, indexes);
        //    }

        //    public override IEnumerable<string> GetDynamicMemberNames()
        //    {
        //        return base.GetDynamicMemberNames();
        //    }

        //    public override DynamicMetaObject BindConvert(ConvertBinder binder)
        //    {
        //        if (binder.ReturnType == typeof(string))
        //            return new DynamicMetaObject(Expression, Restrictions, _Handle.AsString);
        //        else
        //            return new DynamicMetaObject(Expression, Restrictions, null);
        //    }
        //}

    #else
        public interface IDynamicMetaObjectProvider { }
        public class Expression { public static Expression Empty() { return null; } }
        public enum BindingRestrictions { Empty }

        public class DynamicMetaObject
        {
            public DynamicMetaObject(Expression ex, BindingRestrictions rest, object value)
            {
            }
        }
#endif

    // ========================================================================================================================

    /// <summary>
    /// The basic handle interface is a higher level interface that implements members that can be common to many handle types for various 3rd-party script
    /// implementations.  It's primary purpose is to support the DreamSpace.NET development framework, which can support various scripting engines, and is
    /// designed to be non-V8.NET specific.  Third-party scripts should implement this interface for their handles, or create and return value wrappers that
    /// implement this interface.
    /// </summary>
    public interface IBasicHandle : IDisposable, IConvertible
    {
        /// <summary>
        /// Returns the underlying value of this handle.
        /// If the handle represents an object, the the object OR a value represented by the object is returned.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Returns the underlying object associated with this handle.
        /// This exists because 'Value' my not return the underlying object, depending on implementation.
        /// </summary>
        object Object { get; }

        /// <summary>
        /// Returns true if this handle is associated with a CLR object.
        /// </summary>
        bool HasObject { get; }

        /// <summary>
        /// Returns true if this handle is empty (that is, equal to 'Handle.Empty'), and false if a valid handle exists.
        /// <para>An empty state is when a handle is set to 'Handle.Empty' and has no valid native V8 handle assigned.
        /// This is similar to "undefined"; however, this property will be true if a valid native V8 handle exists that is set to "undefined".</para>
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Returns true if this handle is undefined or empty (empty is when this handle is an instance of 'Handle.Empty').
        /// <para>"Undefined" does not mean "null".  A variable (handle) can be defined and set to "null".</para>
        /// </summary>
        bool IsUndefined { get; }

        /// <summary>
        /// Returns 'true' if this handle represents a 'null' value (that is, an explicitly defined 'null' value).
        /// This will return 'false' if 'IsEmpty' or 'IsUndefined' is true.
        /// </summary>
        bool IsNull { get; }

        /// <summary>
        /// The handle represents a Boolean value.
        /// </summary>
        bool IsBoolean { get; }

        /// <summary>
        /// The handle represents an Int32 value.
        /// </summary>
        bool IsInt32 { get; }

        /// <summary>
        /// The handle represents a number value.
        /// </summary>
        bool IsNumber { get; }

        /// <summary>
        /// The handle represents a string value.
        /// </summary>
        bool IsString { get; }

        /// <summary>
        /// The handle represents a *script* object.
        /// </summary>
        bool IsObject { get; }

        /// <summary>
        /// The handle represents a function/procedure/method value.
        /// </summary>
        bool IsFunction { get; }

        /// <summary>
        /// The handle represents a date value.
        /// </summary>
        bool IsDate { get; }

        /// <summary>
        /// The handle represents an array object.
        /// </summary>
        bool IsArray { get; }

        /// <summary>
        /// The handle represents a regular expression object.
        /// </summary>
        bool IsRegExp { get; }

        /// <summary>
        /// Returns true of the handle represents ANY *script* object type.
        /// </summary>
        bool IsObjectType { get; }

        /// <summary>
        /// Returns true of this handle represents an error.
        /// </summary>
        bool IsError { get; }

        /// <summary>
        /// Returns the 'Value' property type cast to the expected type.
        /// Warning: No conversion is made between different value types.
        /// </summary>
        TDerivedType As<TDerivedType>();

        /// Returns the 'LastValue' property type cast to the expected type.
        /// Warning: No conversion is made between different value types.
        TDerivedType LastAs<TDerivedType>();

        /// <summary>
        /// Returns the underlying value converted if necessary to a Boolean type.
        /// </summary>
        bool AsBoolean { get; }

        /// <summary>
        /// Returns the underlying value converted if necessary to an Int32 type.
        /// </summary>
        Int32 AsInt32 { get; }

        /// <summary>
        /// Returns the underlying value converted if necessary to a double type.
        /// </summary>
        double AsDouble { get; }

        /// <summary>
        /// Returns the underlying value converted if necessary to a string type.
        /// </summary>
        String AsString { get; }

        /// <summary>
        /// Returns the underlying value converted if necessary to a DateTime type.
        /// </summary>
        DateTime AsDate { get; }
    }

    /// <summary>
    /// Represents a handle type for tracking native objects.
    /// </summary>
    public interface IHandle
    {
        /// <summary>
        /// Disposes of the current handle proxy reference (if not empty, and different) and replaces it with the specified new reference.
        /// <para>Note: This IS REQUIRED when setting handles, otherwise memory leaks may occur (the native V8 handles will never make it back into the cache).
        /// NEVER use the "=" operator to set a handle.  If using 'InternalHandle' handles, ALWAYS call "Dispose()" when they are no longer needed.
        /// To be safe, use the "using(SomeInternalHandle){}" statement (with 'InternalHandle' handles), or use "Handle refHandle = SomeInternalHandle;", to
        /// to convert it to a handle object that will dispose itself.</para>
        /// </summary>
        InternalHandle Set(InternalHandle handle);

        /// <summary>
        /// Attempts to dispose of the internally wrapped handle proxy and makes this handle empty.
        /// If other handles exist, then they will still be valid, and this handle instance will become empty.
        /// <para>This is useful to use with "using" statements to quickly release a handle into the cache for reuse.</para>
        /// </summary>
        void Dispose();

        /// <summary>
        /// Returns true if this handle is disposed (no longer in use).  Disposed native proxy handles are kept in a cache for performance reasons.
        /// </summary>
        bool IsDisposed { get; }
    }

    /// <summary>
    /// Represents a type that uses or supports a handle.
    /// </summary>
    public interface IHandleBased
    {
        /// <summary>
        /// Returns the engine associated with this instance.
        /// </summary>
        V8Engine Engine { get; }

        /// <summary>
        /// Returns the underlying handle object associated with this instance.  If no 'Handle' object exists, one is created and returned.
        /// </summary>
        Handle AsHandle();

        /// <summary>
        /// Returns a handle value associated with this instance (note: this is not a clone, but an exact copy).
        /// End users should use "AsHandle", unless they are confident in making sure the InternalHandle they use is properly set and later disposed.
        /// </summary>
        InternalHandle AsInternalHandle { get; }

        /// <summary>
        /// Returns the object for this instance, or 'null' if not applicable/available.
        /// </summary>
        V8NativeObject Object { get; }
    }

    // ========================================================================================================================

    /// <summary>
    /// Keeps track of native V8 handles (C++ native side).
    /// When no more handles are in use, the native handle can be disposed when the V8.NET system is ready.
    /// If the handle is a value, the native handle side is disposed immediately - but if the handle represents a managed object, it waits until the managed
    /// object is also no longer in use.
    /// <para>Handles are very small values that can be passed around quickly on the stack, and as a result, the garbage collector is not involved as much.
    /// This helps prevent the GC from kicking in and slowing down applications when a lot of processing is in effect.
    /// Another benefit is that thread locking is required for heap memory allocation (for obvious reasons), so stack allocation is faster within a
    /// multi-threaded context.</para>
    /// </summary>
    public unsafe class Handle : IHandle, IHandleBased, IDynamicMetaObjectProvider, IBasicHandle, IFinalizable
    {
        // --------------------------------------------------------------------------------------------------------------------

        public static readonly Handle Empty = new Handle((HandleProxy*)null);

        // --------------------------------------------------------------------------------------------------------------------

        internal InternalHandle HandleInternal; // ('HandleInfo' or 'WeakReference<HandleInfo>' type only, or 'null' for empty/undefined handles)

        // --------------------------------------------------------------------------------------------------------------------

        internal Handle(HandleProxy* hp)
        {
            HandleInternal._Set(hp, false); // ("check if first" only applies to unwrapped InternalHandle values)
        }

        public Handle(InternalHandle handle)
        {
            HandleInternal.Set(handle);
        }

        ~Handle()
        {
            if (!((IFinalizable)this).CanFinalize && Engine != null)
                lock (Engine.ObjectsToFinalizeInternal)
                {
                    var isLastHandleForObject = (CurrentObjectId >= 0 && ReferenceCount == 1);
                    if (!isLastHandleForObject) // (there should ALWAYS be at least one handle associated with an object)
                        Engine.ObjectsToFinalizeInternal.Add(this);
                    GC.ReRegisterForFinalize(this);
                }
        }

        bool IFinalizable.CanFinalize { get { return IsEmpty || IsDisposed; } set { } }

        void IFinalizable.DoFinalize()
        {
            HandleInternal._Dispose(true);
        }

        /// <summary>
        /// Disposes of the current handle proxy reference (if not empty, and different) and replaces it with the specified new reference.
        /// <para>Note: This IS REQUIRED when setting handles, otherwise memory leaks may occur (the native V8 handles will never make it back into the cache).
        /// NEVER use the "=" operator to set a handle.  If using 'InternalHandle' handles, ALWAYS call "Dispose()" when they are no longer needed.
        /// To be safe, use the "using(SomeInternalHandle){}" statement (with 'InternalHandle' handles), or use "Handle refHandle = SomeInternalHandle;", to
        /// to convert it to a handle object that will dispose itself.</para>
        /// </summary>
        public Handle Set(Handle handle)
        {
            HandleInternal.Set(handle != null ? handle.HandleInternal : InternalHandle.Empty);
            return this;
        }

        /// <summary>
        /// Disposes of the current handle proxy reference (if not empty, and different) and replaces it with the specified new reference.
        /// <para>Note: This IS REQUIRED when setting handles, otherwise memory leaks may occur (the native V8 handles will never make it back into the cache).
        /// NEVER use the "=" operator to set a handle.  If using 'InternalHandle' handles, ALWAYS call "Dispose()" when they are no longer needed.
        /// To be safe, use the "using(SomeInternalHandle){}" statement (with 'InternalHandle' handles), or use "Handle refHandle = SomeInternalHandle;", to
        /// to convert it to a handle object that will dispose itself.</para>
        /// </summary>
        public Handle Set(InternalHandle handle)
        {
            HandleInternal.Set(handle);
            return this;
        }

        InternalHandle IHandle.Set(InternalHandle handle) { return HandleInternal.Set(handle); }

        internal Handle _Set(HandleProxy* hp)
        {
            HandleInternal._Set(hp, false); // ("check if first" only applies to unwrapped InternalHandle values)
            return this;
        }

        /// <summary>
        /// Creates another copy of this handle and increments the reference count to the native handle.
        /// Handles should be set in either two ways: 1. by using the "Set()" method on the left side handle, or 2. using the "Clone()' method on the right side.
        /// Using the "=" operator to set a handle may cause memory leaks if not used correctly.
        /// See also: <seealso cref="Set(InternalHandle)"/>
        /// </summary>
        public Handle Clone()
        {
            return HandleInternal.IsObjectType
                ? new ObjectHandle(HandleInternal)
                : new Handle(HandleInternal);
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Attempts to dispose of the internally wrapped handle proxy and makes this handle empty.
        /// If other handles exist, then they will still be valid, and this handle instance will become empty.
        /// <para>This is useful to use with "using" statements to quickly release a handle into the cache for reuse.</para>
        /// </summary>
        public void Dispose() { HandleInternal._Dispose(false); }

        /// <summary>
        /// Returns true if this handle is disposed (no longer in use).  Disposed native proxy handles are kept in a cache for performance reasons.
        /// </summary>
        public bool IsDisposed { get { return HandleInternal.IsDisposed; } }

        // --------------------------------------------------------------------------------------------------------------------

        public static implicit operator InternalHandle(Handle handle)
        {
            if (handle == null) return InternalHandle.Empty;
            var h = InternalHandle.Empty;
            h.HandleProxyInternal = handle.HandleInternal.HandleProxyInternal; // (this is done to prevent incrementing the managed reference count on implicit conversions [to which a developer may not be aware])
            return h;
        }

        public static implicit operator HandleProxy*(Handle handle)
        {
            return handle != null ? handle.HandleInternal.HandleProxyInternal : null;
        }

        public static implicit operator Handle(HandleProxy* handleProxy)
        {
            var h = InternalHandle._WrapOnly(handleProxy);
            if (h.IsObjectType) return new ObjectHandle(handleProxy);
            return h.IsEmpty ? Handle.Empty : new Handle(handleProxy);
        }

        // --------------------------------------------------------------------------------------------------------------------

        public static bool operator ==(Handle h1, Handle h2)
        {
// ReSharper disable RedundantCast
            return (object)h1 == (object)h2 || (object)h1 != null && h1.Equals(h2);
// ReSharper restore RedundantCast
        }

        public static bool operator !=(Handle h1, Handle h2)
        {
            return !(h1 == h2);
        }

        // --------------------------------------------------------------------------------------------------------------------

        public static implicit operator bool(Handle handle)
        {
            return handle.HandleInternal;
        }

        public static implicit operator Int32(Handle handle)
        {
            return handle.HandleInternal;
        }

        public static implicit operator double(Handle handle)
        {
            return handle.HandleInternal;
        }

        public static implicit operator string(Handle handle)
        {
            return handle.HandleInternal;
        }

        public static implicit operator DateTime(Handle handle)
        {
            return handle.HandleInternal;
        }

        public static implicit operator JSProperty(Handle handle)
        {
            return handle.HandleInternal;
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// A reference to the V8Engine instance that owns this handle.
        /// </summary>
        public virtual V8Engine Engine { get { return HandleInternal.Engine; } }

        public Handle AsHandle()
        {
            return this;
        }

        public InternalHandle AsInternalHandle
        {
            get { return this; }
        }

        // --------------------------------------------------------------------------------------------------------------------
        #region ### SHARED HANDLE CODE START ###

        /// <summary>
        /// The ID (index) of this handle on both the native and managed sides.
        /// </summary>
        public int Id
        {
            get
            {
                return HandleInternal.Id;
            }
        }

        /// <summary>
        /// The JavaScript type this handle represents.
        /// </summary>
        public JSValueType ValueType { get { return HandleInternal.ValueType; } }

        /// <summary>
        /// Used internally to determine the number of references to a handle.
        /// </summary>
        public Int64 ReferenceCount { get { return HandleInternal.ReferenceCount; } }

        // --------------------------------------------------------------------------------------------------------------------
        // Managed Object Properties and References

        /// <summary>
        /// The ID of the managed object represented by this handle.
        /// This ID is expected when handles are passed to 'V8ManagedObject.GetObject()'.
        /// If this value is less than 0 (usually -1), then there is no associated managed object.
        /// </summary>
        public virtual Int32 ObjectId
        {
            get { return HandleInternal.ObjectId; }
            internal set { HandleInternal.ObjectId = value; }
        }

        /// <summary>
        /// Returns the managed object ID "as is".
        /// </summary>
        internal Int32 CurrentObjectId
        {
            get { return HandleInternal.CurrentObjectIdInternal; }
            set { HandleInternal.CurrentObjectIdInternal = value; }
        }

        /// <summary>
        /// A reference to the managed object associated with this handle. This property is only valid for object handles, and will return null otherwise.
        /// Upon reading this property, if the managed object has been garbage collected (because no more handles or references exist), then a new basic 'V8NativeObject' instance will be created.
        /// <para>Instead of checking for 'null' (which may not work as expected), query 'HasManagedObject' instead.</para>
        /// </summary>
        public V8NativeObject Object { get { return HandleInternal.Object; } }

        /// <summary>
        /// If this handle represents an object instance binder, then this returns the bound object.
        /// Bound objects are usually custom user objects (non-V8.NET objects) wrapped in ObjectBinder instances.
        /// </summary>
        public object BoundObject { get { return HandleInternal.BoundObject; } }

        object IBasicHandle.Object { get { return BoundObject ?? Object; } }

        /// <summary>
        /// Returns the registered type ID for objects that represent registered CLR types.
        /// </summary>
        public Int32 ClrTypeId { get { return HandleInternal.ClrTypeId; } }

        /// <summary>
        /// If this handle represents a type binder, then this returns the associated 'TypeBinder' instance.
        /// <para>Bound types are usually non-V8.NET types that are wrapped and exposed in the JavaScript environment for use with the 'new' operator.</para>
        /// </summary>
        public TypeBinder TypeBinder { get { return HandleInternal.TypeBinder; } }

        /// <summary>
        /// Returns true if this handle is associated with a managed object.
        /// <para>Note: This can be false even though 'IsObjectType' may be true.
        /// A handle can represent a native V8 object handle without requiring an associated managed object.</para>
        /// </summary>
        public bool HasObject { get { return HandleInternal.HasObject; } }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reading from this property causes a native call to fetch the current V8 value associated with this handle.
        /// <param>For objects, this returns the in-script type text as a string - unless this handle represents an object binder, in which case this will return the bound object instead.</param>
        /// </summary>
        public object Value { get { return HandleInternal.Value; } }

        /// <summary>
        /// Once "Value" is accessed to retrieve the JavaScript value in real time, there's no need to keep accessing it.  Just call this property
        /// instead (a small bit faster). Note: If the value changes again within the engine (i.e. another scripts executes), you may need to call
        /// 'Value' again to make sure any changes are reflected.
        /// </summary>
        public object LastValue { get { return HandleInternal.LastValue; } }

        /// <summary>
        /// Returns the array length for handles that represent arrays. For all other types, this returns 0.
        /// Note: To get the items of the array, use '{ObjectHandle|InternalHandle|V8NativeObject}.GetProperty(#)'.
        /// </summary>
        public Int32 ArrayLength { get { return HandleInternal.ArrayLength; } }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if this handle is associated with a managed object that has no other references and is ready to be disposed.
        /// </summary>
        public bool IsWeakManagedObject { get { return HandleInternal.IsWeakManagedObject; } }

        /// <summary>
        /// Returns true if the handle is weak and ready to be disposed.
        /// </summary>
        public bool IsWeakHandle { get { return HandleInternal.IsWeakHandle; } }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// True if this handle is place into a queue to be made weak and eventually disposed (cached) on the native side.
        /// </summary>
        public bool IsInPendingDisposalQueue
        {
            get { return HandleInternal.IsInPendingDisposalQueue; }
            internal set { HandleInternal.IsInPendingDisposalQueue = value; }
        }

        /// <summary>
        /// True if this handle was made weak on the native side (for object handles only).  Once a handle is weak, the V8 garbage collector can collect the
        /// handle (and any associated managed object) at any time.
        /// </summary>
        public bool IsNativelyWeak
        {
            get { return HandleInternal.IsNativelyWeak; }
        }

        /// <summary>
        /// Returns true if this handle is weak AND is associated with a weak managed object reference.
        /// When a handle is ready to be disposed, then calling "Dispose()" will succeed and cause the handle to be placed back into the cache on the native side.
        /// </summary>
        public bool IsDisposeReady { get { return HandleInternal.IsDisposeReady; } }

        /// <summary>
        /// Attempts to dispose of this handle (add it back into the native proxy cache for reuse).  If the handle represents a managed object,
        /// the dispose request is placed into a "pending disposal" queue. When the associated managed object
        /// no longer has any references, this method will be .
        /// <para>*** NOTE: This is called by Dispose() when the reference count becomes zero and should not be called directly. ***</para>
        /// </summary>
        internal bool __TryDispose() { return HandleInternal.__TryDispose(); }

        /// <summary>
        /// Completes the disposal of the native handle.
        /// <para>Note: A disposed native handle is simply cached for reuse, and always points back to the same managed handle.</para>
        /// </summary>
        internal void _CompleteDisposal() { HandleInternal._CompleteDisposal(); }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Forces the underlying object, if any, to separate from the handle.  This is done by swapping the object with a
        /// place holder object to keep the ID (index) for the current object alive until the native V8 engine's GC can remove
        /// any associated handles later.  The released object is returned, or null if there is no object.
        /// </summary>
        /// <returns>The object released.</returns>
        public V8NativeObject ReleaseManagedObject() { return HandleInternal.ReleaseManagedObject(); }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if this handle is empty (that is, equal to 'Handle.Empty'), and false if a valid handle exists.
        /// <para>An empty state is when a handle is set to 'Handle.Empty' and has no valid native V8 handle assigned.
        /// This is similar to "undefined"; however, this property will be true if a valid native V8 handle exists that is set to "undefined".</para>
        /// </summary>
        public bool IsEmpty { get { return HandleInternal.IsEmpty; } }

        /// <summary>
        /// Returns true if this handle is undefined or empty (empty is when this handle is an instance of 'Handle.Empty').
        /// <para>"Undefined" does not mean "null".  A variable (handle) can be defined and set to "null".</para>
        /// </summary>
        public bool IsUndefined { get { return HandleInternal.IsUndefined; } }

        /// <summary>
        /// Returns 'true' if this handle represents a 'null' value (that is, an explicitly defined 'null' value).
        /// This will return 'false' if 'IsEmpty' or 'IsUndefined' is true.
        /// </summary>
        public bool IsNull { get { return HandleInternal.IsNull; } }

        public bool IsBoolean { get { return HandleInternal.IsBoolean; } }
        public bool IsBooleanObject { get { return HandleInternal.IsBooleanObject; } }
        public bool IsInt32 { get { return HandleInternal.IsInt32; } }
        public bool IsNumber { get { return HandleInternal.IsNumber; } }
        public bool IsNumberObject { get { return HandleInternal.IsNumberObject; } }
        public bool IsString { get { return HandleInternal.IsString; } }
        public bool IsStringObject { get { return HandleInternal.IsStringObject; } }
        public bool IsObject { get { return HandleInternal.IsObject; } }
        public bool IsFunction { get { return HandleInternal.IsFunction; } }
        public bool IsDate { get { return HandleInternal.IsDate; } }
        public bool IsArray { get { return HandleInternal.IsArray; } }
        public bool IsRegExp { get { return HandleInternal.IsRegExp; } }

        /// <summary>
        /// Returns true of the handle represents ANY object type.
        /// </summary>
        public bool IsObjectType { get { return HandleInternal.IsObjectType; } }

        /// <summary>
        /// Used internally to quickly determine when an instance represents a binder object type (faster than reflection!).
        /// </summary>
        public bool IsBinder { get { return HandleInternal.IsBinder; } }

        /// <summary>
        /// Returns the binding mode (Instance, Static, or None) represented by this handle.  The return is 'None' (0) if not applicable.
        /// </summary>
        public BindingMode BindingMode { get { return HandleInternal.BindingMode; } }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the 'Value' property type cast to the expected type.
        /// Warning: No conversion is made between different value types.
        /// </summary>
        public TDerivedType As<TDerivedType>() { return HandleInternal.As<TDerivedType>(); }

        /// Returns the 'LastValue' property type cast to the expected type.
        /// Warning: No conversion is made between different value types.
        public TDerivedType LastAs<TDerivedType>() { return HandleInternal.LastAs<TDerivedType>(); }

        /// <summary>
        /// Returns the underlying value converted if necessary to a Boolean type.
        /// </summary>
        public bool AsBoolean { get { return (bool)HandleInternal; } }

        /// <summary>
        /// Returns the underlying value converted if necessary to an Int32 type.
        /// </summary>
        public Int32 AsInt32 { get { return (Int32)HandleInternal; } }

        /// <summary>
        /// Returns the underlying value converted if necessary to a double type.
        /// </summary>
        public double AsDouble { get { return (double)HandleInternal; } }

        /// <summary>
        /// Returns the underlying value converted if necessary to a string type.
        /// </summary>
        public String AsString { get { return (String)HandleInternal; } }

        /// <summary>
        /// Returns the underlying value converted if necessary to a DateTime type.
        /// </summary>
        public DateTime AsDate { get { return (DateTime)HandleInternal; } }

        /// <summary>
        /// Returns this handle as a new JSProperty instance with default property attributes.
        /// </summary>
        public IJSProperty AsJSProperty() { return (JSProperty)HandleInternal; }

        // --------------------------------------------------------------------------------------------------------------------

        public override string ToString() { return HandleInternal.ToString(); }

        /// <summary>
        /// Checks if the wrapped handle reference is the same as the one compared with. This DOES NOT compare the underlying JavaScript values for equality.
        /// To test for JavaScript value equality, convert to a desired value-type instead by first casting as needed (i.e. (int)jsv1 == (int)jsv2).
        /// </summary>
        public override bool Equals(object obj) { return HandleInternal.Equals(obj); }

        public override int GetHashCode() { return HandleInternal.GetHashCode(); }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if this handle contains an error message (the string value is the message).
        /// If you have exception catching in place, you can simply call 'ThrowOnError()' instead.
        /// </summary>
        public bool IsError { get { return HandleInternal.IsError; } }

        /// <summary>
        /// Checks if the handle represents an error, and if so, throws one of the corresponding derived V8Exception exceptions.
        /// See 'JSValueType' for possible exception states.  You can check the 'IsError' property to see if this handle represents an error.
        /// <para>Exceptions thrown: V8InternalErrorException, V8CompilerErrorException, V8ExecutionErrorException, and V8Exception (for any general V8-related exceptions).</para>
        /// </summary>
        public void ThrowOnError() { HandleInternal.ThrowOnError(); }

        // --------------------------------------------------------------------------------------------------------------------
#if !(V1_1 || V2 || V3 || V3_5)

        public virtual DynamicMetaObject GetMetaObject(Expression parameter)
        {
            if (!IsObjectType)
                return new DynamicMetaObject(parameter, BindingRestrictions.Empty, this); // (there's no object, this is a value type, so just pass along the value)
            return new DynamicHandle(this, parameter);
        }

#endif
        // --------------------------------------------------------------------------------------------------------------------

        public TypeCode GetTypeCode() { return HandleInternal.GetTypeCode(); }

        public bool ToBoolean(IFormatProvider provider) { return HandleInternal.ToBoolean(provider); }
        public byte ToByte(IFormatProvider provider) { return HandleInternal.ToByte(provider); }
        public char ToChar(IFormatProvider provider) { return HandleInternal.ToChar(provider); }
        public DateTime ToDateTime(IFormatProvider provider) { return HandleInternal.ToDateTime(provider); }
        public decimal ToDecimal(IFormatProvider provider) { return HandleInternal.ToDecimal(provider); }
        public double ToDouble(IFormatProvider provider) { return HandleInternal.ToDouble(provider); }
        public short ToInt16(IFormatProvider provider) { return HandleInternal.ToInt16(provider); }
        public int ToInt32(IFormatProvider provider) { return HandleInternal.ToInt32(provider); }
        public long ToInt64(IFormatProvider provider) { return HandleInternal.ToInt64(provider); }
        public sbyte ToSByte(IFormatProvider provider) { return HandleInternal.ToSByte(provider); }
        public float ToSingle(IFormatProvider provider) { return HandleInternal.ToSingle(provider); }
        public string ToString(IFormatProvider provider) { return HandleInternal.ToString(provider); }
        public object ToType(Type conversionType, IFormatProvider provider) { return HandleInternal.ToType(conversionType, provider); }
        public ushort ToUInt16(IFormatProvider provider) { return HandleInternal.ToUInt16(provider); }
        public uint ToUInt32(IFormatProvider provider) { return HandleInternal.ToUInt32(provider); }
        public ulong ToUInt64(IFormatProvider provider) { return HandleInternal.ToUInt64(provider); }

        #endregion ### SHARED HANDLE CODE END ###
        // --------------------------------------------------------------------------------------------------------------------
    }

    // ========================================================================================================================

    /// <summary>
    /// Represents methods that can be called on V8 objects.
    /// </summary>
    public interface IV8Object
    {
        /// <summary>
        /// Calls the V8 'Set()' function on the underlying native object.
        /// Returns true if successful.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="attributes">Flags that describe the property behavior.  They must be 'OR'd together as needed. (V8PropertyAttributes.Undefined)</param>
        /// <param name="name"></param>
        bool SetProperty(string name, InternalHandle value, V8PropertyAttributes attributes);

        /// <summary>
        /// Calls the V8 'Set()' function on the underlying native object.
        /// Returns true if successful.
        /// </summary>
        bool SetProperty(Int32 index, InternalHandle value);

        /// <summary>
        /// Sets a property to a given object. If the object is not V8.NET related, then the system will attempt to bind the instance and all public members to
        /// the specified property name.
        /// Returns true if successful.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="obj">Some value or object instance. 'Engine.CreateValue()' will be used to convert value types.</param>
        /// <param name="className">A custom in-script function name for the specified object type, or 'null' to use either the type name as is (the default) or any existing 'ScriptObject' attribute name. (Null)</param>
        /// <param name="recursive">For object instances, if true, then object reference members are included, otherwise only the object itself is bound and returned.
        /// For security reasons, public members that point to object instances will be ignored. This must be true to included those as well, effectively allowing
        /// in-script traversal of the object reference tree (so make sure this doesn't expose sensitive methods/properties/fields). (Null)</param>
        /// <param name="memberSecurity">For object instances, these are default flags that describe JavaScript properties for all object instance members that
        /// don't have any 'ScriptMember' attribute.  The flags should be 'OR'd together as needed. (Null)</param>
        bool SetProperty(string name, object obj, string className, bool? recursive, ScriptMemberSecurity? memberSecurity);

        /// <summary>
        /// Binds a 'V8Function' object to the specified type and associates the type name (or custom script name) with the underlying object.
        /// Returns true if successful.
        /// </summary>
        /// <param name="type">The type to wrap.</param>
        /// <param name="propertyAttributes">Flags that describe the property behavior.  They must be 'OR'd together as needed. (V8PropertyAttributes.None)</param>
        /// <param name="className">A custom in-script function name for the specified type, or 'null' to use either the type name as is (the default) or any existing 'ScriptObject' attribute name. (Null)</param>
        /// <param name="recursive">For object types, if true, then object reference members are included, otherwise only the object itself is bound and returned.
        /// For security reasons, public members that point to object instances will be ignored. This must be true to included those as well, effectively allowing
        /// in-script traversal of the object reference tree (so make sure this doesn't expose sensitive methods/properties/fields). (Null)</param>
        /// <param name="memberSecurity">For object instances, these are default flags that describe JavaScript properties for all object instance members that
        /// don't have any 'ScriptMember' attribute.  The flags should be 'OR'd together as needed. (Null)</param>
        bool SetProperty(Type type, V8PropertyAttributes propertyAttributes, string className, bool? recursive, ScriptMemberSecurity? memberSecurity);

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls the V8 'Get()' function on the underlying native object.
        /// If the property doesn't exist, the 'IsUndefined' property will be true.
        /// </summary>
        InternalHandle GetProperty(string name);

        /// <summary>
        /// Calls the V8 'Get()' function on the underlying native object.
        /// If the property doesn't exist, the 'IsUndefined' property will be true.
        /// </summary>
        InternalHandle GetProperty(Int32 index);

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls the V8 'Delete()' function on the underlying native object.
        /// Returns true if the property was deleted.
        /// </summary>
        bool DeleteProperty(string name);

        /// <summary>
        /// Calls the V8 'Delete()' function on the underlying native object.
        /// Returns true if the property was deleted.
        /// </summary>
        bool DeleteProperty(Int32 index);

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls the V8 'SetAccessor()' function on the underlying native object to create a property that is controlled by "getter" and "setter" callbacks.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        /// <param name="attributes">(V8PropertyAttributes.None)</param>
        /// <param name="access">(V8AccessControl.Default)</param>
        void SetAccessor(string name,
            V8NativeObjectPropertyGetter getter, V8NativeObjectPropertySetter setter,
            V8PropertyAttributes attributes, V8AccessControl access);

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns a list of all property names for this object (including all objects in the prototype chain).
        /// </summary>
        string[] GetPropertyNames();

        /// <summary>
        /// Returns a list of all property names for this object (excluding the prototype chain).
        /// </summary>
        string[] GetOwnPropertyNames();

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get the attribute flags for a property of this object.
        /// If a property doesn't exist, then 'V8PropertyAttributes.None' is returned
        /// <para>(Note: only V8 returns 'None'. The value 'Undefined' has an internal proxy meaning for property interception).</para>
        /// </summary>
        V8PropertyAttributes GetPropertyAttributes(string name);

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls an object property with a given name on a specified object as a function and returns the result.
        /// The '_this' property is the "this" object within the function when called.
        /// If the function name is null or empty, then the current object is assumed to be a function object.
        /// </summary>
        InternalHandle Call(string functionName, InternalHandle _this, params InternalHandle[] args);

        /// <summary>
        /// Calls an object property with a given name on a specified object as a function and returns the result.
        /// If the function name is null or empty, then the current object is assumed to be a function object.
        /// </summary>
        InternalHandle StaticCall(string functionName, params InternalHandle[] args);

        // --------------------------------------------------------------------------------------------------------------------
    }

    // ========================================================================================================================

    public unsafe class ObjectHandle : Handle, IV8Object
    {
        // --------------------------------------------------------------------------------------------------------------------

        new public static ObjectHandle Empty { get { return new ObjectHandle((HandleProxy*)null); } }

        // --------------------------------------------------------------------------------------------------------------------

        internal ObjectHandle(HandleProxy* hp)
            : base(hp)
        {
            if (!IsUndefined && !IsObjectType) throw new InvalidOperationException("The native handle proxy does not represent a native object.");
        }

        public ObjectHandle(InternalHandle handle)
            : base(handle)
        {
            if (!IsUndefined && !IsObjectType) throw new InvalidOperationException("The handle does not represent a native object.");
        }

        // --------------------------------------------------------------------------------------------------------------------

        public static implicit operator ObjectHandle(HandleProxy* handleProxy)
        {
            return handleProxy != null ? new ObjectHandle(handleProxy) : ObjectHandle.Empty;
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls the V8 'Set()' function on the underlying native object.
        /// Returns true if successful.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="attributes">Flags that describe the property behavior.  They must be 'OR'd together as needed. (V8PropertyAttributes.None)</param>
        public virtual bool SetProperty(string name, InternalHandle value, V8PropertyAttributes attributes)
        {
            return HandleInternal.SetProperty(name, value, attributes);
        }

        /// <summary>
        /// Calls the V8 'Set()' function on the underlying native object.
        /// Returns true if successful.
        /// </summary>
        public virtual bool SetProperty(Int32 index, InternalHandle value)
        {
            return HandleInternal.SetProperty(index, value);
        }

        /// <summary>
        /// Sets a property to a given object. If the object is not V8.NET related, then the system will attempt to bind the instance and all public members to
        /// the specified property name.
        /// Returns true if successful.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="obj">Some value or object instance. 'Engine.CreateValue()' will be used to convert value types.</param>
        public bool SetProperty(string name, object obj)
        {
            return SetProperty(name, obj, null, null, null);
        }

        /// <summary>
        /// Sets a property to a given object. If the object is not V8.NET related, then the system will attempt to bind the instance and all public members to
        /// the specified property name.
        /// Returns true if successful.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="obj">Some value or object instance. 'Engine.CreateValue()' will be used to convert value types.</param>
        /// <param name="className">A custom type name, or 'null' to use either the type name as is (the default), or any existing 'ScriptObject' attribute name. (null)</param>
        /// <param name="recursive">For object instances, if true, then object reference members are included, otherwise only the object itself is bound and returned.
        /// For security reasons, public members that point to object instances will be ignored. This must be true to included those as well, effectively allowing
        /// in-script traversal of the object reference tree (so make sure this doesn't expose sensitive methods/properties/fields). (null)</param>
        /// <param name="memberSecurity">Flags that describe JavaScript properties.  They must be 'OR'd together as needed. (null)</param>
        public bool SetProperty(string name, object obj, string className, bool? recursive, ScriptMemberSecurity? memberSecurity)
        {
            return HandleInternal.SetProperty(name, obj, className, recursive, memberSecurity);
        }

        /// <summary>
        /// Binds a 'V8Function' object to the specified type and associates the type name (or custom script name) with the underlying object.
        /// Returns true if successful.
        /// </summary>
        /// <param name="type">The type to wrap.</param>
        /// <param name="propertyAttributes">Flags that describe the property behavior.  They must be 'OR'd together as needed. (V8PropertyAttributes.None)</param>
        /// <param name="className">A custom type name, or 'null' to use either the type name as is (the default), or any existing 'ScriptObject' attribute name. (Null)</param>
        /// <param name="recursive">For object types, if true, then object reference members are included, otherwise only the object itself is bound and returned.
        /// For security reasons, public members that point to object instances will be ignored. This must be true to included those as well, effectively allowing
        /// in-script traversal of the object reference tree (so make sure this doesn't expose sensitive methods/properties/fields). (Null)</param>
        /// <param name="memberSecurity">Flags that describe JavaScript properties.  They must be 'OR'd together as needed. (Null)</param>
        public bool SetProperty(Type type, V8PropertyAttributes propertyAttributes, string className, bool? recursive, ScriptMemberSecurity? memberSecurity)
        {
            return HandleInternal.SetProperty(type, propertyAttributes, className, recursive, memberSecurity);
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls the V8 'Get()' function on the underlying native object.
        /// If the property doesn't exist, the 'IsUndefined' property will be true.
        /// </summary>
        public virtual InternalHandle GetProperty(string name)
        {
            return HandleInternal.GetProperty(name);
        }

        /// <summary>
        /// Calls the V8 'Get()' function on the underlying native object.
        /// If the property doesn't exist, the 'IsUndefined' property will be true.
        /// </summary>
        public virtual InternalHandle GetProperty(Int32 index)
        {
            return HandleInternal.GetProperty(index);
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls the V8 'Delete()' function on the underlying native object.
        /// Returns true if the property was deleted.
        /// </summary>
        public virtual bool DeleteProperty(string name)
        {
            return HandleInternal.GetProperty(name);
        }

        /// <summary>
        /// Calls the V8 'Delete()' function on the underlying native object.
        /// Returns true if the property was deleted.
        /// </summary>
        public virtual bool DeleteProperty(Int32 index)
        {
            return HandleInternal.GetProperty(index);
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls the V8 'SetAccessor()' function on the underlying native object to create a property that is controlled by "getter" and "setter" callbacks.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        /// <param name="attributes">(V8PropertyAttributes.None)</param>
        /// <param name="access">(V8AccessControl.Default)</param>
        public void SetAccessor(string name,
            V8NativeObjectPropertyGetter getter, V8NativeObjectPropertySetter setter,
            V8PropertyAttributes attributes, V8AccessControl access)
        {
            HandleInternal.SetAccessor(name, getter, setter, attributes, access);
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns a list of all property names for this object (including all objects in the prototype chain).
        /// </summary>
        public string[] GetPropertyNames()
        {
            return HandleInternal.GetPropertyNames();
        }

        /// <summary>
        /// Returns a list of all property names for this object (excluding the prototype chain).
        /// </summary>
        public virtual string[] GetOwnPropertyNames()
        {
            return HandleInternal.GetOwnPropertyNames();
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Get the attribute flags for a property of this object.
        /// If a property doesn't exist, then 'V8PropertyAttributes.None' is returned
        /// <para>(Note: only V8 returns 'None'. The value 'Undefined' has an internal proxy meaning for property interception).</para>
        /// </summary>
        public V8PropertyAttributes GetPropertyAttributes(string name)
        {
            return HandleInternal.GetPropertyAttributes(name);
        }

        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Calls an object property with a given name on a specified object as a function and returns the result.
        /// The '_this' property is the "this" object within the function when called.
        /// </summary>
        public virtual InternalHandle Call(string functionName, InternalHandle _this, params InternalHandle[] args)
        {
            return HandleInternal.Call(functionName, _this, args);
        }

        /// <summary>
        /// Calls an object property with a given name on a specified object as a function and returns the result.
        /// </summary>
        public InternalHandle StaticCall(string functionName, params InternalHandle[] args)
        {
            return HandleInternal.StaticCall(functionName, args);
        }

        /// <summary>
        /// Calls the underlying object as a function.
        /// The '_this' parameter is the "this" reference within the function when called.
        /// </summary>
        public InternalHandle Call(InternalHandle _this, params InternalHandle[] args)
        {
            return HandleInternal.Call(_this, args);
        }

        /// <summary>
        /// Calls the underlying object as a function.
        /// The 'this' property will not be specified, which will default to the global scope as expected.
        /// </summary>
        public InternalHandle StaticCall(params InternalHandle[] args)
        {
            return HandleInternal.StaticCall(args);
        }

        // --------------------------------------------------------------------------------------------------------------------
#if !(V1_1 || V2 || V3 || V3_5)

        public override DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicHandle(this, parameter);
        }

#endif
        // --------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// The prototype of the object (every JavaScript object implicitly has a prototype).
        /// </summary>
        public ObjectHandle Prototype
        {
            get
            {
                return HandleInternal.Prototype;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------
    }

    // ========================================================================================================================

    /// <summary>
    /// This is a class used internally to create a meta object for dynamic access to 
    /// </summary>
    public sealed class DynamicHandle : DynamicMetaObject
    {
        IV8Object m_handle;
        V8Engine m_engine;

#if (V1_1 || V2 || V3 || V3_5)

        internal DynamicHandle(object value, Expression parameter)
            : base(parameter, BindingRestrictions.Empty, value)
        {
            m_handle = value as IV8Object;
            var based = value as IHandleBased;
            if (based != null)
                m_engine = based.Engine;
        }

#else

        internal DynamicHandle(object value, Expression parameter)
            : base(parameter, BindingRestrictions.GetTypeRestriction(parameter, value.GetType()), value)
        {
            m_handle = value as IV8Object;
            if (value is IHandleBased) m_engine = ((IHandleBased)value).Engine;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            if (m_handle == null)
                throw new InvalidOperationException(InternalHandle.NotAnObjectErrorMsg);

            Expression[] args = new Expression[1];
            MethodInfo methodInfo = ((Func<string, InternalHandle>)m_handle.GetProperty).Method;

            args[0] = Expression.Constant(binder.Name);

            Expression self = Expression.Convert(Expression, LimitType);

            Expression methodCall = Expression.Call(self, methodInfo, args);

            BindingRestrictions restrictions = Restrictions;

            Func<InternalHandle, object> handleWrapper = h => h.HasObject ? h.Object : (Handle)h; // (need to wrap the internal handle value with an object based handle in order to dispose of the value!)

            return new DynamicMetaObject(Expression.Convert(methodCall, typeof(object), handleWrapper.Method), restrictions);
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            if (m_handle == null)
                throw new InvalidOperationException(InternalHandle.NotAnObjectErrorMsg);

            var isHandle = (value.RuntimeType == typeof(InternalHandle) || typeof(Handle).IsAssignableFrom(value.RuntimeType));
            var isV8NativeObject = typeof(V8NativeObject).IsAssignableFrom(value.RuntimeType);

            Expression[] args = new Expression[isHandle || isV8NativeObject ? 3 : 5];
            MethodInfo methodInfo;

            args[0] = Expression.Constant(binder.Name);

            if (isHandle || isV8NativeObject)
            {
                Func<object, InternalHandle> handleParamConversion
                    = obj => (obj is IHandleBased) ? ((IHandleBased)obj).AsInternalHandle
                        : m_engine != null ? m_engine.CreateValue(obj, null, null)
                        : InternalHandle.Empty;
                var convertParameter = Expression.Call(Expression.Constant(handleParamConversion.Target), handleParamConversion.Method, Expression.Convert(value.Expression, typeof(object)));
                args[1] = convertParameter;
                args[2] = Expression.Constant(V8PropertyAttributes.None);
                methodInfo = ((Func<string, InternalHandle, V8PropertyAttributes, bool>)m_handle.SetProperty).Method;
            }
            else
            {
                args[1] = Expression.Convert(value.Expression, typeof(object));
                args[2] = Expression.Constant(null, typeof(string));
                args[3] = Expression.Constant(null, typeof(Nullable<bool>));
                args[4] = Expression.Constant(null, typeof(Nullable<ScriptMemberSecurity>));
                methodInfo = ((Func<string, object, string, bool?, ScriptMemberSecurity?, bool>)m_handle.SetProperty).Method;
            }

            Expression self = Expression.Convert(Expression, LimitType);

            Expression methodCall = Expression.Call(self, methodInfo, args);

            BindingRestrictions restrictions = Restrictions.Merge(value.Restrictions);

            return new DynamicMetaObject(Expression.Convert(methodCall, binder.ReturnType), restrictions);
        }

        public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
        {
            return base.BindInvoke(binder, args);
        }

        public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        {
            return base.BindInvokeMember(binder, args);
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return base.BindGetIndex(binder, indexes);
        }

        public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            return base.BindSetIndex(binder, indexes, value);
        }

        public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
        {
            return base.BindDeleteMember(binder);
        }

        public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
        {
            return base.BindDeleteIndex(binder, indexes);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            if (m_handle == null)
                throw new InvalidOperationException(InternalHandle.NotAnObjectErrorMsg);
            return m_handle.GetPropertyNames();
        }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            Expression convertExpression;

            if (LimitType.IsAssignableFrom(binder.Type))
            {
                convertExpression = Expression.Convert(Expression, binder.Type);
            }
            else if (typeof(V8NativeObject).IsAssignableFrom(binder.Type))
            {
                Func<object, V8NativeObject> getUnderlyingObjectMethod = obj => obj is IHandleBased ? ((IHandleBased)obj).Object : null;
                convertExpression = Expression.Convert(Expression.Convert(Expression, typeof(V8NativeObject), getUnderlyingObjectMethod.Method), binder.Type);
            }
            else
            {
                MethodInfo conversionMethodInfo;

                if (binder.Type == typeof(InternalHandle))
                    conversionMethodInfo =
                        ((Func<object, InternalHandle>)(obj => obj is IHandleBased ? ((IHandleBased)obj).AsInternalHandle : InternalHandle.Empty)).Method;
                else if (binder.Type == typeof(Handle))
                    conversionMethodInfo =
                        ((Func<object, Handle>)(obj => obj is IHandleBased ? ((IHandleBased)obj).AsHandle() : Handle.Empty)).Method;
                else
                    conversionMethodInfo = ((Func<object, object>)(obj => Types.ChangeType(Value, binder.Type, null))).Method;

                convertExpression = Expression.Convert(Expression, binder.Type, conversionMethodInfo);
            }

            BindingRestrictions restrictions = Restrictions.Merge(BindingRestrictions.GetTypeRestriction(convertExpression, binder.Type));

            return new DynamicMetaObject(convertExpression, Restrictions);
        }

#endif
    }

    // ========================================================================================================================
}
