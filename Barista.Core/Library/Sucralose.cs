namespace Barista.Library
{
  using System;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.TeamFoundation.Client;

  public static class Sucralose
  {
    [Serializable]
    public sealed class RoundFunctionInstance : FunctionInstance
    {
      public RoundFunctionInstance(ScriptEngine engine, ObjectInstance prototype)
        : base(engine, prototype)
      {
      }

      public override object CallLateBound(object thisObject, params object[] argumentValues)
      {
        var number = TypeConverter.ToNumber(thisObject);

        //No precision specified
        if (argumentValues.Length == 0 || argumentValues[0] == Undefined.Value || argumentValues[0] == Null.Value || argumentValues[0] == null)
        {
          return MathObject.Round(number);
        }

        var precision = TypeConverter.ToNumber(argumentValues[0]);

        var multiplier = Math.Pow(10, Math.Abs(precision));
        if (precision < 0)
          multiplier = 1 / multiplier;
        return MathObject.Round(number * multiplier) / multiplier;
      }
    }

    [Serializable]
    public sealed class CeilFunctionInstance : FunctionInstance
    {
      public CeilFunctionInstance(ScriptEngine engine, ObjectInstance prototype)
        : base(engine, prototype)
      {
      }

      public override object CallLateBound(object thisObject, params object[] argumentValues)
      {
        var number = TypeConverter.ToNumber(thisObject);

        //No precision specified
        if (argumentValues.Length == 0 || argumentValues[0] == Undefined.Value || argumentValues[0] == Null.Value || argumentValues[0] == null)
        {
          return MathObject.Ceil(number);
        }

        var precision = TypeConverter.ToNumber(argumentValues[0]);

        var multiplier = Math.Pow(10, Math.Abs(precision));
        if (precision < 0)
          multiplier = 1 / multiplier;
        return MathObject.Ceil(number * multiplier) / multiplier;
      }
    }

    [Serializable]
    public sealed class FloorFunctionInstance : FunctionInstance
    {
      public FloorFunctionInstance(ScriptEngine engine, ObjectInstance prototype)
        : base(engine, prototype)
      {
      }

      public override object CallLateBound(object thisObject, params object[] argumentValues)
      {
        var number = TypeConverter.ToNumber(thisObject);

        //No precision specified
        if (argumentValues.Length == 0 || argumentValues[0] == Undefined.Value || argumentValues[0] == Null.Value || argumentValues[0] == null)
        {
          return MathObject.Floor(number);
        }

        var precision = TypeConverter.ToNumber(argumentValues[0]);

        var multiplier = Math.Pow(10, Math.Abs(precision));
        if (precision < 0)
          multiplier = 1 / multiplier;
        return MathObject.Floor(number * multiplier) / multiplier;
      }
    }

    [Serializable]
    public sealed class MergeFunctionInstance : FunctionInstance
    {
      public MergeFunctionInstance(ScriptEngine engine, ObjectInstance prototype)
        : base(engine, prototype)
      {
      }

      public override object CallLateBound(object thisObject, params object[] argumentValues)
      {
        var target = argumentValues.Length >= 1 ? argumentValues[0] : this.Engine.Object.Construct();
        var source = argumentValues.Length >= 2 ? argumentValues[1] : null;
        var deep = argumentValues.Length >= 3 && TypeConverter.ToBoolean(argumentValues[2]);
        var resolve = argumentValues.Length >= 4 ? argumentValues[3] : true;

        if (target == null || target == Null.Value || target == Undefined.Value)
            target = this.Engine.Object.Construct();

        if (TypeConverter.ToBoolean(target) && TypeUtilities.IsString(source) == false && source != null && source != Null.Value && source != Undefined.Value)
        {
          var sourceObj = TypeConverter.ToObject(this.Engine, source);
          var targetObj = TypeConverter.ToObject(this.Engine, target);

          foreach (var kvp in sourceObj.Properties)
          {
            if (!ObjectInstance.HasOwnProperty(this.Engine, source, kvp.Name) || !TypeConverter.ToBoolean(target))
              continue;

            var val = kvp.Value;
            var goDeep = deep && JurassicHelper.IsObjectType(val);

            //Conflict!
            if (targetObj.HasProperty(kvp.Name) && targetObj[kvp.Name] != Undefined.Value && (target is ArrayInstance) == false)
            {
              //Do not merge.
              if (TypeConverter.ToBoolean(resolve) == false && !goDeep)
                continue;

               // Use the result of the callback as the result.
              if (resolve is FunctionInstance)
              {
                val = (resolve as FunctionInstance).Call(source, kvp.Name, targetObj[kvp.Name], sourceObj[kvp.Name]);

                //Hack to fix issue with a resolve function returning a double, when the source type is an int.
                if (sourceObj[kvp.Name].GetType() != val.GetType())
                  val = TypeConverter.ConvertTo(this.Engine, val, sourceObj[kvp.Name].GetType());
              }
              else if (TypeConverter.ToBoolean(resolve))
                val = kvp.Value;
            }
           
            // Deep merging.
            if (goDeep)
            {
              if (val is DateInstance)
                val = this.Engine.Date.Construct((val as DateInstance).GetTime());
              else if (val is RegExpInstance)
                val = this.Engine.RegExp.Construct((val as RegExpInstance).Source, (val as RegExpInstance).Flags);
              else
              {
                if (!TypeConverter.ToBoolean(targetObj[kvp.Name]))
                {
                  if (val is ArrayInstance)
                    targetObj[kvp.Name] = this.Engine.Array.Construct();
                  else
                    targetObj[kvp.Name] = this.Engine.Object.Construct();
                }

                CallLateBound(thisObject, targetObj[kvp.Name], sourceObj[kvp.Name], deep, resolve);
                continue;
              }
            }

            if (target is ArrayInstance && kvp.Name == "length")
              continue;

            targetObj.SetPropertyValue(kvp.Name, val, false);
          }

          target = targetObj;
        }

        return target;
      }
    }
  }
}
