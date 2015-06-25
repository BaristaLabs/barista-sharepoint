namespace Barista.Extensions
{
  using Barista.DocumentStore;
  using System;
  using System.ComponentModel.DataAnnotations;

  public static class ValidationAttributeExtensions
  {
    private static readonly object SyncLock = new object();

    public static ValidationResult IsValid(this ValidationAttribute validationAttribute, object value, ValidationContext validationContext)
    {
      ValidationResult validationResult;
      object obj;
      lock (SyncLock)
      {
        ValidationResult success = ValidationResult.Success;
        if (!validationAttribute.IsValid(value))
        {
          if (validationContext.MemberName != null)
          {
            string[] memberName = new string[1];
            memberName[0] = validationContext.MemberName;
            obj = memberName;
          }
          else
          {
            obj = null;
          }
          string[] strArrays = (string[])obj;
          success = new ValidationResult(validationAttribute.FormatErrorMessage(validationContext.DisplayName), strArrays);
        }
        validationResult = success;
      }
      return validationResult;
    }

    public static ValidationResult GetValidationResult(this ValidationAttribute validationAttribute, object value, ValidationContext validationContext)
    {
      bool flag;
      if (validationContext != null)
      {
        ValidationResult validationResult = validationAttribute.IsValid(value, validationContext);
        if (validationResult != null)
        {
          if (validationResult != null)
          {
            flag = !string.IsNullOrEmpty(validationResult.ErrorMessage);
          }
          else
          {
            flag = false;
          }
          bool flag1 = flag;
          if (!flag1)
          {
            string str = validationAttribute.FormatErrorMessage(validationContext.DisplayName);
            validationResult = new ValidationResult(str, validationResult.MemberNames);
          }
        }
        return validationResult;
      }
      else
      {
        throw new ArgumentNullException("validationContext");
      }
    }
  }
}
