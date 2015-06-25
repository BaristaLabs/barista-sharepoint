namespace Barista.DocumentStore.Linq.Csv
{
  public class DataRowItem
  {
    private readonly string m_value;
    private readonly int m_lineNbr;

    public DataRowItem(string value, int lineNbr)
    {
      m_value = value;
      m_lineNbr = lineNbr;
    }

    public int LineNbr
    {
      get { return m_lineNbr; }
    }

    public string Value
    {
      get { return m_value; }
    }
  }
}
