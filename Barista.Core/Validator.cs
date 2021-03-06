﻿namespace Barista.DocumentStore
{
  using Barista.Extensions;
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.ComponentModel.DataAnnotations;
  using System.Linq;

  public static class Validator
  {
    private static ValidationAttributeStore s_store;

    static Validator()
    {
      Validator.s_store = ValidationAttributeStore.Instance;
    }

    private static bool CanBeAssigned(Type destinationType, object value)
    {
      if (destinationType != null)
      {
        if (value != null)
        {
          return destinationType.IsInstanceOfType(value);
        }
        if (!destinationType.IsValueType)
        {
          return true;
        }
        if (!destinationType.IsGenericType)
        {
          return false;
        }
        return destinationType.GetGenericTypeDefinition() == typeof(Nullable<>);
      }
      throw new ArgumentNullException("destinationType");
    }

    internal static ValidationContext CreateValidationContext(object instance, ValidationContext validationContext)
    {
      if (validationContext != null)
      {
        var validationContext1 = new ValidationContext(instance, validationContext, validationContext.Items);
        return validationContext1;
      }
      throw new ArgumentNullException("validationContext");
    }

    private static void EnsureValidPropertyType(string propertyName, Type propertyType, object value)
    {
      if (!Validator.CanBeAssigned(propertyType, value))
      {
        string message = String.Format("The Property Value is of the wrong type: {0} on {1}", propertyType, propertyName);
        throw new ArgumentException(message, "value");
      }
    }

    private static IEnumerable<Validator.ValidationError> GetObjectPropertyValidationErrors(object instance, ValidationContext validationContext, bool validateAllProperties, bool breakOnFirstError)
    {
      ICollection<KeyValuePair<ValidationContext, object>> propertyValues = Validator.GetPropertyValues(instance, validationContext);
      List<Validator.ValidationError> validationErrors = new List<Validator.ValidationError>();
      IEnumerator<KeyValuePair<ValidationContext, object>> enumerator = propertyValues.GetEnumerator();
      using (enumerator)
      {
        do
        {
          if (!enumerator.MoveNext())
          {
            break;
          }
          KeyValuePair<ValidationContext, object> current = enumerator.Current;
          IEnumerable<ValidationAttribute> propertyValidationAttributes = Validator.s_store.GetPropertyValidationAttributes(current.Key);
          if (!validateAllProperties)
          {
            IEnumerable<ValidationAttribute> validationAttributes = propertyValidationAttributes;
            RequiredAttribute requiredAttribute = validationAttributes.FirstOrDefault(a => a is RequiredAttribute) as RequiredAttribute;
            if (requiredAttribute == null)
            {
              continue;
            }
            ValidationResult validationResult = requiredAttribute.GetValidationResult(current.Value, current.Key);
            if (validationResult == ValidationResult.Success)
            {
              continue;
            }
            validationErrors.Add(new Validator.ValidationError(requiredAttribute, current.Value, validationResult));
          }
          else
          {
            validationErrors.AddRange(Validator.GetValidationErrors(current.Value, current.Key, propertyValidationAttributes, breakOnFirstError));
          }
        }
        while (!breakOnFirstError || !validationErrors.Any());
      }
      return validationErrors;
    }

    private static IEnumerable<Validator.ValidationError> GetObjectValidationErrors(object instance, ValidationContext validationContext, bool validateAllProperties, bool breakOnFirstError)
    {
      if (instance != null)
      {
        if (validationContext != null)
        {
          List<Validator.ValidationError> validationErrors = new List<Validator.ValidationError>();
          validationErrors.AddRange(Validator.GetObjectPropertyValidationErrors(instance, validationContext, validateAllProperties, breakOnFirstError));
          if (!validationErrors.Any())
          {
            IEnumerable<ValidationAttribute> typeValidationAttributes = Validator.s_store.GetTypeValidationAttributes(validationContext);
            validationErrors.AddRange(Validator.GetValidationErrors(instance, validationContext, typeValidationAttributes, breakOnFirstError));
            if (!validationErrors.Any())
            {
              //IValidatableObject validatableObject = instance as IValidatableObject;
              //if (validatableObject != null)
              //{
              //    IEnumerable<ValidationResult> validationResults = validatableObject.Validate(validationContext);
              //    IEnumerable<ValidationResult> validationResults1 = validationResults;
              //    foreach (ValidationResult validationResult in validationResults1.Where<ValidationResult>((ValidationResult r) => r != ValidationResult.Success))
              //    {
              //        validationErrors.Add(new Validator.ValidationError(null, instance, validationResult));
              //    }
              //}
              return validationErrors;
            }

            return validationErrors;
          }
          return validationErrors;
        }
        throw new ArgumentNullException("validationContext");
      }
      throw new ArgumentNullException("instance");
    }

    private static ICollection<KeyValuePair<ValidationContext, object>> GetPropertyValues(object instance, ValidationContext validationContext)
    {
      PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(instance);
      List<KeyValuePair<ValidationContext, object>> keyValuePairs = new List<KeyValuePair<ValidationContext, object>>(properties.Count);
      foreach (PropertyDescriptor property in properties)
      {
        ValidationContext name = Validator.CreateValidationContext(instance, validationContext);
        name.MemberName = property.Name;
        if (!Validator.s_store.GetPropertyValidationAttributes(name).Any())
        {
          continue;
        }
        keyValuePairs.Add(new KeyValuePair<ValidationContext, object>(name, property.GetValue(instance)));
      }
      return keyValuePairs;
    }

    private static IEnumerable<Validator.ValidationError> GetValidationErrors(object value, ValidationContext validationContext, IEnumerable<ValidationAttribute> attributes, bool breakOnFirstError)
    {
      if (validationContext != null)
      {
        List<Validator.ValidationError> validationErrors = new List<Validator.ValidationError>();
        IEnumerable<ValidationAttribute> validationAttributes = attributes;
        RequiredAttribute requiredAttribute = validationAttributes.FirstOrDefault<ValidationAttribute>((ValidationAttribute a) => a is RequiredAttribute) as RequiredAttribute;
        Validator.ValidationError validationError;
        if (requiredAttribute == null || Validator.TryValidate(value, validationContext, requiredAttribute, out validationError))
        {
          IEnumerator<ValidationAttribute> enumerator = attributes.GetEnumerator();
          using (enumerator)
          {
            do
            {
            Label0:
              if (!enumerator.MoveNext())
              {
                break;
              }
              ValidationAttribute current = enumerator.Current;
              if (current != requiredAttribute && !Validator.TryValidate(value, validationContext, current, out validationError))
              {
                validationErrors.Add(validationError);
              }
              else
              {
                goto Label0;
              }
            }
            while (!breakOnFirstError);
          }
          return validationErrors;
        }
        validationErrors.Add(validationError);
        return validationErrors;
      }
      throw new ArgumentNullException("validationContext");
    }

    private static bool TryValidate(object value, ValidationContext validationContext, ValidationAttribute attribute, out Validator.ValidationError validationError)
    {
      if (validationContext != null)
      {
        ValidationResult validationResult = attribute.GetValidationResult(value, validationContext);
        if (validationResult == ValidationResult.Success)
        {
          validationError = null;
          return true;
        }
        validationError = new Validator.ValidationError(attribute, value, validationResult);
        return false;
      }
      throw new ArgumentNullException("validationContext");
    }

    public static bool TryValidateObject(object instance, ValidationContext validationContext, ICollection<ValidationResult> validationResults)
    {
      return Validator.TryValidateObject(instance, validationContext, validationResults, false);
    }

    public static bool TryValidateObject(object instance, ValidationContext validationContext, ICollection<ValidationResult> validationResults, bool validateAllProperties)
    {
      bool isValid = true;
      if (instance != null)
      {
        if (validationContext == null || instance == validationContext.ObjectInstance)
        {
          bool flag1 = validationResults == null;
          foreach (Validator.ValidationError objectValidationError in Validator.GetObjectValidationErrors(instance, validationContext, validateAllProperties, flag1))
          {
            isValid = false;
            if (validationResults == null)
            {
              continue;
            }
            validationResults.Add(objectValidationError.ValidationResult);
          }
          return isValid;
        }
        throw new ArgumentException(@"Instance must match validation context instance.", "instance");
      }
      throw new ArgumentNullException("instance");
    }

    public static bool TryValidateProperty(object value, ValidationContext validationContext, ICollection<ValidationResult> validationResults)
    {
      bool isValid = true;
      Type propertyType = Validator.s_store.GetPropertyType(validationContext);
      string memberName = validationContext.MemberName;
      Validator.EnsureValidPropertyType(memberName, propertyType, value);
      bool flag1 = validationResults == null;
      IEnumerable<ValidationAttribute> propertyValidationAttributes = Validator.s_store.GetPropertyValidationAttributes(validationContext);
      foreach (Validator.ValidationError validationError in Validator.GetValidationErrors(value, validationContext, propertyValidationAttributes, flag1))
      {
        isValid = false;
        if (validationResults == null)
        {
          continue;
        }
        validationResults.Add(validationError.ValidationResult);
      }
      return isValid;
    }

    public static bool TryValidateValue(object value, ValidationContext validationContext, ICollection<ValidationResult> validationResults, IEnumerable<ValidationAttribute> validationAttributes)
    {
      bool isValid = true;
      bool breakOnFirstError = validationResults == null;
      foreach (Validator.ValidationError validationError in Validator.GetValidationErrors(value, validationContext, validationAttributes, breakOnFirstError))
      {
        isValid = false;
        if (validationResults == null)
        {
          continue;
        }
        validationResults.Add(validationError.ValidationResult);
      }
      return isValid;
    }

    public static void ValidateObject(object instance, ValidationContext validationContext)
    {
      Validator.ValidateObject(instance, validationContext, false);
    }

    public static void ValidateObject(object instance, ValidationContext validationContext, bool validateAllProperties)
    {
      if (instance != null)
      {
        if (validationContext != null)
        {
          if (instance == validationContext.ObjectInstance)
          {
            Validator.ValidationError validationError = Validator.GetObjectValidationErrors(instance, validationContext, validateAllProperties, false).FirstOrDefault();
            if (validationError != null)
            {
              validationError.ThrowValidationException();
            }
            return;
          }
          throw new ArgumentException(@"Instance Must Match Validation Context Instance.", "instance");
        }
        throw new ArgumentNullException("validationContext");
      }
      throw new ArgumentNullException("instance");
    }

    public static void ValidateProperty(object value, ValidationContext validationContext)
    {
      Type propertyType = Validator.s_store.GetPropertyType(validationContext);
      Validator.EnsureValidPropertyType(validationContext.MemberName, propertyType, value);
      IEnumerable<ValidationAttribute> propertyValidationAttributes = Validator.s_store.GetPropertyValidationAttributes(validationContext);
      Validator.ValidationError validationError = Validator.GetValidationErrors(value, validationContext, propertyValidationAttributes, false).FirstOrDefault();
      if (validationError != null)
      {
        validationError.ThrowValidationException();
      }
    }

    public static void ValidateValue(object value, ValidationContext validationContext, IEnumerable<ValidationAttribute> validationAttributes)
    {
      if (validationContext != null)
      {
        Validator.ValidationError validationError = Validator.GetValidationErrors(value, validationContext, validationAttributes, false).FirstOrDefault();
        if (validationError != null)
        {
          validationError.ThrowValidationException();
        }
      }
      else
      {
        throw new ArgumentNullException("validationContext");
      }
    }

    private class ValidationError
    {
      internal ValidationAttribute ValidationAttribute
      {
        get;
        set;
      }

      internal ValidationResult ValidationResult
      {
        get;
        set;
      }

      internal object Value
      {
        get;
        set;
      }

      internal ValidationError(ValidationAttribute validationAttribute, object value, ValidationResult validationResult)
      {
        this.ValidationAttribute = validationAttribute;
        this.ValidationResult = validationResult;
        this.Value = value;
      }

      internal void ThrowValidationException()
      {
        throw new ValidationException(this.ValidationResult.ErrorMessage, this.ValidationAttribute, this.Value);
      }
    }
  }

}
