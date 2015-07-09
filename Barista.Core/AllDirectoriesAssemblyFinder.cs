namespace Barista
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Ninject.Extensions.Conventions.BindingBuilder;
    using Ninject.Modules;
    using Barista.Extensions;

    public class AllDirectoriesAssemblyFinder : IAssemblyFinder
    {
        /// <summary>
        /// Retrieves the name of an assembly form its file name.
        /// </summary>
        private readonly IAssemblyNameRetriever m_assemblyNameRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyFinder"/> class.
        /// </summary>
        /// <param name="assemblyNameRetriever">The assembly name retriever.</param>
        public AllDirectoriesAssemblyFinder(IAssemblyNameRetriever assemblyNameRetriever)
        {
            m_assemblyNameRetriever = assemblyNameRetriever;
        }

        public IEnumerable<Assembly> FindAssemblies(IEnumerable<string> assemblies, Predicate<Assembly> filter)
        {
            return m_assemblyNameRetriever
                .GetAssemblyNames(assemblies, filter)
                .Select<AssemblyName, Assembly>(Assembly.Load);
        }

        public IEnumerable<string> FindAssembliesInPath(string path)
        {
            var root = new DirectoryInfo (path);
            return root.EnumerateAllFiles().Where(IsAssemblyFile).Select(f => f.FullName);
        }

        public IEnumerable<string> FindAssembliesMatching(IEnumerable<string> patterns)
        {
            return patterns.SelectMany<string, string>(GetFilesMatchingPattern).Where(IsAssemblyFile);
        }

        private static bool IsAssemblyFile(FileInfo file)
        {
            var extension = file.Extension;
            return HasAssemblyExtension(extension);
        }

        private static bool IsAssemblyFile(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return HasAssemblyExtension(extension);
        }

        private static bool HasAssemblyExtension(string extension)
        {
            const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
            return string.Equals(extension, ".dll", comparison) ||
                   string.Equals(extension, ".exe", comparison);
        }

        private static IEnumerable<string> GetFilesMatchingPattern(string pattern)
        {
            var path = NormalizePath(Path.GetDirectoryName(pattern));
            var glob = Path.GetFileName(pattern);

            if (glob == null)
                throw new InvalidOperationException("pattern is null.");

            return Directory.GetFiles(path, glob);
        }

        private static string NormalizePath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(GetBaseDirectory(), path);
            }

            return Path.GetFullPath(path);
        }

        private static string GetBaseDirectory()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string searchPath = AppDomain.CurrentDomain.RelativeSearchPath;

            return string.IsNullOrEmpty(searchPath) ? baseDirectory : Path.Combine(baseDirectory, searchPath);
        }
    }
}
