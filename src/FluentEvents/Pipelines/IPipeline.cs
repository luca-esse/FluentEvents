using System;
using System.Threading.Tasks;
using FluentEvents.Infrastructure;

namespace FluentEvents.Pipelines
{
    /// <summary>
    ///     This API supports the FluentEvents infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface IPipeline
    {
        /// <summary>
        ///     Adds a module with the associated configuration to the pipeline.
        /// </summary>
        /// <typeparam name="TModule">The type of the module.</typeparam>
        /// <typeparam name="TConfig">The type of the module configuration.</typeparam>
        /// <param name="moduleConfig">An instance of the module configuration</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="moduleConfig"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="PipelineModuleNotFoundException">
        ///     The TModule type wasn't registered in the internal service provider.
        /// </exception>
        void AddModule<TModule, TConfig>(TConfig moduleConfig) where TModule : IPipelineModule<TConfig>;

        
        /// <summary>
        ///     Invokes the modules added with <see cref="AddModule{TModule,TConfig}"/>.
        /// </summary>
        /// <param name="pipelineEvent">The event.</param>
        /// <param name="eventsScope">The scope of the event.</param>
        Task ProcessEventAsync(PipelineEvent pipelineEvent, IEventsScope eventsScope);
    }
}