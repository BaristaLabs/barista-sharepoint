namespace Barista.TuesPechkin
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    [Serializable]
    public abstract class EmbeddedDeployment : IDeployment
    {
        public virtual string Path
        {
            get
            {
                var path = System.IO.Path.Combine(Physical.Path, PathModifier ?? string.Empty);

                if (!m_deployed)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    foreach (var nameAndContents in GetContents())
                    {
                        var filename = System.IO.Path.Combine(path, nameAndContents.Key);

                        if (!File.Exists(filename))
                        {
                            WriteStreamToFile(filename, nameAndContents.Value);
                        }
                    }

                    m_deployed = true;
                }

                // In 2.2.0 needs to return path
                return Physical.Path;
            }
        }

        // Embedded deployments need to override this instead in 2.2.0
        protected virtual string PathModifier
        {
            get
            {
                return GetType().Assembly.GetName().Version.ToString();
            }
        }

        protected EmbeddedDeployment(IDeployment physical)
        {
            if (physical == null)
            {
                throw new ArgumentException("physical");
            }

            Physical = physical;
        }

        protected IDeployment Physical;

        protected abstract IEnumerable<KeyValuePair<string, Stream>> GetContents();

        private bool m_deployed;

        private void WriteStreamToFile(string fileName, Stream stream)
        {
            if (!File.Exists(fileName))
            {
                var writeBuffer = new byte[8192];

                using (var newFile = File.Open(fileName, FileMode.Create))
                {
                    int writeLength;
                    while ((writeLength = stream.Read(writeBuffer, 0, writeBuffer.Length)) > 0)
                    {
                        newFile.Write(writeBuffer, 0, writeLength);
                    }
                }
            }
        }
    }
}
