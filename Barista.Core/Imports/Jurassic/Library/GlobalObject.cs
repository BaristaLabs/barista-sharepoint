namespace Jurassic.Library
{
  using System;
  using System.Text;
  using Barista;

  /// <summary>
  /// Represents functions and properties within the global scope.
  /// </summary>
  [Serializable]
  public class GlobalObject : ObjectInstance
  {

    //     INITIALIZATION
    //_________________________________________________________________________________________

    /// <summary>
    /// Creates a new Global object.
    /// </summary>
    /// <param name="prototype"> The next object in the prototype chain. </param>
    internal GlobalObject(ObjectInstance prototype)
      : base(prototype)
    {
      // Add the global constants.
      // Infinity, NaN and undefined are read-only in ECMAScript 5.
      this.FastSetProperty("Infinity", double.PositiveInfinity, PropertyAttributes.Sealed, false);
      this.FastSetProperty("NaN", double.NaN, PropertyAttributes.Sealed, false);
      this.FastSetProperty("undefined", Undefined.Value, PropertyAttributes.Sealed, false);
    }



    //     .NET ACCESSOR PROPERTIES
    //_________________________________________________________________________________________

    /// <summary>
    /// Gets the internal class name of the object.  Used by the default toString()
    /// implementation.
    /// </summary>
    protected override string InternalClassName
    {
      get { return "Global"; }
    }


    //     JAVASCRIPT FUNCTIONS
    //_________________________________________________________________________________________

    private static bool[] s_decodeUriReservedSet;
    private static bool[] s_decodeUriComponentReservedSet;
    private static bool[] s_encodeUriUnescapedSet;
    private static bool[] s_encodeUriComponentUnescapedSet;

    /// <summary>
    /// Decodes a string that was encoded with the encodeURI function.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="input"> The associated script engine. </param>
    /// <returns> The string, as it was before encoding. </returns>
    [JSInternalFunction(Name = "decodeURI", Flags = JSFunctionFlags.HasEngineParameter)]
    public static string DecodeUri(ScriptEngine engine, string input)
    {
      if (s_decodeUriReservedSet == null)
      {
        var lookupTable = CreateCharacterSetLookupTable(";/?:@&=+$,#");
        System.Threading.Thread.MemoryBarrier();
        s_decodeUriReservedSet = lookupTable;
      }
      return Decode(engine, input, s_decodeUriReservedSet);
    }

    /// <summary>
    /// Decodes a string that was encoded with the decodeURIComponent function.
    /// </summary>
    /// <param name="engine"> The associated script engine. </param>
    /// <param name="input"> The string to decode. </param>
    /// <returns> The string, as it was before encoding. </returns>
    [JSInternalFunction(Name = "decodeURIComponent", Flags = JSFunctionFlags.HasEngineParameter)]
    public static string DecodeUriComponent(ScriptEngine engine, string input)
    {
      if (s_decodeUriComponentReservedSet == null)
      {
        var lookupTable = CreateCharacterSetLookupTable("");
        System.Threading.Thread.MemoryBarrier();
        s_decodeUriComponentReservedSet = lookupTable;
      }
      return Decode(engine, input, s_decodeUriComponentReservedSet);
    }

    /// <summary>
    /// Encodes a string containing a Uniform Resource Identifier (URI).
    /// </summary>
    /// <param name="engine"> The associated script engine. </param>
    /// <param name="input"> The string to encode. </param>
    /// <returns> A copy of the given URI with the special characters encoded. </returns>
    [JSInternalFunction(Name = "encodeURI", Flags = JSFunctionFlags.HasEngineParameter)]
    public static string EncodeUri(ScriptEngine engine, string input)
    {
      if (s_encodeUriUnescapedSet == null)
      {
        var lookupTable = CreateCharacterSetLookupTable(";/?:@&=+$,-_.!~*'()#ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
        System.Threading.Thread.MemoryBarrier();
        s_encodeUriUnescapedSet = lookupTable;
      }
      return Encode(engine, input, s_encodeUriUnescapedSet);
    }

    /// <summary>
    /// Encodes a string containing a portion of a Uniform Resource Identifier (URI).
    /// </summary>
    /// <param name="engine"> The associated script engine. </param>
    /// <param name="input"> The string to encode. </param>
    /// <returns> A copy of the given URI with the special characters encoded. </returns>
    [JSInternalFunction(Name = "encodeURIComponent", Flags = JSFunctionFlags.HasEngineParameter)]
    public static string EncodeUriComponent(ScriptEngine engine, string input)
    {
      if (s_encodeUriComponentUnescapedSet == null)
      {
        var lookupTable = CreateCharacterSetLookupTable("-_.!~*'()ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
        System.Threading.Thread.MemoryBarrier();
        s_encodeUriComponentUnescapedSet = lookupTable;
      }
      return Encode(engine, input, s_encodeUriComponentUnescapedSet);
    }

    /// <summary>
    /// Encodes a string using an encoding similar to that used in URLs.
    /// </summary>
    /// <param name="input"> The string to encode. </param>
    /// <returns> A copy of the given string with the special characters encoded. </returns>
    [JSInternalFunction(Deprecated = true, Name = "escape")]
    public static string Escape(string input)
    {
      var result = new StringBuilder(input.Length);
      for (int i = 0; i < input.Length; i++)
      {
        char c = input[i];
        if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') ||
            c == '@' || c == '*' || c == '_' || c == '+' || c == '-' || c == '.' || c == '/')
          result.Append(c);
        else if (c < 256)
          result.AppendFormat("%{0:X2}", (int)c);
        else
          result.AppendFormat("%u{0:X4}", (int)c);
      }
      return result.ToString();
    }

    /// <summary>
    /// Evaluates the given javascript source code and returns the result.
    /// </summary>
    /// <param name="engine"> The associated script engine. </param>
    /// <param name="code"> The source code to evaluate. </param>
    /// <returns> The value of the last statement that was executed, or <c>undefined</c> if
    /// there were no executed statements. </returns>
    [JSInternalFunction(Name = "eval", Flags = JSFunctionFlags.HasEngineParameter)]
    public static object Eval(ScriptEngine engine, object code)
    {
      if (TypeUtilities.IsString(code) == false)
        return code;
      return engine.Eval(TypeConverter.ToString(code), engine.CreateGlobalScope(), engine.Global, false);
    }

    /// <summary>
    /// Evaluates the given javascript source code and returns the result.
    /// </summary>
    /// <param name="engine"> The associated script engine. </param>
    /// <param name="code"> The source code to evaluate. </param>
    /// <param name="scope"> The containing scope. </param>
    /// <param name="thisObject"> The value of the "this" keyword in the containing scope. </param>
    /// <param name="strictMode"> Indicates whether the eval statement is being called from
    /// strict mode code. </param>
    /// <returns> The value of the last statement that was executed, or <c>undefined</c> if
    /// there were no executed statements. </returns>
    public static object Eval(ScriptEngine engine, object code, Compiler.Scope scope, object thisObject, bool strictMode)
    {
      if (scope == null)
        throw new ArgumentNullException("scope");
      if (TypeUtilities.IsString(code) == false)
        return code;
      return engine.Eval(TypeConverter.ToString(code), scope, thisObject, strictMode);
    }

    /// <summary>
    /// Determines whether the given number is finite.
    /// </summary>
    /// <param name="value"> The number to test. </param>
    /// <returns> <c>false</c> if the number is NaN or positive or negative infinity,
    /// <c>true</c> otherwise. </returns>
    [JSInternalFunction(Name = "isFinite")]
    public static bool IsFinite(double value)
    {
      return double.IsNaN(value) == false && double.IsInfinity(value) == false;
    }

    /// <summary>
    /// Determines whether the given number is NaN.
    /// </summary>
    /// <param name="value"> The number to test. </param>
    /// <returns> <c>true</c> if the number is NaN, <c>false</c> otherwise. </returns>
    [JSInternalFunction(Name = "isNaN")]
    public static bool IsNaN(double value)
    {
      return double.IsNaN(value);
    }

    /// <summary>
    /// Parses the given string and returns the equivalent numeric value. 
    /// </summary>
    /// <param name="input"> The string to parse. </param>
    /// <returns> The equivalent numeric value of the given string. </returns>
    /// <remarks> Leading whitespace is ignored.  Parsing continues until the first invalid
    /// character, at which point parsing stops.  No error is returned in this case. </remarks>
    [JSInternalFunction(Name = "parseFloat")]
    public static double ParseFloat(string input)
    {
      return NumberParser.ParseFloat(input);
    }

    /// <summary>
    /// Parses the given string and returns the equivalent integer value. 
    /// </summary>
    /// <param name="engine"> The associated script engine. </param>
    /// <param name="input"> The string to parse. </param>
    /// <param name="radixArg"> The numeric base to use for parsing.  Pass zero to use base 10
    /// except when the input string starts with '0' in which case base 16 or base 8 are used
    /// instead (base 8 is only supported in compatibility mode). </param>
    /// <returns> The equivalent integer value of the given string. </returns>
    /// <remarks> Leading whitespace is ignored.  Parsing continues until the first invalid
    /// character, at which point parsing stops.  No error is returned in this case. </remarks>
    [JSInternalFunction(Name = "parseInt", Flags = JSFunctionFlags.HasEngineParameter)]
    public static double ParseInt(ScriptEngine engine, string input, [DefaultParameterValue(0.0)] object radixArg)
    {
      var radix = JurassicHelper.GetTypedArgumentValue(engine, radixArg, 0.0);

      // Check for a valid radix.
      // Note: this is the only function that uses TypeConverter.ToInt32() for parameter
      // conversion (as opposed to the normal method which is TypeConverter.ToInteger() so
      // the radix parameter must be converted to an integer in code.
      int radix2 = TypeConverter.ToInt32(radix);
      if (radix2 < 0 || radix2 == 1 || radix2 > 36)
        return double.NaN;

      return NumberParser.ParseInt(input, radix2, engine.CompatibilityMode == CompatibilityMode.ECMAScript3);
    }

    /// <summary>
    /// Decodes a string that has been encoded using escape().
    /// </summary>
    /// <param name="input"> The string to decode. </param>
    /// <returns> A copy of the given string with the escape sequences decoded. </returns>
    [JSInternalFunction(Deprecated = true, Name = "unescape")]
    public static string Unescape(string input)
    {
      var result = new StringBuilder(input.Length);
      for (int i = 0; i < input.Length; i++)
      {
        char c = input[i];
        if (c == '%')
        {
          // Make sure the string is long enough.
          if (i == input.Length - 1)
            break;

          if (input[i + 1] == 'u')
          {
            // 4 digit escape sequence %uXXXX.
            int value = ParseHexNumber(input, i + 2, 4);
            if (value < 0)
            {
              result.Append('%');
              continue;
            }
            result.Append((char)value);
            i += 5;
          }
          else
          {
            // 2 digit escape sequence %XX.
            int value = ParseHexNumber(input, i + 1, 2);
            if (value < 0)
            {
              result.Append('%');
              continue;
            }
            result.Append((char)value);
            i += 2;
          }
        }
        else
          result.Append(c);
      }
      return result.ToString();
    }



    //     PRIVATE IMPLEMENTATION METHODS
    //_________________________________________________________________________________________

    /// <summary>
    /// Decodes a string containing a URI or a portion of a URI.
    /// </summary>
    /// <param name="engine"> The script engine used to create the error objects. </param>
    /// <param name="input"> The string to decode. </param>
    /// <param name="reservedSet"> A string containing the set of characters that should not
    /// be escaped.  Alphanumeric characters should not be included. </param>
    /// <returns> A copy of the given string with the escape sequences decoded. </returns>
    private static string Decode(ScriptEngine engine, string input, bool[] reservedSet)
    {
      var result = new StringBuilder(input.Length);
      for (int i = 0; i < input.Length; i++)
      {
        char c = input[i];
        if (c == '%')
        {
          // 2 digit escape sequence %XX.

          // Decode the %XX encoding.
          int utf8Byte = ParseHexNumber(input, i + 1, 2);
          if (utf8Byte < 0)
            throw new JavaScriptException(engine, "URIError", "URI malformed");
          i += 2;

          // If the high bit is not set, then this is a single byte ASCII character.
          if ((utf8Byte & 0x80) == 0)
          {
            // Decode only if the character is not reserved.
            if (reservedSet[utf8Byte])
            {
              // Leave the escape sequence as is.
              result.Append(input.Substring(i - 2, 3));
            }
            else
            {
              result.Append((char)utf8Byte);
            }
          }
          else
          {
            // Otherwise, this character was encoded to multiple bytes.

            // Check for an invalid UTF-8 start value.
            if (utf8Byte == 0xc0 || utf8Byte == 0xc1)
              throw new JavaScriptException(engine, "URIError", "URI malformed");

            // Count the number of high bits set (this is the number of bytes required for the character).
            int utf8ByteCount = 1;
            for (int j = 6; j >= 0; j--)
            {
              if ((utf8Byte & (1 << j)) != 0)
                utf8ByteCount++;
              else
                break;
            }
            if (utf8ByteCount < 2 || utf8ByteCount > 4)
              throw new JavaScriptException(engine, "URIError", "URI malformed");

            // Read the additional bytes.
            byte[] utf8Bytes = new byte[utf8ByteCount];
            utf8Bytes[0] = (byte)utf8Byte;
            for (int j = 1; j < utf8ByteCount; j++)
            {
              // An additional escape sequence is expected.
              if (i >= input.Length - 1 || input[++i] != '%')
                throw new JavaScriptException(engine, "URIError", "URI malformed");

              // Decode the %XX encoding.
              utf8Byte = ParseHexNumber(input, i + 1, 2);
              if (utf8Byte < 0)
                throw new JavaScriptException(engine, "URIError", "URI malformed");

              // Top two bits must be 10 (i.e. byte must be 10XXXXXX in binary).
              if ((utf8Byte & 0xC0) != 0x80)
                throw new JavaScriptException(engine, "URIError", "URI malformed");

              // Store the byte.
              utf8Bytes[j] = (byte)utf8Byte;

              // Update the character position.
              i += 2;
            }

            // Decode the UTF-8 sequence.
            result.Append(System.Text.Encoding.UTF8.GetString(utf8Bytes, 0, utf8Bytes.Length));
          }
        }
        else
          result.Append(c);
      }
      return result.ToString();
    }

    /// <summary>
    /// Encodes a string containing a URI or a portion of a URI.
    /// </summary>
    /// <param name="engine"> The associated script engine. </param>
    /// <param name="input"> The string to encode. </param>
    /// <param name="unescapedSet"> An array containing the set of characters that should not
    /// be escaped. </param>
    /// <returns> A copy of the given URI with the special characters encoded. </returns>
    private static string Encode(ScriptEngine engine, string input, bool[] unescapedSet)
    {
      var result = new StringBuilder(input.Length);
      for (int i = 0; i < input.Length; i++)
      {
        // Get the next character in the string.  This might be half of a surrogate pair.
        int c = input[i];
        if (c >= 0xD800 && c < 0xE000)
        {
          // The character is a surrogate pair.

          // Surrogate pairs need to advance an extra character position.
          i++;

          // Compute the code point.
          if (c >= 0xDC00)
            throw new JavaScriptException(engine, "URIError", "URI malformed");
          if (i == input.Length)
            throw new JavaScriptException(engine, "URIError", "URI malformed");
          int c2 = input[i];
          if (c2 < 0xDC00 || c2 >= 0xE000)
            throw new JavaScriptException(engine, "URIError", "URI malformed");
          c = (c - 0xD800) * 0x400 + (c2 - 0xDC00) + 0x10000;
        }

        if (c < 128 && unescapedSet[c])
        {
          // Character should not be escaped.
          result.Append((char)c);
        }
        else
        {
          // Character should be escaped.
          if (c < 0x80)
          {
            // Encodes to a single byte.
            result.AppendFormat("%{0:X2}", c);
          }
          else if (c < 0x800)
          {
            // Encodes to two bytes.
            result.AppendFormat("%{0:X2}", 0xC0 | (c >> 6));
            result.AppendFormat("%{0:X2}", 0x80 | (c & 0x3F));
          }
          else if (c < 0x10000)
          {
            // Encodes to three bytes.
            result.AppendFormat("%{0:X2}", 0xE0 | (c >> 12));
            result.AppendFormat("%{0:X2}", 0x80 | ((c >> 6) & 0x3F));
            result.AppendFormat("%{0:X2}", 0x80 | (c & 0x3F));
          }
          else
          {
            // Encodes to four bytes.
            result.AppendFormat("%{0:X2}", 0xF0 | (c >> 18));
            result.AppendFormat("%{0:X2}", 0x80 | ((c >> 12) & 0x3F));
            result.AppendFormat("%{0:X2}", 0x80 | ((c >> 6) & 0x3F));
            result.AppendFormat("%{0:X2}", 0x80 | (c & 0x3F));
          }
        }
      }
      return result.ToString();
    }

    /// <summary>
    /// Parses a hexidecimal number from within a string.
    /// </summary>
    /// <param name="input"> The string containing the hexidecimal number. </param>
    /// <param name="start"> The start index of the hexidecimal number. </param>
    /// <param name="length"> The number of characters in the hexidecimal number. </param>
    /// <returns> The numeric value of the hexidecimal number, or <c>-1</c> if the number
    /// is not valid. </returns>
    private static int ParseHexNumber(string input, int start, int length)
    {
      if (start + length > input.Length)
        return -1;
      int result = 0;
      for (int i = start; i < start + length; i++)
      {
        result *= 0x10;
        char c = input[i];
        if (c >= '0' && c <= '9')
          result += c - '0';
        else if (c >= 'A' && c <= 'F')
          result += c - 'A' + 10;
        else if (c >= 'a' && c <= 'f')
          result += c - 'a' + 10;
        else
          return -1;
      }
      return result;
    }

    /// <summary>
    /// Creates a 128 entry lookup table for the characters in the given string.
    /// </summary>
    /// <param name="characters"> The characters to include in the set. </param>
    /// <returns> An array containing <c>true</c> for each character in the set. </returns>
    private static bool[] CreateCharacterSetLookupTable(string characters)
    {
      var result = new bool[128];
      for (int i = 0; i < characters.Length; i++)
      {
        char c = characters[i];
        if (c >= 128)
          throw new ArgumentException(@"Characters must be ASCII.", "characters");
        result[c] = true;
      }
      return result;
    }
  }
}
