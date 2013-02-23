namespace Jurassic.Library
{
  using System;
  using Barista;

  /// <summary>
  /// Represents a constructor for one of the error types: Error, RangeError, SyntaxError, etc.
  /// </summary>
  [Serializable]
  public class ErrorConstructor : ClrFunction
  {

    //     INITIALIZATION
    //_________________________________________________________________________________________

    /// <summary>
    /// Creates a new derived error function.
    /// </summary>
    /// <param name="prototype"> The next object in the prototype chain. </param>
    /// <param name="typeName"> The name of the error object, e.g. "Error", "RangeError", etc. </param>
    internal ErrorConstructor(ObjectInstance prototype, string typeName)
      : base(prototype, typeName, GetInstancePrototype(prototype.Engine, typeName))
    {
    }

    /// <summary>
    /// Determine the instance prototype for the given error type.
    /// </summary>
    /// <param name="engine"> The script engine associated with this object. </param>
    /// <param name="typeName"> The name of the error object, e.g. "Error", "RangeError", etc. </param>
    /// <returns> The instance prototype. </returns>
    private static ObjectInstance GetInstancePrototype(ScriptEngine engine, string typeName)
    {
      return typeName == "Error"
        ? new ErrorInstance(engine.Object.InstancePrototype, typeName, string.Empty)
        : new ErrorInstance(engine.Error.InstancePrototype, typeName, string.Empty);

      // This constructor is for derived Error objects like RangeError, etc.
      // Prototype chain: XXXError instance -> XXXError prototype -> Error prototype -> Object prototype
    }


    //     JAVASCRIPT INTERNAL FUNCTIONS
    //_________________________________________________________________________________________

    /// <summary>
    /// Called when the Error object is invoked like a function, e.g. var x = Error("oh no").
    /// Creates a new derived error instance with the given message.
    /// </summary>
    /// <param name="messageArg"> A description of the error. </param>
    [JSCallFunction]
    public ErrorInstance Call([DefaultParameterValue("")] object messageArg)
    {
      var message = JurassicHelper.GetTypedArgumentValue(this.Engine, messageArg, "");
      return new ErrorInstance(this.InstancePrototype, null, message);
    }

    /// <summary>
    /// Creates a new derived error instance with the given message.
    /// </summary>
    /// <param name="messageArg"> A description of the error. </param>
    [JSConstructorFunction]
    public ErrorInstance Construct([DefaultParameterValue("")] object messageArg)
    {
      var message = JurassicHelper.GetTypedArgumentValue(this.Engine, messageArg, "");
      return new ErrorInstance(this.InstancePrototype, null, message);
    }

  }
}
