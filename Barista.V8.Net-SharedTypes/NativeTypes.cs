﻿using System;
using System.Runtime.InteropServices;

namespace Barista.V8.Net
{
    // ========================================================================================================================

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct NativeV8EngineProxy
    {
        public ProxyObjectType NativeClassType;
        public Int32 ID;
    }

    // ========================================================================================================================

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct NativeObjectTemplateProxy
    {
        public ProxyObjectType NativeClassType;
        public void* NativeEngineProxy;
        public Int32 EngineID;
        public Int32 ObjectID;
        public void* NativeObjectTemplate;
    }

    // ========================================================================================================================

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct NativeFunctionTemplateProxy
    {
        public ProxyObjectType NativeClassType;
        public void* NativeEngineProxy;
        public Int32 EngineID;
        public void* NativeFucntionTemplate;
    }

    // ========================================================================================================================

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ManagedAccessorInfo
    {
        public NativeObjectTemplateProxy* NativeObjectTemplateProxy;
        public Int32 ManagedObjectID; // This is ALWAYS set to a manage object ID (index) that is associated with the call-back process for objects created from templates.

    }

    // ========================================================================================================================

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 68)]
    public unsafe struct HandleProxy
    {
        // --------------------------------------------------------------------------------------------------------------------
        // Native Fields 

        [FieldOffset(0), MarshalAs(UnmanagedType.I4)]
        public ProxyObjectType NativeClassType;
        
        [FieldOffset(4), MarshalAs(UnmanagedType.I4)]
        public Int32 ID; // The native ID (index) of this handle proxy.
        
        [FieldOffset(8), MarshalAs(UnmanagedType.I4)]
        public Int32 _ObjectID; // If set (>=0), then a managed object is associated with this handle. The default is -1.

        [FieldOffset(12), MarshalAs(UnmanagedType.I4)]
        public Int32 _CLRTypeID; // If set (>=0), then this represents a registered type. The default is -1.

        [FieldOffset(16), MarshalAs(UnmanagedType.I4)]
        public JSValueType _ValueType; // 32-bit value type, which is the type this handle represents.  This is not provided by default until 'GetType()' is called.

        #region ### HANDLE VALUE ### - Note: This is only valid upon calling 'UpdateValue()'.
        [FieldOffset(20), MarshalAs(UnmanagedType.I1)]
        public Byte V8Boolean;
        [FieldOffset(20), MarshalAs(UnmanagedType.I8)]
        public Int64 V8Integer; // (JavaScript only supports 32-bit integers, but this is needed to keep the expected marshalling size of the native union)
        [FieldOffset(20)]
        public double V8Number; // (also used with Date milliseconds since epoch [Jan 1, 1970  00:00:00])
        [FieldOffset(28)]
        public void* V8String; // (for strings and objects, this is the ToString() value [Unicode characters (2 bytes each)])
        #endregion

        [FieldOffset(36), MarshalAs(UnmanagedType.I8)]
        public Int64 ManagedReferenceCount; // The number of references on the managed side.

        [FieldOffset(44), MarshalAs(UnmanagedType.I4)]
        public Int32 Disposed; // (0 = in use, 1 = managed side ready to dispose, 2 = object is weak (if applicable), 3 = disposed/cached)

        [FieldOffset(48), MarshalAs(UnmanagedType.I4)]
        public Int32 EngineID;

        [FieldOffset(52)]
        public void* NativeEngineProxy; // Pointer to the native V8 engine proxy object associated with this proxy handle instance (used native side to free the handle upon destruction).

        [FieldOffset(60)]
        public void* NativeV8Handle; // The native V8 persistent object handle (not used on the managed side).

        // --------------------------------------------------------------------------------------------------------------------
        // Properties for interpretation of fields.

        public bool IsDisposeReady
        {
            get { return Disposed == 1 || Disposed == 2; }
            set { if (Disposed <= 1) Disposed = value ? 1 : 0; } // (once disposed is > 1, the process cannot be stopped)
        }

        public bool IsWeak { get { return Disposed == 2; } }

        public bool IsDisposed { get { return Disposed == 3; } }

        // --------------------------------------------------------------------------------------------------------------------

        public object Value
        {
            get
            {
                if ((int)_ValueType == -1) throw new InvalidOperationException("Type is unknown - you must call 'V8NetProxy.UpdateHandleValue()' first.");
                switch (_ValueType)
                {
                    case JSValueType.Undefined: return "undefined";
                    case JSValueType.Null: return null;
                    case JSValueType.Bool: return !(V8Boolean == 0);
                    case JSValueType.BoolObject: return V8Boolean; // TODO: Test this.
                    case JSValueType.Int32: return (Int32)V8Integer;
                    case JSValueType.Number: return V8Number;
                    case JSValueType.NumberObject: return V8Number; // TODO: Test this.
                    default: return V8String != null ? Marshal.PtrToStringUni((IntPtr)V8String) : null;
                }
            }
        }

        // --------------------------------------------------------------------------------------------------------------------
    }

    // ========================================================================================================================

    /// <summary>
    /// NamedProperty[Getter|Setter] are used as interceptors on object.
    /// See ObjectTemplate::SetNamedPropertyHandler.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public unsafe delegate HandleProxy* ManagedNamedPropertyGetter(string propertyName, ref ManagedAccessorInfo info);

    /// <summary>
    /// Returns the value if the setter intercepts the request.
    /// Otherwise, returns an empty handle.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public unsafe delegate HandleProxy* ManagedNamedPropertySetter(string propertyName, HandleProxy* value, ref ManagedAccessorInfo info);

    /// <summary>
    /// Returns a non-empty value (>=0) if the interceptor intercepts the request.
    /// The result is an integer encoding property attributes (like v8::None,
    /// v8::DontEnum, etc.)
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public unsafe delegate V8PropertyAttributes ManagedNamedPropertyQuery(string propertyName, ref ManagedAccessorInfo info);

    /// <summary>
    /// Returns a value indicating if the deleter intercepts the request.
    /// The return value is true (>0) if the property could be deleted and false (0)
    /// otherwise.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public unsafe delegate int ManagedNamedPropertyDeleter(string propertyName, ref ManagedAccessorInfo info);

    /// <summary>
    /// Returns an array containing the names of the properties the named
    /// property getter intercepts.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public unsafe delegate HandleProxy* ManagedNamedPropertyEnumerator(ref ManagedAccessorInfo info);

    // ------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Returns the value of the property if the getter intercepts __stdcall
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public unsafe delegate HandleProxy* ManagedIndexedPropertyGetter(Int32 index, ref ManagedAccessorInfo info);

    /// <summary>
    /// Returns the value if the setter intercepts the request.
    /// Otherwise, returns an empty handle.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public unsafe delegate HandleProxy* ManagedIndexedPropertySetter(Int32 index, HandleProxy* value, ref ManagedAccessorInfo info);

    /// <summary>
    /// Returns a non-empty value (>=0) if the interceptor intercepts the request.
    /// The result is an integer encoding property attributes.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public unsafe delegate V8PropertyAttributes ManagedIndexedPropertyQuery(Int32 index, ref ManagedAccessorInfo info);

    /// <summary>
    /// Returns a value indicating if the deleter intercepts the request.
    /// The return value is true (>0) if the property could be deleted and false (0)
    /// otherwise.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public unsafe delegate int ManagedIndexedPropertyDeleter(Int32 index, ref ManagedAccessorInfo info);

    /// <summary>
    /// Returns an array containing the indices of the properties the
    /// indexed property getter intercepts.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public unsafe delegate HandleProxy* ManagedIndexedPropertyEnumerator(ref ManagedAccessorInfo info);

    // ========================================================================================================================

    /// <summary>
    /// Returns the value if the setter intercepts the request.
    /// Otherwise, returns an empty handle.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public unsafe delegate HandleProxy* ManagedAccessorSetter(HandleProxy* _this, string propertyName, HandleProxy* value);

    /// <summary>
    /// Returns the value if the setter intercepts the request.
    /// Otherwise, returns an empty handle.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public unsafe delegate HandleProxy* ManagedAccessorGetter(HandleProxy* _this, string propertyName);

    // ========================================================================================================================

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public unsafe delegate HandleProxy* ManagedJSFunctionCallback(Int32 managedObjectID, bool isConstructCall, HandleProxy* _this, HandleProxy** args, Int32 argCount);
    // ('IntPtr' == HandleProxy*)

    // ========================================================================================================================

    /// <summary>
    /// Intercepts a request to garbage collect a persistent V8 handle associated with the specified proxy handle.
    /// Return true to allow the collection, or false to prevent it.
    /// <para>Note: Internally, this is used to remove the strong reference to managed objects, leaving only a week one.</para>
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public unsafe delegate bool V8GarbageCollectionRequestCallback(HandleProxy* objectToBeCollected);

    // ========================================================================================================================
}
