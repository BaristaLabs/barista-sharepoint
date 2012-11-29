namespace Barista.DocumentStore
{
  using System;

  [Flags]
  public enum SchemaOptions
  {
    PreventInvalidData = 0,
    ValidateExisting = 1,
    DeleteExistingNonconformant = 2,
    //Others...
  }
}
