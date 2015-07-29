namespace Barista.Jurassic.Library
{
    using System;

    /// <summary>
    /// Represents a JavaScript function that throws a type error.
    /// </summary>
    [Serializable]
    internal sealed class ThrowTypeErrorFunction : FunctionInstance
    {
        private readonly string m_message;

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new ThrowTypeErrorFunction instance.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        internal ThrowTypeErrorFunction(ObjectInstance prototype)
            : this(prototype, "It is illegal to access the 'callee' or 'caller' property in strict mode")
        {
        }

        /// <summary>
        /// Creates a new ThrowTypeErrorFunction instance.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        /// <param name="message"> The TypeError message. </param>
        internal ThrowTypeErrorFunction(ObjectInstance prototype, string message)
            : base(prototype)
        {
            FastSetProperty("name", "ThrowTypeError", PropertyAttributes.Sealed, false);
            FastSetProperty("length", 0, PropertyAttributes.Sealed, false);
            IsExtensible = false;
            m_message = message;
        }


        //     OVERRIDES
        //_________________________________________________________________________________________

        /// <summary>
        /// Calls this function, passing in the given "this" value and zero or more arguments.
        /// </summary>
        /// <param name="thisObject">The value of the "this" keyword within the function. </param>
        /// <param name="argumentValues">An array of argument values to pass to the function. </param>
        /// <returns> The value that was returned from the function. </returns>
        public override object CallLateBound(object thisObject, params object[] argumentValues)
        {
            throw new JavaScriptException(Engine, "TypeError", m_message);
        }
    }
}
