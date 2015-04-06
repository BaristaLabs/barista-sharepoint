using Barista.NiL.JS.Core;

namespace Barista.NiL.JS
{
    public enum MessageLevel
    {
        Regular = 0,
        Recomendation,
        Warning,
        CriticalWarning,
        Error
    }

    public delegate void CompilerMessageCallback(MessageLevel level, CodeCoordinates coords, string message);
}
