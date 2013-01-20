namespace Barista.Linq2Rest.Parser
{
  using System;
  using System.Linq.Expressions;
  using System.Web.UI.WebControls;

  /// <summary>
  /// Defines a sort description.
  /// </summary>
  public class SortDescription
  {
    private readonly SortDirection m_direction;
    private readonly Expression m_keySelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="SortDescription"/> class.
    /// </summary>
    /// <param name="keySelector">The function to select the sort key.</param>
    /// <param name="direction">The sort direction.</param>
    public SortDescription(Expression keySelector, SortDirection direction)
    {
      if (keySelector == null)
        throw new ArgumentNullException("keySelector");

      m_keySelector = keySelector;
      m_direction = direction;
    }

    /// <summary>
    /// Gets the sort direction.
    /// </summary>
    public SortDirection Direction
    {
      get { return m_direction; }
    }

    /// <summary>
    /// Gets the key to sort by.
    /// </summary>
    public Expression KeySelector
    {
      get { return m_keySelector; }
    }
  }
}