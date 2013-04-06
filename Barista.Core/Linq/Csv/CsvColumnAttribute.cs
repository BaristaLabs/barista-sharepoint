namespace Barista.DocumentStore.Linq.Csv
{
  using System;
  using System.Globalization;

  /// <summary>
  /// Summary description for CsvColumnAttribute
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
  public class CsvColumnAttribute : Attribute
  {
    internal const int McDefaultFieldIndex = Int32.MaxValue;

    public string Name { get; set; }
    public bool CanBeNull { get; set; }
    public int FieldIndex { get; set; }
    public NumberStyles NumberStyle { get; set; }
    public string OutputFormat { get; set; }

    public CsvColumnAttribute()
    {
      Name = "";
      FieldIndex = McDefaultFieldIndex;
      CanBeNull = true;
      NumberStyle = NumberStyles.Any;
      OutputFormat = "G";
    }

    public CsvColumnAttribute(
                string name,
                int fieldIndex,
                bool canBeNull,
                string outputFormat,
                NumberStyles numberStyle)
    {
      Name = name;
      FieldIndex = fieldIndex;
      CanBeNull = canBeNull;
      NumberStyle = numberStyle;
      OutputFormat = outputFormat;
    }
  }
}
