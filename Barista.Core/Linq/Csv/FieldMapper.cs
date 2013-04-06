namespace Barista.DocumentStore.Linq.Csv
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.ComponentModel;
  using System.Reflection;

  /// <summary>
  /// Summary description for FieldMapper
  /// </summary>
  internal class FieldMapper<T>
  {
    protected class TypeFieldInfo : IComparable<TypeFieldInfo>
    {
      public int Index = CsvColumnAttribute.McDefaultFieldIndex;
      public string Name = null;
      public bool CanBeNull = true;
      public NumberStyles InputNumberStyle = NumberStyles.Any;
      public string OutputFormat = null;
      public bool HasColumnAttribute = false;

      public MemberInfo MemberInfo = null;
      public Type FieldType = null;

      // parseNumberMethod will remain null if the property is not a numeric type.
      // This would be the case for DateTime, Boolean, String and custom types.
      // In those cases, just use a TypeConverter.
      //
      // DateTime and Boolean also have Parse methods, but they don't provide
      // functionality that TypeConverter doesn't give you.

      public TypeConverter TypeConverter = null;
      public MethodInfo ParseNumberMethod = null;

      // ----

      public int CompareTo(TypeFieldInfo other)
      {
        return Index.CompareTo(other.Index);
      }
    }

    // -----------------------------

    // IndexToInfo is used to quickly translate the index of a field
    // to its TypeFieldInfo.
    protected TypeFieldInfo[] IndexToInfo = null;

    // Used to build IndexToInfo
    protected Dictionary<string, TypeFieldInfo> NameToInfo = null;

    protected CsvFileDescription FileDescription;

    // Only used when throwing an exception
    protected string FileName;

    // -----------------------------
    // AnalyzeTypeField
    //
    private TypeFieldInfo AnalyzeTypeField(
                            MemberInfo mi,
                            bool allRequiredFieldsMustHaveFieldIndex,
                            bool allCsvColumnFieldsMustHaveFieldIndex)
    {
      TypeFieldInfo tfi = new TypeFieldInfo
      {
        MemberInfo = mi
      };

      if (mi is PropertyInfo)
      {
        tfi.FieldType = ((PropertyInfo)mi).PropertyType;
      }
      else
      {
        tfi.FieldType = ((FieldInfo)mi).FieldType;
      }

      // parseNumberMethod will remain null if the property is not a numeric type.
      // This would be the case for DateTime, Boolean, String and custom types.
      // In those cases, just use a TypeConverter.
      //
      // DateTime and Boolean also have Parse methods, but they don't provide
      // functionality that TypeConverter doesn't give you.

      tfi.ParseNumberMethod =
          tfi.FieldType.GetMethod("Parse",
              new[] { typeof(String), typeof(NumberStyles), typeof(IFormatProvider) });

      tfi.TypeConverter = null;
      if (tfi.ParseNumberMethod == null)
      {
        tfi.TypeConverter =
            TypeDescriptor.GetConverter(tfi.FieldType);
      }

      // -----
      // Process the attributes

      tfi.Index = CsvColumnAttribute.McDefaultFieldIndex;
      tfi.Name = mi.Name;
      tfi.InputNumberStyle = NumberStyles.Any;
      tfi.OutputFormat = "";
      tfi.HasColumnAttribute = false;

      foreach (Object attribute in mi.GetCustomAttributes(typeof(CsvColumnAttribute), true))
      {
        CsvColumnAttribute cca = (CsvColumnAttribute)attribute;

        if (!string.IsNullOrEmpty(cca.Name))
        {
          tfi.Name = cca.Name;
        }

        tfi.Index = cca.FieldIndex;
        tfi.HasColumnAttribute = true;
        tfi.CanBeNull = cca.CanBeNull;
        tfi.OutputFormat = cca.OutputFormat;
        tfi.InputNumberStyle = cca.NumberStyle;
      }

      // -----

      if (allCsvColumnFieldsMustHaveFieldIndex &&
          tfi.HasColumnAttribute &&
          tfi.Index == CsvColumnAttribute.McDefaultFieldIndex)
      {
        throw new ToBeWrittenButMissingFieldIndexException(
                        typeof(T).ToString(),
                        tfi.Name);
      }

      if (allRequiredFieldsMustHaveFieldIndex &&
          (!tfi.CanBeNull) &&
          (tfi.Index == CsvColumnAttribute.McDefaultFieldIndex))
      {
        throw new RequiredButMissingFieldIndexException(
                        typeof(T).ToString(),
                        tfi.Name);
      }

      // -----

      return tfi;
    }

    // -----------------------------
    // AnalyzeType
    //
    protected void AnalyzeType(
                    Type type,
                    bool allRequiredFieldsMustHaveFieldIndex,
                    bool allCsvColumnFieldsMustHaveFieldIndex)
    {
      NameToInfo.Clear();

      // ------
      // Initialize NameToInfo

      foreach (MemberInfo mi in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
      {
        // Only process field and property members.
        if ((mi.MemberType == MemberTypes.Field) ||
            (mi.MemberType == MemberTypes.Property))
        {
          // Note that the compiler does not allow fields and/or properties
          // with the same name as some other field or property.
          TypeFieldInfo tfi =
              AnalyzeTypeField(
                      mi,
                      allRequiredFieldsMustHaveFieldIndex,
                      allCsvColumnFieldsMustHaveFieldIndex);

          NameToInfo[tfi.Name] = tfi;
        }
      }

      // -------
      // Initialize IndexToInfo

      int nbrTypeFields = NameToInfo.Keys.Count;
      IndexToInfo = new TypeFieldInfo[nbrTypeFields];

      int i = 0;
      foreach (KeyValuePair<string, TypeFieldInfo> kvp in NameToInfo)
      {
        IndexToInfo[i++] = kvp.Value;
      }

      // Sort by FieldIndex. Fields without FieldIndex will 
      // be sorted towards the back, because their FieldIndex
      // is Int32.MaxValue.
      //
      // The sort order is important when reading a file that 
      // doesn't have the field names in the first line, and when
      // writing a file. 
      //
      // Note that for reading from a file with field names in the 
      // first line, method ReadNames reworks IndexToInfo.

      Array.Sort(IndexToInfo);

      // ----------
      // Make sure there are no duplicate FieldIndices.
      // However, allow gaps in the FieldIndex range, to make it easier to later insert
      // fields in the range.

      int lastFieldIndex = Int32.MinValue;
      string lastName = "";
      foreach (TypeFieldInfo tfi in IndexToInfo)
      {
        if ((tfi.Index == lastFieldIndex) &&
            (tfi.Index != CsvColumnAttribute.McDefaultFieldIndex))
        {
          throw new DuplicateFieldIndexException(
                      typeof(T).ToString(),
                      tfi.Name,
                      lastName,
                      tfi.Index);
        }

        lastFieldIndex = tfi.Index;
        lastName = tfi.Name;
      }
    }

    /// ///////////////////////////////////////////////////////////////////////
    /// FieldMapper
    /// 
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileDescription"></param>
    /// <param name="fileName"></param>
    /// <param name="writingFile"></param>
    public FieldMapper(CsvFileDescription fileDescription, string fileName, bool writingFile)
    {
      if ((!fileDescription.FirstLineHasColumnNames) &&
          (!fileDescription.EnforceCsvColumnAttribute))
      {
        throw new CsvColumnAttributeRequiredException();
      }

      // ---------

      FileDescription = fileDescription;
      FileName = fileName;

      NameToInfo = new Dictionary<string, TypeFieldInfo>();

      AnalyzeType(
          typeof(T),
          !fileDescription.FirstLineHasColumnNames,
          writingFile && !fileDescription.FirstLineHasColumnNames);
    }

    /// ///////////////////////////////////////////////////////////////////////
    /// WriteNames
    /// 
    /// <summary>
    /// Writes the field names given in T to row.
    /// </summary>
    /// 
    public void WriteNames(ref List<string> row)
    {
      row.Clear();

      for (int i = 0; i < IndexToInfo.Length; i++)
      {
        TypeFieldInfo tfi = IndexToInfo[i];

        if (FileDescription.EnforceCsvColumnAttribute &&
                (!tfi.HasColumnAttribute))
        {
          continue;
        }

        // ----

        row.Add(tfi.Name);
      }
    }


    /// ///////////////////////////////////////////////////////////////////////
    /// WriteObject
    /// 
    public void WriteObject(T obj, ref List<string> row)
    {
      row.Clear();

      for (int i = 0; i < IndexToInfo.Length; i++)
      {
        TypeFieldInfo tfi = IndexToInfo[i];

        if (FileDescription.EnforceCsvColumnAttribute &&
                (!tfi.HasColumnAttribute))
        {
          continue;
        }

        // ----

        Object objValue = null;

        if (tfi.MemberInfo is PropertyInfo)
        {
          objValue =
              ((PropertyInfo)tfi.MemberInfo).GetValue(obj, null);
        }
        else
        {
          objValue =
              ((FieldInfo)tfi.MemberInfo).GetValue(obj);
        }

        // ------

        string resultString = null;
        if (objValue != null)
        {
          if ((objValue is IFormattable))
          {
            resultString =
                ((IFormattable)objValue).ToString(
                    tfi.OutputFormat,
                    FileDescription.FileCultureInfo);
          }
          else
          {
            resultString = objValue.ToString();
          }
        }

        // -----

        row.Add(resultString);
      }
    }
  }

  /// ///////////////////////////////////////////////////////////////////////
  // To do reading, the object needs to create an object of type T
  // to read the data into. This requires the restriction T : new()
  // However, for writing, you don't want to impose that restriction.
  //
  // So, use FieldMapper (without the restriction) for writing,
  // and derive a FieldMapper_Reading (with restrictions) for reading.
  //
  internal class FieldMapper_Reading<T> : FieldMapper<T> where T : new()
  {
    /// ///////////////////////////////////////////////////////////////////////
    /// FieldMapper
    /// 
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileDescription"></param>
    /// <param name="fileName"></param>
    /// <param name="writingFile"></param>
    public FieldMapper_Reading(
                CsvFileDescription fileDescription,
                string fileName,
                bool writingFile)
      : base(fileDescription, fileName, writingFile)
    {
    }


    /// ///////////////////////////////////////////////////////////////////////
    /// ReadNames
    /// 
    /// <summary>
    /// Assumes that the fields in parameter row are field names.
    /// Reads the names into the objects internal structure.
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public void ReadNames(IDataRow row)
    {
      // It is now the order of the field names that determines
      // the order of the elements in m_IndexToInfo, instead of
      // the FieldIndex fields.

      // If there are more names in the file then fields in the type,
      // one of the names will not be found, causing an exception.

      for (int i = 0; i < row.Count; i++)
      {
        if (!NameToInfo.ContainsKey(row[i].Value))
        {
          // name not found
          throw new NameNotInTypeException(typeof(T).ToString(), row[i].Value, FileName);
        }

        // ----

        IndexToInfo[i] = NameToInfo[row[i].Value];

        if (FileDescription.EnforceCsvColumnAttribute &&
            (!IndexToInfo[i].HasColumnAttribute))
        {
          // enforcing column attr, but this field/prop has no column attr.
          throw new MissingCsvColumnAttributeException(typeof(T).ToString(), row[i].Value, FileName);
        }
      }
    }

    /// ///////////////////////////////////////////////////////////////////////
    /// ReadObject
    /// 
    /// <summary>
    /// Creates an object of type T from the data in row and returns that object.
    /// 
    /// </summary>
    /// <param name="row"></param>
    /// <param name="ae"></param>
    /// <returns></returns>
    public T ReadObject(IDataRow row, AggregatedException ae)
    {
      if (row.Count > IndexToInfo.Length)
      {
        // Too many fields
        throw new TooManyDataFieldsException(typeof(T).ToString(), row[0].LineNbr, FileName);
      }

      // -----

      T obj = new T();

      for (int i = 0; i < row.Count; i++)
      {
        TypeFieldInfo tfi = IndexToInfo[i];

        if (FileDescription.EnforceCsvColumnAttribute &&
                (!tfi.HasColumnAttribute))
        {
          // enforcing column attr, but this field/prop has no column attr.
          // So there are too many fields in this record.
          throw new TooManyNonCsvColumnDataFieldsException(typeof(T).ToString(), row[i].LineNbr, FileName);
        }

        // -----

        if ((!FileDescription.FirstLineHasColumnNames) &&
                (tfi.Index == CsvColumnAttribute.McDefaultFieldIndex))
        {
          // First line in the file does not have field names, so we're 
          // depending on the FieldIndex of each field in the type
          // to ensure each value is placed in the correct field.
          // However, now hit a field where there is no FieldIndex.
          throw new MissingFieldIndexException(typeof(T).ToString(), row[i].LineNbr, FileName);
        }

        // -----

        // value to put in the object
        string value = row[i].Value;

        if (value == null)
        {
          if (!tfi.CanBeNull)
          {
            ae.AddException(
                new MissingRequiredFieldException(
                        typeof(T).ToString(),
                        tfi.Name,
                        row[i].LineNbr,
                        FileName));
          }
        }
        else
        {
          try
          {
            Object objValue;

            // Normally, either tfi.typeConverter is not null,
            // or tfi.parseNumberMethod is not null. 
            // 
            if (tfi.TypeConverter != null)
            {
              objValue = tfi.TypeConverter.ConvertFromString(
                              null,
                              FileDescription.FileCultureInfo,
                              value);
            }
            else if (tfi.ParseNumberMethod != null)
            {
              objValue =
                  tfi.ParseNumberMethod.Invoke(
                      tfi.FieldType,
                      new Object[] { 
                                    value, 
                                    tfi.InputNumberStyle, 
                                    FileDescription.FileCultureInfo });
            }
            else
            {
              // No TypeConverter and no Parse method available.
              // Try direct approach.
              objValue = value;
            }

            if (tfi.MemberInfo is PropertyInfo)
            {
              ((PropertyInfo)tfi.MemberInfo).SetValue(obj, objValue, null);
            }
            else
            {
              ((FieldInfo)tfi.MemberInfo).SetValue(obj, objValue);
            }
          }
          catch (Exception e)
          {
            if (e is TargetInvocationException)
            {
              e = e.InnerException;
            }

            if (e is FormatException)
            {
              e = new WrongDataFormatException(
                      typeof(T).ToString(),
                      tfi.Name,
                      value,
                      row[i].LineNbr,
                      FileName,
                      e);
            }

            ae.AddException(e);
          }
        }
      }

      // Visit any remaining fields in the type for which no value was given
      // in the data row, to see whether any of those was required.
      // If only looking at fields with CsvColumn attribute, do ignore
      // fields that don't have that attribute.

      for (int i = row.Count; i < IndexToInfo.Length; i++)
      {
        TypeFieldInfo tfi = IndexToInfo[i];

        if (((!FileDescription.EnforceCsvColumnAttribute) ||
             tfi.HasColumnAttribute) &&
            (!tfi.CanBeNull))
        {
          ae.AddException(
              new MissingRequiredFieldException(
                      typeof(T).ToString(),
                      tfi.Name,
                      row[row.Count - 1].LineNbr,
                      FileName));
        }
      }

      return obj;
    }
  }
}
