namespace Barista
{
    public interface IScriptEngine
    {
        object Evaluate(string code);
    }
}
