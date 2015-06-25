namespace Barista.Library
{
  using System.Drawing;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class SizeConstructor : ClrFunction
  {
    public SizeConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Size", new SizeInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SizeInstance Construct()
    {
      return new SizeInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SizeInstance : ObjectInstance
  {
    private Size m_size;

    public SizeInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SizeInstance(ObjectInstance prototype, Size size)
      : this(prototype)
    {
      if (size == null)
        throw new ArgumentNullException("size");

      m_size = size;
    }

    public Size Size
    {
      get { return m_size; }
    }

    [JSProperty(Name = "height")]
    public int Height
    {
      get { return m_size.Height; }
      set { m_size.Height = value; }
    }

    [JSProperty(Name = "Width")]
    public int Width
    {
      get { return m_size.Width; }
      set { m_size.Width = value; }
    }

    [JSProperty(Name = "isEmpty")]
    public bool IsEmpty
    {
      get { return m_size.IsEmpty; }
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return m_size.ToString();
    }
  }
}
