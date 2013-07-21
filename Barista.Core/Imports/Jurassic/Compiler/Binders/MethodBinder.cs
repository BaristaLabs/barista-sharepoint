namespace Barista.Jurassic.Compiler
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Selects a method from a list of candidates and performs type conversion from actual
  /// argument type to formal argument type.
  /// </summary>
  [Serializable]
  internal abstract class MethodBinder : Binder
  {
    private readonly string m_name;
    private readonly Type m_declaringType;
    private readonly int m_functionLength;

    //     INITIALIZATION
    //_________________________________________________________________________________________

    /// <summary>
    /// Creates a new Binder instance.
    /// </summary>
    /// <param name="targetMethod"> A method to bind to. </param>
    protected MethodBinder(BinderMethod targetMethod)
    {
      if (targetMethod == null)
        throw new ArgumentNullException("targetMethod");
      this.m_name = targetMethod.Name;
      this.m_declaringType = targetMethod.DeclaringType;
      this.m_functionLength = targetMethod.RequiredParameterCount +
          targetMethod.OptionalParameterCount + (targetMethod.HasParamArray ? 1 : 0);
    }

    /// <summary>
    /// Creates a new Binder instance.
    /// </summary>
    /// <param name="targetMethods"> An enumerable list of methods to bind to.  At least one
    /// method must be provided.  Every method must have the same name and declaring type. </param>
    protected MethodBinder(IEnumerable<BinderMethod> targetMethods)
    {
      if (targetMethods == null)
        throw new ArgumentNullException("targetMethods");

      // At least one method must be provided.
      // Every method must have the same name and declaring type.
      foreach (var method in targetMethods)
      {
        if (this.Name == null)
        {
          this.m_name = method.Name;
          this.m_declaringType = method.DeclaringType;
        }
        else
        {
          if (this.Name != method.Name)
            throw new ArgumentException(@"Every method must have the same name.", "targetMethods");
          if (this.m_declaringType != method.DeclaringType)
            throw new ArgumentException(@"Every method must have the same declaring type.", "targetMethods");
        }
        this.m_functionLength = Math.Max(this.FunctionLength, method.RequiredParameterCount +
            method.OptionalParameterCount + (method.HasParamArray ? 1 : 0));
      }
      if (this.Name == null)
        throw new ArgumentException(@"At least one method must be provided.", "targetMethods");
    }

    //     PROPERTIES
    //_________________________________________________________________________________________

    /// <summary>
    /// Gets the name of the target methods.
    /// </summary>
    public override string Name
    {
      get { return this.m_name; }
    }

    /// <summary>
    /// Gets the full name of the target methods, including the type name.
    /// </summary>
    public override string FullName
    {
      get { return string.Format("{0}.{1}", this.m_declaringType, this.m_name); }
    }

    /// <summary>
    /// Gets the maximum number of arguments of any of the target methods.  Used to set the
    /// length property on the function.
    /// </summary>
    public override int FunctionLength
    {
      get { return this.m_functionLength; }
    }
  }
}
