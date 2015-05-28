namespace Barista.Jurassic.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the built-in javascript Array object.
    /// </summary>
    [Serializable]
    public class ArrayConstructor : ClrFunction
    {

        //     INITIALIZATION
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Array object.
        /// </summary>
        /// <param name="prototype"> The next object in the prototype chain. </param>
        internal ArrayConstructor(ObjectInstance prototype)
            : base(prototype, "Array", new ArrayInstance(prototype.Engine.Object.InstancePrototype, 0, 0))
        {
        }


        /// <summary>
        /// Creates a new Array instance.
        /// </summary>
        public ArrayInstance New()
        {
            return new ArrayInstance(this.InstancePrototype, 0, 10);
        }

        /// <summary>
        /// Creates a new Array instance.
        /// </summary>
        /// <param name="elements"> The initial elements of the new array. </param>
        public ArrayInstance New(object[] elements)
        {
            // Copy the array if it is not an object array (for example, if it is a string[]).
            if (elements.GetType() != typeof(object[]))
            {
                var temp = new object[elements.Length];
                Array.Copy(elements, temp, elements.Length);
                return new ArrayInstance(this.InstancePrototype, elements);
            }

            return new ArrayInstance(this.InstancePrototype, elements);
        }



        //     JAVASCRIPT INTERNAL FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Creates a new Array instance and initializes the contents of the array.
        /// Called when the Array object is invoked like a function, e.g. var x = Array(length).
        /// </summary>
        /// <param name="elements"> The initial elements of the new array. </param>
        [JSCallFunction]
        public ArrayInstance Call(params object[] elements)
        {
            return Construct(elements);
        }

        public ArrayInstance Construct(IList<object> elements)
        {
            if (elements.Count == 1)
            {
                if (elements[0] is double)
                {
                    var length = (double)elements[0];
                    var length32 = TypeConverter.ToUint32(length);
                    if (Double.IsNaN(length) || Math.Abs(length - length32) > double.Epsilon)
                        throw new JavaScriptException(this.Engine, "RangeError", "Invalid array length");
                    return new ArrayInstance(this.InstancePrototype, length32, length32);
                }

                if (elements[0] is int)
                {
                    var length = (int)elements[0];
                    if (length < 0)
                        throw new JavaScriptException(this.Engine, "RangeError", "Invalid array length");
                    return new ArrayInstance(this.InstancePrototype, (uint)length, (uint)length);
                }
            }

            // Transform any nulls into undefined.
            for (int i = 0; i < elements.Count; i++)
                if (elements[i] == null)
                    elements[i] = Undefined.Value;

            return New(elements.ToArray());
        }

        /// <summary>
        /// Creates a new Array instance and initializes the contents of the array.
        /// Called when the new expression is used on this object, e.g. var x = new Array(length).
        /// </summary>
        /// <param name="elements"> The initial elements of the new array. </param>
        [JSConstructorFunction]
        public ArrayInstance Construct(params object[] elements)
        {
            if (elements.Length == 1)
            {
                if (elements[0] is double)
                {
                    var length = (double)elements[0];
                    var length32 = TypeConverter.ToUint32(length);
                    if (Double.IsNaN(length) || Math.Abs(length - length32) > double.Epsilon)
                        throw new JavaScriptException(this.Engine, "RangeError", "Invalid array length");
                    return new ArrayInstance(this.InstancePrototype, length32, length32);
                }

                if (elements[0] is int)
                {
                    var length = (int)elements[0];
                    if (length < 0)
                        throw new JavaScriptException(this.Engine, "RangeError", "Invalid array length");
                    return new ArrayInstance(this.InstancePrototype, (uint)length, (uint)length);
                }

                if (elements[0] is uint)
                {
                    var length = (uint)elements[0];
                    return new ArrayInstance(this.InstancePrototype, length, length);
                }
            }

            // Transform any nulls into undefined.
            for (int i = 0; i < elements.Length; i++)
                if (elements[i] == null)
                    elements[i] = Undefined.Value;

            return New(elements);
        }



        //     JAVASCRIPT FUNCTIONS
        //_________________________________________________________________________________________

        /// <summary>
        /// Tests if the given value is an Array instance.
        /// </summary>
        /// <param name="value"> The value to test. </param>
        /// <returns> <c>true</c> if the given value is an Array instance, <c>false</c> otherwise. </returns>
        [JSInternalFunction(Name = "isArray")]
        public static bool IsArray(object value)
        {
            return value is ArrayInstance;
        }

    }
}
