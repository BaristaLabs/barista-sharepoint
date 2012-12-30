using Jurassic;
namespace Barista.Justache
{
  public class Block : Section
  {
    public Block(string name, params Part[] parts)
      : base(name)
    {
      Load(parts);
    }

    public override void Render(RenderContext context)
    {
      //var lambda = context.GetValue(Name) as Lambda;
      //if (lambda != null)
      //{
      //    context.Write(lambda(InnerSource()).ToString());
      //    return;
      //}

      var currentValue = context.GetValue(Name);
      if (currentValue is bool)
      {
        var shouldRender = (bool) context.GetValue(Name);
        if (shouldRender)
        {
          base.Render(context);
        }
      }
      else if (currentValue == Null.Value || currentValue == Undefined.Value || currentValue == null)
      {
        //Do Nothing.
      }
      else
      {
        foreach (var value in context.GetValues(Name))
        {
          context.Push(this, value);

          base.Render(context);

          context.Pop();
        }
      }
    }

    public override string ToString()
    {
      return string.Format("Block(\"{0}\")", Name);
    }
  }
}