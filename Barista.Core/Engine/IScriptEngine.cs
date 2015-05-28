namespace Barista.Engine
{
    public interface IScriptEngine
    {
        object Evaluate(IScriptSource script);


        string Stringify(object value, object replacer, object spacer);

        //TODO: Other methods related to loading packages.
    }
}
