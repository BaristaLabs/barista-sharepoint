namespace Barista.Jurassic.Library
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Barista.Newtonsoft.Json;
    using Barista.Newtonsoft.Json.Linq;

    /// <summary>
    /// Converts a value into JSON text.
    /// </summary>
    internal sealed class JSONSerializer
    {
        private readonly ScriptEngine m_engine;
        private Stack<ObjectInstance> m_objectStack;
        private Stack<ArrayInstance> m_arrayStack;
        private string m_separator;

        /// <summary>
        /// Creates a new JSONSerializer instance with the default options.
        /// </summary>
        /// <param name="engine"> The associated script engine. </param>
        public JSONSerializer(ScriptEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            this.m_engine = engine;
        }

        /// <summary>
        /// Gets or sets a function which can transform values before they are serialized.
        /// </summary>
        public FunctionInstance ReplacerFunction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a string to use for indentation.
        /// </summary>
        public string Indentation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of property names to be serialized.
        /// </summary>
        public ICollection<string> SerializableProperties
        {
            get;
            set;
        }


        /// <summary>
        /// Serializes a value into a JSON string.
        /// </summary>
        /// <param name="value"> The value to serialize. </param>
        /// <returns> The JSON repesentation of the value. </returns>
        public string Serialize(object value)
        {
            // Initialize private variables.
            this.m_objectStack = new Stack<ObjectInstance>();
            this.m_arrayStack = new Stack<ArrayInstance>();
            this.m_separator = string.IsNullOrEmpty(this.Indentation) ? string.Empty : "\n";

            // Create a temp object to hold the value.
            var tempObject = this.m_engine.Object.Construct();
            tempObject[string.Empty] = value;

            // Transform the value.
            value = TransformPropertyValue(tempObject, string.Empty);
            if (value == null || value == Undefined.Value || value is FunctionInstance)
                return null;

            // Serialize the value.
            var result = new StringBuilder(100);
            SerializePropertyValue(value, result);
            return result.ToString();
        }

        /// <summary>
        /// Transforms the value stored in the given object using toJSON and/or the replacer function.
        /// </summary>
        /// <param name="holder"> The object containing the value. </param>
        /// <param name="propertyName"> The name of the property holding the value to transform. </param>
        /// <returns> The transformed value. </returns>
        private object TransformPropertyValue(ObjectInstance holder, string propertyName)
        {
            var value = holder[propertyName];

            // Transform the value by calling toJSON(), if the method exists on the object.
            if (value is ObjectInstance)
            {
                object toJsonResult;
                if (((ObjectInstance)value).TryCallMemberFunction(out toJsonResult, "toJSON", propertyName))
                    value = toJsonResult;
            }

            // Transform the value by calling the replacer function, if one was provided.
            if (this.ReplacerFunction != null)
                value = this.ReplacerFunction.CallFromNative("stringify", holder, propertyName, value);

            return value;
        }

        /// <summary>
        /// Transforms the value stored in the given object using toJSON and/or the replacer function.
        /// </summary>
        /// <param name="holder"> The object containing the value. </param>
        /// <param name="arrayIndex"> The array index of the property holding the value to transform. </param>
        /// <returns> The transformed value. </returns>
        private object TransformPropertyValue(ArrayInstance holder, uint arrayIndex)
        {
            string propertyName = null;
            var value = holder[arrayIndex];

            // Transform the value by calling toJSON(), if the method exists on the object.
            if (value is ObjectInstance)
            {
                propertyName = arrayIndex.ToString(CultureInfo.InvariantCulture);
                object toJsonResult;
                if (((ObjectInstance)value).TryCallMemberFunction(out toJsonResult, "toJSON", propertyName))
                    value = toJsonResult;
            }

            // Transform the value by calling the replacer function, if one was provided.
            if (this.ReplacerFunction == null)
                return value;

            if (propertyName == null)
                propertyName = arrayIndex.ToString(CultureInfo.InvariantCulture);
            value = this.ReplacerFunction.CallFromNative("stringify", holder, propertyName, value);

            return value;
        }

        /// <summary>
        /// Serializes a value into a JSON string.  Does not serialize "undefined", check for that
        /// value before calling this method.
        /// </summary>
        /// <param name="value"> The value to serialize. </param>
        /// <param name="result"> The StringBuilder to write the JSON representation of the
        /// value to. </param>
        private void SerializePropertyValue(object value, StringBuilder result)
        {
            // Transform boolean, numeric and string objects into their primitive equivalents.
            if (value is NumberInstance)
                value = ((NumberInstance)value).Value;
            else if (value is StringInstance)
                value = ((StringInstance)value).Value;
            else if (value is BooleanInstance)
                value = ((BooleanInstance)value).Value;

            // Serialize a null value.
            if (value == Null.Value)
            {
                result.Append("null");
                return;
            }

            // Serialize a boolean value.
            if (value is bool)
            {
                result.Append((bool)value == false
                  ? "false"
                  : "true");
                return;
            }

            // Serialize a string value.
            if (value is string || value is ConcatenatedString)
            {
                QuoteString(value.ToString(), result);
                return;
            }

            // Serialize a numeric value.
            if (value is double)
            {
                if (double.IsInfinity((double)value) || double.IsNaN((double)value))
                    result.Append("null");
                else
                    result.Append(NumberFormatter.ToString((double)value, 10, NumberFormatter.Style.Regular, 0));
                return;
            }

            if (value is SByte)
            {
                result.Append(((SByte)value).ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is Int16)
            {
                result.Append(((Int16)value).ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is Int32)
            {
                result.Append(((Int32)value).ToString(CultureInfo.InvariantCulture));
                return;
            }

            if ((value is Int64))
            {
                result.Append(((Int64)value).ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is Byte)
            {
                result.Append(((Byte)value).ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is UInt16)
            {
                result.Append(((UInt16)value).ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is UInt32)
            {
                result.Append(((UInt32)value).ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is UInt64)
            {
                result.Append(((UInt64)value).ToString(CultureInfo.InvariantCulture));
                return;
            }

            // Serialize an array.
            if (value is ArrayInstance)
            {
                SerializeArray((ArrayInstance)value, result);
                return;
            }

            // Serialize an object.
            if (value is ObjectInstance && (value is FunctionInstance) == false)
            {
                SerializeObject((ObjectInstance)value, result);
                return;
            }

            if (value is JToken)
            {
                var formatting = string.IsNullOrEmpty(this.Indentation) ? Formatting.None : Formatting.Indented;
                result.Append(JsonConvert.SerializeObject(value, formatting));
                return;
            }

            // The value is of a type we cannot serialize.
            throw new NotSupportedException(string.Format("Unsupported value type: {0}", value.GetType()));
        }

        /// <summary>
        /// Adds double quote characters to the start and end of the given string and converts any
        /// invalid characters into escape sequences.
        /// </summary>
        /// <param name="input"> The string to quote. </param>
        /// <param name="result"> The StringBuilder to write the quoted string to. </param>
        private static void QuoteString(string input, System.Text.StringBuilder result)
        {
            result.Append('\"');

            // Check if there are characters that need to be escaped.
            // These characters include '"', '\' and any character with an ASCII value less than 32.
            bool containsUnsafeCharacters = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '\\' || c == '\"' || c < 0x20)
                {
                    containsUnsafeCharacters = true;
                    break;
                }
            }

            if (containsUnsafeCharacters == false)
            {
                // The string does not contain escape characters.
                result.Append(input);
            }
            else
            {
                // The string contains escape characters - fall back to the slower code path.
                foreach (char c in input)
                {
                    switch (c)
                    {
                        case '\"':
                        case '\\':
                            result.Append('\\');
                            result.Append(c);
                            break;
                        case '\b':
                            result.Append("\\b");
                            break;
                        case '\f':
                            result.Append("\\f");
                            break;
                        case '\n':
                            result.Append("\\n");
                            break;
                        case '\r':
                            result.Append("\\r");
                            break;
                        case '\t':
                            result.Append("\\t");
                            break;
                        default:
                            if (c < 0x20)
                            {
                                result.Append('\\');
                                result.Append('u');
                                result.Append(((int)c).ToString("x4"));
                            }
                            else
                                result.Append(c);
                            break;
                    }
                }
            }
            result.Append('\"');
        }

        /// <summary>
        /// Serializes an object into a JSON string.
        /// </summary>
        /// <param name="value"> The object to serialize. </param>
        /// <param name="result"> The StringBuilder to write the JSON representation of the
        /// object to. </param>
        private void SerializeObject(ObjectInstance value, StringBuilder result)
        {
            // Add the spacer string to the current separator string.
            string previousSeparator = this.m_separator;
            this.m_separator += this.Indentation;

            // Check for cyclical references.
            if (this.m_objectStack.Contains(value))
                throw new JavaScriptException(this.m_engine, "TypeError", "The given object must not contain cyclical references");
            this.m_objectStack.Push(value);

            // Create a list of property names to serialize.
            var propertiesToSerialize = this.SerializableProperties;
            if (propertiesToSerialize == null)
            {
                propertiesToSerialize = new List<string>();
                foreach (var property in value.Properties)
                    if (property.IsEnumerable)
                        propertiesToSerialize.Add(property.Name);
            }

            result.Append('{');
            int serializedPropertyCount = 0;
            foreach (string propertyName in propertiesToSerialize)
            {
                // Transform the value using the replacer function or toJSON().
                object propertyValue = TransformPropertyValue(value, propertyName);

                // Undefined values are not serialized.
                if (propertyValue == null || propertyValue == Undefined.Value || propertyValue is FunctionInstance)
                    continue;

                // Append the separator.
                if (serializedPropertyCount > 0)
                    result.Append(',');
                result.Append(this.m_separator);

                // Append the property name and value to the result.
                QuoteString(propertyName, result);
                result.Append(':');
                if (string.IsNullOrEmpty(this.Indentation) == false)
                    result.Append(' ');
                SerializePropertyValue(propertyValue, result);

                // Keep track of how many properties we have serialized.
                serializedPropertyCount++;
            }
            if (serializedPropertyCount > 0)
                result.Append(previousSeparator);
            result.Append('}');

            // Remove this object from the stack.
            this.m_objectStack.Pop();

            // Restore the separator to it's previous value.
            this.m_separator = previousSeparator;
        }

        /// <summary>
        /// Serializes an array into a JSON string.
        /// </summary>
        /// <param name="value"> The array to serialize. </param>
        /// <param name="result"> The StringBuilder to write the JSON representation of the
        /// array to. </param>
        private void SerializeArray(ArrayInstance value, StringBuilder result)
        {
            // Add the spacer string to the current separator string.
            var previousSeparator = this.m_separator;
            this.m_separator += this.Indentation;

            // Check for cyclical references.
            if (this.m_arrayStack.Contains(value))
                throw new JavaScriptException(this.m_engine, "TypeError", "The given object must not contain cyclical references");
            this.m_arrayStack.Push(value);

            result.Append('[');
            for (uint i = 0; i < value.Length; i++)
            {
                // Append the separator.
                if (i > 0)
                    result.Append(',');
                result.Append(this.m_separator);

                // Transform the value using the replacer function or toJSON().
                var elementValue = TransformPropertyValue(value, i);

                if (elementValue == null || elementValue == Undefined.Value || elementValue is FunctionInstance)
                {
                    // Undefined is serialized as "null".
                    result.Append("null");
                }
                else
                {
                    // Serialize the element value to the output.
                    SerializePropertyValue(elementValue, result);
                }
            }
            if (value.Length > 0)
                result.Append(previousSeparator);
            result.Append(']');

            // Remove this object from the stack.
            this.m_arrayStack.Pop();

            // Restore the separator to it's previous value.
            this.m_separator = previousSeparator;
        }
    }

}