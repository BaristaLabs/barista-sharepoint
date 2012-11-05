namespace Barista.OrcaDB.Linq.Csv
{
  using System;

  /// <summary>
  /// Summary description for FieldFormat
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class CsvOutputFormatAttribute : System.Attribute
  {
    private string m_Format = "";
    public string Format
    {
      get { return m_Format; }
      set { m_Format = value; }
    }

    public CsvOutputFormatAttribute(string format)
    {
      m_Format = format;
    }
  }
}
