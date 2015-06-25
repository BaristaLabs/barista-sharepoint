namespace Barista.Jurassic.Compiler
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Represents information useful for optimizing a method.
  /// </summary>
  internal class MethodOptimizationHints
  {
    private readonly HashSet<string> m_names = new HashSet<string>();
    private bool m_cached, m_hasArguments, m_hasEval, m_hasNestedFunction, m_hasThis;

    /// <summary>
    /// Called by the parser whenever a variable is encountered (variable being any identifier
    /// which is not a property name).
    /// </summary>
    /// <param name="name"> The variable name. </param>
    public void EncounteredVariable(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      this.m_names.Add(name);
      this.m_cached = false;
    }

    /// <summary>
    /// Determines if the parser encountered the given variable name while parsing the
    /// function, or if the function contains a reference to "eval" or the function contains
    /// nested functions which may reference the variable.
    /// </summary>
    /// <param name="name"> The variable name. </param>
    /// <returns> <c>true</c> if the parser encountered the given variable name or "eval" while
    /// parsing the function; <c>false</c> otherwise. </returns>
    public bool HasVariable(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (this.HasEval)
        return true;
      if (this.HasNestedFunction)
        return true;
      return this.m_names.Contains(name);
    }

    /// <summary>
    /// Gets a value that indicates whether the function being generated contains a reference
    /// to the arguments object.
    /// </summary>
    public bool HasArguments
    {
      get
      {
        CacheResults();
        return this.m_hasArguments;
      }
    }

    /// <summary>
    /// Gets a value that indicates whether the function being generated contains an eval
    /// statement.
    /// </summary>
    public bool HasEval
    {
      get
      {
        CacheResults();
        return this.m_hasEval;
      }
    }

    /// <summary>
    /// Caches the HasEval and HasArguments property access.
    /// </summary>
    private void CacheResults()
    {
      if (this.m_cached == false)
      {
        this.m_hasEval = this.HasNestedFunction || this.m_names.Contains("eval");
        this.m_hasArguments = this.m_hasEval || this.m_names.Contains("arguments");
        this.m_cached = true;
      }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the function being generated contains a
    /// nested function declaration or expression.
    /// </summary>
    public bool HasNestedFunction
    {
      get { return this.m_hasNestedFunction; }
      set
      {
        this.m_hasNestedFunction = value;
        this.m_cached = false;
      }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the function being generated contains a
    /// reference to the "this" keyword.
    /// </summary>
    public bool HasThis
    {
      get { return this.m_hasThis || this.HasEval; }
      set { this.m_hasThis = value; }
    }
  }

}
