namespace Barista.Extensions
{
    using System;

    using Ninject.Extensions.Conventions.BindingBuilder;
    using Ninject.Extensions.Conventions.BindingGenerators;
    using Ninject.Extensions.Conventions.Syntax;
    using Ninject.Modules;
    using Ninject.Syntax;

    /// <summary>
    /// Provides extensions for the IKernel interface
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IKernelExtensions
    {
        /// <summary>
        /// Creates bindings using conventions
        /// </summary>
        /// <param name="kernel">The kernel for which the bindings are created.</param>
        /// <param name="configure">The binding convention configuration.</param>
        public static void Bind(this IBindingRoot kernel, Action<IFromSyntax> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException("configure");
            }

            var assemblyNameRetriever = new AssemblyNameRetriever();
            try
            {
                var builder = new ConventionSyntax(
                    new ConventionBindingBuilder(kernel, new TypeSelector()),
                    new AllDirectoriesAssemblyFinder(assemblyNameRetriever), 
                    new TypeFilter(),
                    new BindingGeneratorFactory(new BindableTypeSelector()));
                configure(builder);
            }
            finally
            {
                assemblyNameRetriever.Dispose();
            }
        }
    }
}
