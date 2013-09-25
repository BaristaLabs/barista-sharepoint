namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using System.Drawing;

  [Serializable]
  public class PointConstructor : ClrFunction
  {
    public PointConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Point", new PointInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public PointInstance Construct()
    {
      return new PointInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class PointInstance : ObjectInstance
  {
    private Point m_point;

    public PointInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public PointInstance(ObjectInstance prototype, Point point)
      : this(prototype)
    {
      if (point == null)
        throw new ArgumentNullException("point");

      m_point = point;
    }

    public Point Point
    {
      get { return m_point; }
    }

    [JSProperty(Name = "x")]
    public int X
    {
      get { return m_point.X; }
      set { m_point.X = value; }
    }

    [JSProperty(Name = "y")]
    public int Y
    {
      get { return m_point.Y; }
      set { m_point.Y = value; }
    }

    [JSProperty(Name = "isEmpty")]
    public bool IsEmpty
    {
      get { return m_point.IsEmpty; }
    }

    [JSFunction(Name = "offset")]
    public void Offset(int dx, int dy)
    {
      m_point.Offset(dx, dy);
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return m_point.ToString();
    }
  }
}
