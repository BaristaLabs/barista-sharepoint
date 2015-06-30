namespace Barista.TuesPechkin
{
    using System;

    [Serializable]
    public sealed class StaticDeployment : IDeployment
    {
        public string Path { get; private set; }

        public StaticDeployment(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            Path = path;
        }
    }
}
