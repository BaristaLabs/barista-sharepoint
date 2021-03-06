﻿namespace Barista.Jurassic.Library
{
    using System;

    /// <summary>
    /// Represents a JavaScript function.
    /// </summary>
    [Serializable]
    public abstract class FunctionInstance : ObjectInstance
    {
        // Used to speed up access to the prototype property.
        private int m_cachedInstancePrototypeIndex;
        private object m_cachedInstancePrototypeSchema;


        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new instance of a built-in function object, with the default Function
        /// prototype.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        protected FunctionInstance(ScriptEngine engine)
            : base(engine, engine.Function.InstancePrototype)
        {
        }

        /// <summary>
        /// Creates a new instance of a built-in function object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        protected FunctionInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        /// <summary>
        /// Creates a new instance of a built-in function object.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        /// <param name="prototype"> The next object in the prototype chain.  Can be <c>null</c>. </param>
        protected FunctionInstance(ScriptEngine engine, ObjectInstance prototype)
            : base(engine, prototype)
        {
        }



        //     .NET ACCESSOR PROPERTIES
        //_________________________________________________________________________________________

        /// <summary>
        /// Gets the internal class name of the object.  Used by the default toString()
        /// implementation.
        /// </summary>
        protected override string InternalClassName
        {
            get { return "Function"; }
        }

        /// <summary>
        /// Gets the prototype of objects constructed using this function.  Equivalent to
        /// the Function.prototype property.
        /// </summary>
        public ObjectInstance InstancePrototype
        {
            get
            {
                // See 13.2.2

                // Retrieve the value of the prototype property.
                //var prototype = this["prototype"] as ObjectInstance;
                ObjectInstance prototype;
                if (m_cachedInstancePrototypeSchema == InlineCacheKey)
                    prototype = InlinePropertyValues[m_cachedInstancePrototypeIndex] as ObjectInstance;
                else
                    prototype = InlineGetPropertyValue("prototype", out m_cachedInstancePrototypeIndex, out m_cachedInstancePrototypeSchema) as ObjectInstance;

                // If the prototype property is not set to an object, use the Object prototype property instead.
                if (prototype == null && this != Engine.Object)
                    return Engine.Object.InstancePrototype;

                return prototype;
            }
        }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string Name
        {
            get { return TypeConverter.ToString(this["name"]); }
        }

        /// <summary>
        /// Gets the display name of the function.  This is equal to the displayName property, if
        /// it exists, or the name property otherwise.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (HasProperty("displayName"))
                    return TypeConverter.ToString(this["displayName"]);
                var name = TypeConverter.ToString(this["name"]);
                return name == string.Empty
                    ? "[Anonymous]"
                    : name;
            }
        }

        /// <summary>
        /// Gets the number of arguments expected by the function.
        /// </summary>
        public int Length
        {
            get { return TypeConverter.ToInteger(this["length"]); }
            protected set { FastSetProperty("length", value, PropertyAttributes.Sealed, false); }
        }



        //     JAVASCRIPT INTERNAL FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Determines whether the given object inherits from this function.  More precisely, it
        /// checks whether the prototype chain of the object contains the prototype property of
        /// this function.  Used by the "instanceof" operator.
        /// </summary>
        /// <param name="instance"> The instance to check. </param>
        /// <returns> <c>true</c> if the object inherits from this function; <c>false</c>
        /// otherwise. </returns>
        public virtual bool HasInstance(object instance)
        {
            if ((instance is ObjectInstance) == false)
                return false;
            object functionPrototype = this["prototype"];
            if ((functionPrototype is ObjectInstance) == false)
                throw new JavaScriptException(Engine, "TypeError", "Function has non-object prototype in instanceof check");
            var instancePrototype = ((ObjectInstance)instance).Prototype;
            while (instancePrototype != null)
            {
                if (instancePrototype == functionPrototype)
                    return true;
                instancePrototype = instancePrototype.Prototype;
            }
            return false;
        }

        /// <summary>
        /// Calls this function, passing in the given "this" value and zero or more arguments.
        /// </summary>
        /// <param name="thisObject"> The value of the "this" keyword within the function. </param>
        /// <param name="argumentValues"> An array of argument values. </param>
        /// <returns> The value that was returned from the function. </returns>
        public abstract object CallLateBound(object thisObject, params object[] argumentValues);

        /// <summary>
        /// Calls this function, passing in the given "this" value and zero or more arguments.
        /// </summary>
        /// <param name="function"> The name of the caller function. </param>
        /// <param name="thisObject"> The value of the "this" keyword within the function. </param>
        /// <param name="argumentValues"> An array of argument values. </param>
        /// <returns> The value that was returned from the function. </returns>
        internal object CallFromNative(string function, object thisObject, params object[] argumentValues)
        {
            Engine.PushStackFrame("native", function, 0);
            try
            {
                return CallLateBound(thisObject, argumentValues);
            }
            finally
            {
                Engine.PopStackFrame();
            }
        }

        /// <summary>
        /// Calls this function, passing in the given "this" value and zero or more arguments.
        /// </summary>
        /// <param name="path"> The path of the javascript source file that contains the caller. </param>
        /// <param name="function"> The name of the caller function. </param>
        /// <param name="line"> The line number of the statement that is calling this function. </param>
        /// <param name="thisObject"> The value of the "this" keyword within the function. </param>
        /// <param name="argumentValues"> An array of argument values. </param>
        /// <returns> The value that was returned from the function. </returns>
        public object CallWithStackTrace(string path, string function, int line, object thisObject, object[] argumentValues)
        {
            Engine.PushStackFrame(path, function, line);
            try
            {
                return CallLateBound(thisObject, argumentValues);
            }
            finally
            {
                Engine.PopStackFrame();
            }
        }

        /// <summary>
        /// Creates an object, using this function as the constructor.
        /// </summary>
        /// <param name="argumentValues"> An array of argument values. </param>
        /// <returns> The object that was created. </returns>
        public virtual ObjectInstance ConstructLateBound(params object[] argumentValues)
        {
            // Create a new object and set the prototype to the instance prototype of the function.
            var newObject = CreateRawObject(InstancePrototype);

            // Run the function, with the new object as the "this" keyword.
            var result = CallLateBound(newObject, argumentValues);
            var bound = result as ObjectInstance;

            //Return the result of the function if it is an object, otherwise, return the new object.
            return bound ?? newObject;
        }

        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Calls the function, passing in parameters from the given array.
        /// </summary>
        /// <param name="thisObj"> The value of <c>this</c> in the context of the function. </param>
        /// <param name="arguments"> The arguments passed to the function, as an array. </param>
        /// <returns> The result from the function call. </returns>
        [JSInternalFunction(Name = "apply")]
        public object Apply(object thisObj, object arguments)
        {
            // Convert the arguments parameter into an array.
            object[] argumentsArray;
            if (arguments == null || arguments == Undefined.Value || arguments == Null.Value)
                argumentsArray = new object[0];
            else
            {
                if ((arguments is ObjectInstance) == false)
                    throw new JavaScriptException(Engine, "TypeError", "The second parameter of apply() must be an array or an array-like object.");
                ObjectInstance argumentsObject = (ObjectInstance)arguments;
                object arrayLengthObj = argumentsObject["length"];
                if (arrayLengthObj == null || arrayLengthObj == Undefined.Value || arrayLengthObj == Null.Value)
                    throw new JavaScriptException(Engine, "TypeError", "The second parameter of apply() must be an array or an array-like object.");
                uint arrayLength = TypeConverter.ToUint32(arrayLengthObj);
                if (Math.Abs(arrayLength - TypeConverter.ToNumber(arrayLengthObj)) > double.Epsilon)
                    throw new JavaScriptException(Engine, "TypeError", "The second parameter of apply() must be an array or an array-like object.");
                argumentsArray = new object[arrayLength];
                for (uint i = 0; i < arrayLength; i++)
                    argumentsArray[i] = argumentsObject[i];
            }

            return CallLateBound(thisObj, argumentsArray);
        }

        /// <summary>
        /// Calls the function.
        /// </summary>
        /// <param name="thisObj"> The value of <c>this</c> in the context of the function. </param>
        /// <param name="arguments"> Any number of arguments that will be passed to the function. </param>
        /// <returns> The result from the function call. </returns>
        [JSInternalFunction(Name = "call", Length = 1)]
        public object Call(object thisObj, params object[] arguments)
        {
            return CallLateBound(thisObj, arguments);
        }

        /// <summary>
        /// Creates a new function that, when called, calls this function with the given "this"
        /// value and, optionally, one or more more arguments.
        /// </summary>
        /// <param name="boundThis"> The fixed value of "this". </param>
        /// <param name="boundArguments"> Any number of fixed arguments values. </param>
        /// <returns> A new function. </returns>
        [JSInternalFunction(Name = "bind", Length = 1)]
        public FunctionInstance Bind(object boundThis, params object[] boundArguments)
        {
            return new BoundFunction(this, boundThis, boundArguments);
        }

        /// <summary>
        /// Returns a string representing this object.
        /// </summary>
        /// <returns> A string representing this object. </returns>
        [JSInternalFunction(Name = "toString")]
        public string ToStringJS()
        {
            return ToString();
        }

        /// <summary>
        /// Returns a string representing this object.
        /// </summary>
        /// <returns> A string representing this object. </returns>
        public override string ToString()
        {
            return string.Format("function {0}() {{ [native code] }}", Name);
        }
    }
}
