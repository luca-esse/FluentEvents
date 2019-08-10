using System;
using System.Collections.Generic;
using FluentEvents.Plugins;

namespace FluentEvents.Configuration
{
    /// <summary>
    ///     Provides a way to add or list plugins.
    /// </summary>
    public interface IEventsContextOptions
    {
        /// <summary>
        ///     Adds a plugin.
        /// </summary>
        /// <remarks>
        ///     This method should be called with custom extension methods
        ///     to provide a better configuration experience.
        /// </remarks>
        /// <param name="plugin">The instance of the plugin.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="plugin"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="DuplicatePluginException">
        ///     Another plugin with the same type has already been added.
        /// </exception>
        void AddPlugin(IFluentEventsPlugin plugin);

        /// <summary>
        ///     Returns the list of the added plugins.
        /// </summary>
        IEnumerable<IFluentEventsPlugin> Plugins { get; }
    }
}