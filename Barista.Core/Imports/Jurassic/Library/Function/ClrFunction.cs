namespace Barista.Jurassic.Library
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using Jurassic.Compiler;
  using Binder = Jurassic.Compiler.Binder;

  /// <summary>
  /// Represents a JavaScript function implemented by one or more .NET methods.
  /// </summary>
  [Serializable]
  public class ClrFunction : FunctionInstance
  {
    readonly object m_thisBinding;
    private readonly Binder m_callBinder;
    private readonly Binder m_constructBinder;


    //     INITIALIZATION
    //_________________________________________________________________________________________

    /// <summary>
    /// Creates a new instance of a built-in constructor function.
    /// </summary>
    /// <param name="prototype"> The next object in the prototype chain. </param>
    /// <param name="name"> The name of the function. </param>
    /// <param name="instancePrototype">  </param>
    protected ClrFunction(ObjectInstance prototype, string name, ObjectInstance instancePrototype)
      : base(prototype)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (instancePrototype == null)
        throw new ArgumentNullException("instancePrototype");

      // This is a constructor so ignore the "this" parameter when the function is called.
      m_thisBinding = this;

      // Search through every method in this type looking for [JSCallFunction] and [JSConstructorFunction] attributes.
      var callBinderMethods = new List<JSBinderMethod>(1);
      var constructBinderMethods = new List<JSBinderMethod>(1);
      var methods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
      foreach (var method in methods)
      {
        // Search for the [JSCallFunction] and [JSConstructorFunction] attributes.
        var callAttribute = (JSCallFunctionAttribute)Attribute.GetCustomAttribute(method, typeof(JSCallFunctionAttribute));
        var constructorAttribute = (JSConstructorFunctionAttribute)Attribute.GetCustomAttribute(method, typeof(JSConstructorFunctionAttribute));

        // Can't declare both attributes.
        if (callAttribute != null && constructorAttribute != null)
          throw new InvalidOperationException("Methods cannot be marked with both [JSCallFunction] and [JSConstructorFunction].");

        if (callAttribute != null)
        {
          // Method is marked with [JSCallFunction]
          callBinderMethods.Add(new JSBinderMethod(method, callAttribute.Flags));
        }
        else if (constructorAttribute != null)
        {
          var binderMethod = new JSBinderMethod(method, constructorAttribute.Flags);
          constructBinderMethods.Add(binderMethod);

          // Constructors must return ObjectInstance or a derived type.
          if (typeof(ObjectInstance).IsAssignableFrom(binderMethod.ReturnType) == false)
            throw new InvalidOperationException(string.Format("Constructors must return {0} (or a derived type).", typeof(ObjectInstance).Name));
        }
      }

      // Initialize the Call function.
      this.m_callBinder = callBinderMethods.Count > 0
        ? new JSBinder(callBinderMethods)
        : new JSBinder(new JSBinderMethod(new Func<object>(() => Undefined.Value).Method, JSFunctionFlags.None));

      // Initialize the Construct function.
      this.m_constructBinder = constructBinderMethods.Count > 0
        ? new JSBinder(constructBinderMethods)
        : new JSBinder(new JSBinderMethod(new Func<ObjectInstance>(() => this.Engine.Object.Construct()).Method, JSFunctionFlags.None));

      // Add function properties.
      this.FastSetProperty("name", name, PropertyAttributes.Sealed, false);
      this.FastSetProperty("length", this.m_callBinder.FunctionLength, PropertyAttributes.Sealed, false);
      this.FastSetProperty("prototype", instancePrototype, PropertyAttributes.Sealed, false);
      instancePrototype.FastSetProperty("constructor", this, PropertyAttributes.NonEnumerable, false);
    }

    /// <summary>
    /// Creates a new instance of a function which calls the given delegate.
    /// </summary>
    /// <param name="prototype"> The next object in the prototype chain. </param>
    /// <param name="delegateToCall"> The delegate to call. </param>
    /// <param name="name"> The name of the function.  Pass <c>null</c> to use the name of the
    /// delegate for the function name. </param>
    /// <param name="length"> The "typical" number of arguments expected by the function.  Pass
    /// <c>-1</c> to use the number of arguments expected by the delegate. </param>
    internal ClrFunction(ObjectInstance prototype, Delegate delegateToCall, string name, int length)
      : base(prototype)
    {
      // Initialize the [[Call]] method.
      this.m_callBinder = new JSBinder(new JSBinderMethod(delegateToCall.Method, JSFunctionFlags.None));

      // If the delegate has a class instance, use that to call the method.
      this.m_thisBinding = delegateToCall.Target;

      // Add function properties.
      this.FastSetProperty("name", name ?? this.m_callBinder.Name, PropertyAttributes.Sealed, false);
      this.FastSetProperty("length", length >= 0 ? length : this.m_callBinder.FunctionLength, PropertyAttributes.Sealed, false);
      //this.FastSetProperty("prototype", this.Engine.Object.Construct());
      //this.InstancePrototype.FastSetProperty("constructor", this, PropertyAttributes.NonEnumerable);
    }

    /// <summary>
    /// Creates a new instance of a function which calls one or more provided methods.
    /// </summary>
    /// <param name="prototype"> The next object in the prototype chain. </param>
    /// <param name="methods"> An enumerable collection of methods that logically comprise a
    /// single method group. </param>
    /// <param name="name"> The name of the function.  Pass <c>null</c> to use the name of the
    /// provided methods for the function name (in this case all the provided methods must have
    /// the same name). </param>
    /// <param name="length"> The "typical" number of arguments expected by the function.  Pass
    /// <c>-1</c> to use the maximum of arguments expected by any of the provided methods. </param>
    internal ClrFunction(ObjectInstance prototype, IEnumerable<JSBinderMethod> methods, string name, int length)
      : base(prototype)
    {
      this.m_callBinder = new JSBinder(methods);

      // Add function properties.
      this.FastSetProperty("name", name ?? this.m_callBinder.Name, PropertyAttributes.Sealed, false);
      this.FastSetProperty("length", length >= 0 ? length : this.m_callBinder.FunctionLength, PropertyAttributes.Sealed, false);
      //this.FastSetProperty("prototype", this.Engine.Object.Construct());
      //this.InstancePrototype.FastSetProperty("constructor", this, PropertyAttributes.NonEnumerable);
    }

    /// <summary>
    /// Creates a new instance of a function which calls the given binder.
    /// </summary>
    /// <param name="prototype"> The next object in the prototype chain. </param>
    /// <param name="binder"> An object representing a collection of methods to bind to. </param>
    internal ClrFunction(ObjectInstance prototype, Binder binder)
      : base(prototype)
    {
      this.m_callBinder = binder;

      // Add function properties.
      this.FastSetProperty("name", binder.Name, PropertyAttributes.Sealed, false);
      this.FastSetProperty("length", binder.FunctionLength, PropertyAttributes.Sealed, false);
      //this.FastSetProperty("prototype", this.Engine.Object.Construct());
      //this.InstancePrototype.FastSetProperty("constructor", this, PropertyAttributes.NonEnumerable);
    }



    //     OVERRIDES
    //_________________________________________________________________________________________

    /// <summary>
    /// Calls this function, passing in the given "this" value and zero or more arguments.
    /// </summary>
    /// <param name="thisObject"> The value of the "this" keyword within the function. </param>
    /// <param name="arguments"> An array of argument values. </param>
    /// <returns> The value that was returned from the function. </returns>
    public override object CallLateBound(object thisObject, params object[] arguments)
    {
      if (this.Engine.CompatibilityMode == CompatibilityMode.ECMAScript3)
      {
        // Convert null or undefined to the global object.
        if (TypeUtilities.IsUndefined(thisObject) || thisObject == Null.Value)
          thisObject = this.Engine.Global;
        else
          thisObject = TypeConverter.ToObject(this.Engine, thisObject);
      }
      try
      {
        return this.m_callBinder.Call(this.Engine, m_thisBinding ?? thisObject, arguments);
      }
      catch (JavaScriptException ex)
      {
        if (ex.FunctionName == null && ex.SourcePath == null && ex.LineNumber == 0)
        {
          ex.FunctionName = this.DisplayName;
          ex.SourcePath = "native";
          ex.PopulateStackTrace();
        }
        throw;
      }
    }

    /// <summary>
    /// Creates an object, using this function as the constructor.
    /// </summary>
    /// <param name="argumentValues"> An array of argument values. </param>
    /// <returns> The object that was created. </returns>
    public override ObjectInstance ConstructLateBound(params object[] argumentValues)
    {
      if (this.m_constructBinder == null)
        throw new JavaScriptException(this.Engine, "TypeError", "Objects cannot be constructed from built-in functions");
      return (ObjectInstance)this.m_constructBinder.Call(this.Engine, this, argumentValues);
    }

    /// <summary>
    /// Returns a string representing this object.
    /// </summary>
    /// <returns> A string representing this object. </returns>
    public override string ToString()
    {
      return string.Format("function {0}() {{ [native code] }}", this.Name);
    }

    ///// <summary>
    ///// Creates a delegate that does type conversion and calls the method represented by this
    ///// object.
    ///// </summary>
    ///// <param name="argumentTypes"> The types of the arguments that will be passed to the delegate. </param>
    ///// <returns> A delegate that does type conversion and calls the method represented by this
    ///// object. </returns>
    //internal BinderDelegate CreateBinder<T>()
    //{
    //    // Delegate types have an Invoke method containing the relevant parameters.
    //    MethodInfo adapterInvokeMethod = typeof(T).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
    //    if (adapterInvokeMethod == null)
    //        throw new ArgumentException("The type parameter T must be delegate type.", "T");

    //    // Get the argument types.
    //    Type[] argumentTypes = adapterInvokeMethod.GetParameters().Select(p => p.ParameterType).ToArray();

    //    // Create the binder.
    //    return this.callBinder.CreateBinder(argumentTypes);
    //}
  }
}
