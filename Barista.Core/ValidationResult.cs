namespace Barista.OrcaDB
{
  using System;
  using System.Collections.Generic;

  public class ValidationResult
  {
    private string _errorMessage;

    private IEnumerable<string> _memberNames;
    public readonly static ValidationResult Success;

    public string ErrorMessage
    {
      get
      {
        return this._errorMessage;
      }
      set
      {
        this._errorMessage = value;
      }
    }

    public IEnumerable<string> MemberNames
    {
      get
      {
        return this._memberNames;
      }
    }

    public ValidationResult(string errorMessage)
      : this(errorMessage, null)
    {
    }

    public ValidationResult(string errorMessage, IEnumerable<string> memberNames)
    {
      this._errorMessage = errorMessage;
      ValidationResult validationResult = this;
      IEnumerable<string> strs = memberNames;
      IEnumerable<string> strs1 = strs;
      if (strs == null)
      {
        strs1 = (IEnumerable<string>)(new string[0]);
      }
      validationResult._memberNames = strs1;
    }

    protected ValidationResult(ValidationResult validationResult)
    {
      if (validationResult != null)
      {
        this._errorMessage = validationResult._errorMessage;
        this._memberNames = validationResult._memberNames;
        return;
      }
      else
      {
        throw new ArgumentNullException("validationResult");
      }
    }

    public override string ToString()
    {
      string errorMessage = this.ErrorMessage;
      string str = errorMessage;
      if (errorMessage == null)
      {
        str = this.ToString();
      }
      return str;
    }
  }
}
