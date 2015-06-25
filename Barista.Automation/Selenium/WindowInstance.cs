namespace Barista.Automation.Selenium
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using OpenQA.Selenium;
  using System;

  [Serializable]
  public class WindowConstructor : ClrFunction
  {
    public WindowConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Window", new WindowInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public WindowInstance Construct()
    {
      return new WindowInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class WindowInstance : ObjectInstance
  {
    private readonly IWindow m_window;

    public WindowInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public WindowInstance(ObjectInstance prototype, IWindow window)
      : this(prototype)
    {
      if (window == null)
        throw new ArgumentNullException("window");

      m_window = window;
    }

    public IWindow Window
    {
      get { return m_window; }
    }

    [JSProperty(Name = "position")]
    public PointInstance Position
    {
      get
      {
        return new PointInstance(this.Engine.Object.InstancePrototype, m_window.Position);
      }
      set
      {
        if (value == null)
        {
          m_window.Position = System.Drawing.Point.Empty;
          return;
        }

        m_window.Position = value.Point;
      }
    }

    [JSProperty(Name = "size")]
    public SizeInstance Size
    {
      get
      {
        return new SizeInstance(this.Engine.Object.InstancePrototype, m_window.Size);
      }
      set
      {
        if (value == null)
        {
          m_window.Size = System.Drawing.Size.Empty;
          return;
        }

        m_window.Size = value.Size;
      }
    }

    [JSFunction(Name = "maximize")]
    public void Maximize()
    {
      m_window.Maximize();
    }
  }
}
