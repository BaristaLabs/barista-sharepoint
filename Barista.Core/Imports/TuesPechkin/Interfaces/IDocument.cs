namespace Barista.TuesPechkin
{
    using System.Collections.Generic;

    public interface IDocument : ISettings
    {
        IEnumerable<IObject> GetObjects();
    }
}
