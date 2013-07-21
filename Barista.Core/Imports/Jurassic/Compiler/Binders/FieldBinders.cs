namespace Barista.Jurassic.Compiler
{
  using System;
  using System.Reflection;

  /// <summary>
  /// Base class of field getter and setter binders.
  /// </summary>
  [Serializable]
  internal abstract class FieldBinder : Binder
  {
    protected FieldInfo Field;

    /// <summary>
    /// Creates a new FieldGetterBinder instance.
    /// </summary>
    /// <param name="field"> The field. </param>
    protected FieldBinder(FieldInfo field)
    {
      this.Field = field;
    }

    //     PROPERTIES
    //_________________________________________________________________________________________

    /// <summary>
    /// Gets the name of the target methods.
    /// </summary>
    public override string Name
    {
      get { return this.Field.Name; }
    }

    /// <summary>
    /// Gets the full name of the target methods, including the type name.
    /// </summary>
    public override string FullName
    {
      get { return string.Format("{0}.{1}", this.Field.DeclaringType, this.Field.Name); }
    }
  }




  /// <summary>
  /// Retrieves the value of a field.
  /// </summary>
  [Serializable]
  internal class FieldGetterBinder : FieldBinder
  {
    /// <summary>
    /// Creates a new FieldGetterBinder instance.
    /// </summary>
    /// <param name="field"> The field. </param>
    public FieldGetterBinder(FieldInfo field)
      : base(field)
    {
    }

    /// <summary>
    /// Generates a method that does type conversion and calls the bound method.
    /// </summary>
    /// <param name="generator"> The ILGenerator used to output the body of the method. </param>
    /// <param name="argumentCount"> The number of arguments that will be passed to the delegate. </param>
    /// <returns> A delegate that does type conversion and calls the method represented by this
    /// object. </returns>
    protected override void GenerateStub(ILGenerator generator, int argumentCount)
    {
      // Check for the correct number of arguments.
      if (argumentCount != 0)
      {
        EmitHelpers.EmitThrow(generator, "TypeError", "Wrong number of arguments");
        EmitHelpers.EmitDefaultValue(generator, PrimitiveType.Any);
        generator.Complete();
        return;
      }

      if (this.Field.IsStatic == false)
      {
        generator.LoadArgument(1);
        ClrBinder.EmitConversionToType(generator, this.Field.DeclaringType, true);
      }
      generator.LoadField(this.Field);
      ClrBinder.EmitConversionToObject(generator, this.Field.FieldType);
      generator.Complete();
    }

  }

  /// <summary>
  /// Sets the value of a field.
  /// </summary>
  [Serializable]
  internal class FieldSetterBinder : FieldBinder
  {
    /// <summary>
    /// Creates a new FieldSetterBinder instance.
    /// </summary>
    /// <param name="field"> The field. </param>
    public FieldSetterBinder(FieldInfo field)
      : base(field)
    {
    }

    /// <summary>
    /// Generates a method that does type conversion and calls the bound method.
    /// </summary>
    /// <param name="generator"> The ILGenerator used to output the body of the method. </param>
    /// <param name="argumentCount"> The number of arguments that will be passed to the delegate. </param>
    /// <returns> A delegate that does type conversion and calls the method represented by this
    /// object. </returns>
    protected override void GenerateStub(ILGenerator generator, int argumentCount)
    {
      // Check for the correct number of arguments.
      if (argumentCount != 1)
      {
        EmitHelpers.EmitThrow(generator, "TypeError", "Wrong number of arguments");
        EmitHelpers.EmitDefaultValue(generator, PrimitiveType.Any);
        generator.Complete();
        return;
      }
      if (this.Field.IsStatic == false)
      {
        generator.LoadArgument(1);
        ClrBinder.EmitConversionToType(generator, this.Field.DeclaringType, true);
      }
      generator.LoadArgument(2);
      generator.LoadInt32(0);
      generator.LoadArrayElement(typeof(object));
      ClrBinder.EmitConversionToType(generator, this.Field.FieldType, false);
      generator.StoreField(this.Field);
      EmitHelpers.EmitUndefined(generator);
      generator.Complete();
    }

  }
}
