namespace Barista.TuesPechkin
{
    public interface IDeployment
    {
        /// <summary>
        /// Represent a path to a folder that contains the wkhtmltox.dll 
        /// library and any dependencies it may have.
        /// </summary>
        string Path { get; }
    }
}
