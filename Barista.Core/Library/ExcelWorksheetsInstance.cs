namespace Barista.Library
{
  using System.Collections.Generic;
  using System.Globalization;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;

  [Serializable]
  public class ExcelWorksheetsConstructor : ClrFunction
  {
    public ExcelWorksheetsConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelWorksheets", new ExcelWorksheetsInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelWorksheetsInstance Construct()
    {
      return new ExcelWorksheetsInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelWorksheetsInstance : ObjectInstance
  {
    private readonly ExcelWorksheets m_excelWorksheets;

    public ExcelWorksheetsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelWorksheetsInstance(ObjectInstance prototype, ExcelWorksheets excelWorksheets)
      : this(prototype)
    {
      if (excelWorksheets == null)
        throw new ArgumentNullException("excelWorksheets");

      m_excelWorksheets = excelWorksheets;
    }

    public ExcelWorksheets ExcelWorksheets
    {
      get { return m_excelWorksheets; }
    }

    public override PropertyDescriptor GetOwnPropertyDescriptor(uint index)
    {
      return new PropertyDescriptor(new ExcelWorksheetInstance(this.Engine.Object.InstancePrototype, m_excelWorksheets[(int)index + 1]), PropertyAttributes.FullAccess);
    }

    /// <summary>
    /// Gets an enumerable list of every property name and value associated with this object.
    /// </summary>
    public override IEnumerable<PropertyNameAndValue> Properties
    {
      get
      {
        for (var i = 0; i < m_excelWorksheets.Count; i++)
        {
          yield return
            new PropertyNameAndValue(i.ToString(CultureInfo.InvariantCulture),
                                     new PropertyDescriptor(
                                       new ExcelWorksheetInstance(this.Engine.Object.InstancePrototype,
                                                                  m_excelWorksheets[i + 1]),
                                       PropertyAttributes.FullAccess));
        }
        
        // Delegate to the base implementation.
        foreach (var nameAndValue in base.Properties)
          yield return nameAndValue;
      }
    }

    #region Functions
    [JSFunction(Name = "add")]
    [JSDoc("Adds a new, blank worksheet.")]
    public ExcelWorksheetInstance Add(string name)
    {
      var result = m_excelWorksheets.Add(name);
      return new ExcelWorksheetInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "addExisting")]
    [JSDoc("Adds a copy of an existing worksheet.")]
    public ExcelWorksheetInstance AddExisting(string name, ExcelWorksheetInstance worksheet)
    {
      var result = m_excelWorksheets.Add(name, worksheet.ExcelWorksheet);
      return new ExcelWorksheetInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "copy")]
    public ExcelWorksheetInstance Copy(string name, string newName)
    {
      var result = m_excelWorksheets.Copy(name, newName);
      return new ExcelWorksheetInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var sheet in this.m_excelWorksheets)
        ArrayInstance.Push(result, new ExcelWorksheetInstance(this.Engine.Object.InstancePrototype, sheet));
      return result;
    }

    [JSFunction(Name = "getWorksheet")]
    public ExcelWorksheetInstance GetWorksheet(object sheet)
    {
      if (TypeUtilities.IsString(sheet))
        return new ExcelWorksheetInstance(this.Engine.Object.InstancePrototype, m_excelWorksheets[TypeConverter.ToString(sheet)]);
      
      if (TypeUtilities.IsNumeric(sheet))
        return new ExcelWorksheetInstance(this.Engine.Object.InstancePrototype, m_excelWorksheets[TypeConverter.ToInteger(sheet)]);

      return null;
    }

    [JSFunction(Name = "delete")]
    public void Delete(object worksheet)
    {
      if (TypeUtilities.IsString(worksheet))
        m_excelWorksheets.Delete(TypeConverter.ToString(worksheet));
      else if (TypeUtilities.IsNumeric(worksheet))
        m_excelWorksheets.Delete(TypeConverter.ToInteger(worksheet));
      else if (worksheet is ExcelWorksheetInstance)
        m_excelWorksheets.Delete((worksheet as ExcelWorksheetInstance).ExcelWorksheet);
      else
      {
        throw new JavaScriptException(this.Engine, "Error", "Could not delete a worksheet -- did not understand the argument type.");
      }
    }

    [JSFunction(Name = "moveAfter")]
    public void MoveAfter(object source, object target)
    {
      if (TypeUtilities.IsNumeric(source) && TypeUtilities.IsNumeric(target))
        m_excelWorksheets.MoveAfter(TypeConverter.ToInteger(source), TypeConverter.ToInteger(target));
      else if (TypeUtilities.IsString(source) && TypeUtilities.IsString(target))
        m_excelWorksheets.MoveAfter(TypeConverter.ToString(source), TypeConverter.ToString(target));
      else
      {
        throw new JavaScriptException(this.Engine, "Error", "Could not move worksheet -- did not understand the argument types.");
      }
    }

    [JSFunction(Name = "moveBefore")]
    public void MoveBefore(object source, object target)
    {
      if (TypeUtilities.IsNumeric(source) && TypeUtilities.IsNumeric(target))
        m_excelWorksheets.MoveBefore(TypeConverter.ToInteger(source), TypeConverter.ToInteger(target));
      else if (TypeUtilities.IsString(source) && TypeUtilities.IsString(target))
        m_excelWorksheets.MoveBefore(TypeConverter.ToString(source), TypeConverter.ToString(target));
      else
      {
        throw new JavaScriptException(this.Engine, "Error", "Could not move worksheet -- did not understand the argument types.");
      }
    }

    [JSFunction(Name = "moveToEnd")]
    public void MoveToEnd(object source)
    {
      if (TypeUtilities.IsNumeric(source))
        m_excelWorksheets.MoveToEnd(TypeConverter.ToInteger(source));
      else if (TypeUtilities.IsString(source))
        m_excelWorksheets.MoveToEnd(TypeConverter.ToString(source));
      else
      {
        throw new JavaScriptException(this.Engine, "Error", "Could not move worksheet -- did not understand the argument type.");
      }
    }

    [JSFunction(Name = "moveToStart")]
    public void MoveToStart(object source)
    {
      if (TypeUtilities.IsNumeric(source))
        m_excelWorksheets.MoveToStart(TypeConverter.ToInteger(source));
      else if (TypeUtilities.IsString(source))
        m_excelWorksheets.MoveToStart(TypeConverter.ToString(source));
      else
      {
        throw new JavaScriptException(this.Engine, "Error", "Could not move worksheet -- did not understand the argument type.");
      }
    }
    #endregion
  }
}
